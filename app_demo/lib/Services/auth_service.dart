// lib/services/auth_service.dart

import 'dart:convert';
import 'package:google_sign_in/google_sign_in.dart';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
import '../Model/user_model.dart'; // Đảm bảo đường dẫn này đúng

/// AuthService chịu trách nhiệm cho tất cả các hoạt động xác thực người dùng,
/// bao gồm đăng nhập, đăng xuất, và quản lý session cục bộ.
class AuthService {
  /// URL cơ sở của API backend C#.
  /// QUAN TRỌNG:
  /// - Dùng 'http://10.0.2.2:PORT' nếu bạn đang chạy app trên Android Emulator.
  /// - Dùng 'http://localhost:PORT' nếu bạn đang chạy app trên iOS Simulator hoặc web.
  /// - Dùng địa chỉ IP LAN của máy tính (ví dụ: 'http://192.168.1.10:PORT') nếu chạy trên thiết bị thật.
  /// Thay PORT bằng port của backend (ví dụ: 5001, 7209).
  static const String _baseUrl = "http://255.255.240.0:2000";

  final GoogleSignIn _googleSignIn = GoogleSignIn(scopes: ['email', 'profile']);

  /// Đăng nhập bằng tài khoản Google.
  ///
  /// Trả về đối tượng `User` nếu thành công, ngược lại trả về `null`.
  // lib/services/auth_service.dart

  Future<User?> signInWithGoogle() async {
    print("--- BẮT ĐẦU QUÁ TRÌNH ĐĂNG NHẬP GOOGLE ---");
    try {
      // Bước 1: Kiểm tra xem người dùng đã đăng nhập Google trước đó chưa
      final isAlreadySignedIn = await _googleSignIn.isSignedIn();
      if (isAlreadySignedIn) {
        print(
            "Phát hiện người dùng đã đăng nhập Google trước đó. Đang cố gắng đăng xuất để làm mới...");
        await _googleSignIn.signOut();
        print("Đã đăng xuất khỏi Google thành công.");
      }

      print("Bước 1: Đang gọi _googleSignIn.signIn()...");
      // Kích hoạt cửa sổ đăng nhập của Google
      final GoogleSignInAccount? googleUser = await _googleSignIn.signIn();

      // Bước 2: Kiểm tra kết quả từ Google
      if (googleUser == null) {
        // Nếu người dùng hủy, hoặc có lỗi cấu hình ngầm
        print("LỖI hoặc HỦY: _googleSignIn.signIn() trả về null.");
        print("--- KẾT THÚC QUÁ TRÌNH ĐĂNG NHẬP (THẤT BẠI) ---");
        return null;
      }

      print("Bước 2: Thành công! Lấy được thông tin từ Google.");
      print("  - Google ID: ${googleUser.id}");
      print("  - Email: ${googleUser.email}");
      print("  - Tên hiển thị: ${googleUser.displayName}");
      print("  - Ảnh đại diện URL: ${googleUser.photoUrl}");

      // Bước 3: Gửi thông tin tới backend để xử lý
      print(
          "Bước 3: Đang gửi request POST đến backend tại: $_baseUrl/api/auth/process-google-signin");
      final response = await http
          .post(
            Uri.parse('$_baseUrl/api/auth/process-google-signin'),
            headers: {'Content-Type': 'application/json'},
            body: jsonEncode({
              'googleId': googleUser.id,
              'email': googleUser.email,
              'name': googleUser.displayName ?? '',
            }),
          )
          .timeout(
              const Duration(seconds: 60)); // Thêm timeout để tránh chờ vô tận

      // Bước 4: Xử lý phản hồi từ backend
      print(
          "Bước 4: Nhận được phản hồi từ backend với Status Code: ${response.statusCode}");
      if (response.statusCode == 200) {
        print("  - Phản hồi backend (thành công): ${response.body}");
        final User user = userFromJson(response.body);
        await _saveUserData(user); // Lưu thông tin người dùng vào cục bộ
        print("Đăng nhập bằng Google thành công cho user: ${user.name}");
        print("--- KẾT THÚC QUÁ TRÌNH ĐĂNG NHẬP (THÀNH CÔNG) ---");
        return user;
      } else {
        print("  - LỖI Phản hồi backend: ${response.body}");
        print("Đang đăng xuất khỏi Google để tránh kẹt trạng thái...");
        await _googleSignIn.signOut();
        print("--- KẾT THÚC QUÁ TRÌNH ĐĂNG NHẬP (THẤT BẠI) ---");
        return null;
      }
    } catch (e, stackTrace) {
      print("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
      print("LỖI NGOẠI LỆ TRONG signInWithGoogle(): $e");
      print("Stack Trace:\n$stackTrace");
      print("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
      print("--- KẾT THÚC QUÁ TRÌNH ĐĂNG NHẬP (THẤT BẠI) ---");
      // Thử đăng xuất để reset
      await _googleSignIn.signOut().catchError((signOutError) {
        print("Lỗi khi cố gắng đăng xuất sau khi gặp ngoại lệ: $signOutError");
      });
      return null;
    }
  }

  /// Đăng nhập bằng Email và Mật khẩu (Chức năng chờ).
  ///
  /// Trả về đối tượng `User` nếu thành công, ngược lại trả về `null`.
  Future<User?> signInWithEmailPassword(String email, String password) async {
    try {
      final response = await http.post(
        Uri.parse(
          '$_baseUrl/api/auth/login',
        ), // Sử dụng endpoint "login" của bạn
        headers: {'Content-Type': 'application/json'},
        body: jsonEncode({'email': email, 'password': password}),
      );

      if (response.statusCode == 200) {
        final User user = userFromJson(response.body);
        await _saveUserData(user);
        print("Đăng nhập bằng Email/Pass thành công cho user: ${user.name}");
        return user;
      } else {
        print(
          "Lỗi đăng nhập Email/Pass: ${response.statusCode} - ${response.body}",
        );
        return null;
      }
    } catch (e) {
      print("Đã xảy ra lỗi khi đăng nhập bằng Email/Pass: $e");
      return null;
    }
  }

  /// Đăng xuất người dùng.
  /// Xóa dữ liệu khỏi Google Sign-In và SharedPreferences.
  Future<void> signOut() async {
    await _googleSignIn.signOut();
    final prefs = await SharedPreferences.getInstance();
    await prefs.clear();
    print("Người dùng đã đăng xuất.");
  }

  /// Kiểm tra xem người dùng đã đăng nhập hay chưa.
  Future<bool> isLoggedIn() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getBool('isLoggedIn') ?? false;
  }

  /// Lấy thông tin người dùng đã được lưu trong SharedPreferences.
  ///
  /// Trả về `User` nếu tìm thấy, ngược lại `null`.
  Future<User?> getSavedUser() async {
    final prefs = await SharedPreferences.getInstance();
    final userProfileJson = prefs.getString('userProfile');
    if (userProfileJson != null) {
      return userFromJson(userProfileJson);
    }
    return null;
  }

  /// [Hàm nội bộ] Lưu thông tin người dùng vào SharedPreferences.
  Future<void> _saveUserData(User user) async {
    final prefs = await SharedPreferences.getInstance();
    // Lưu trạng thái đăng nhập
    await prefs.setBool('isLoggedIn', true);
    // Lưu toàn bộ đối tượng user dưới dạng chuỗi JSON để dễ dàng truy xuất sau này
    await prefs.setString('userProfile', userToJson(user));
  }
}
