import 'package:flutter/material.dart';
import 'services/auth_service.dart'; // Đảm bảo đường dẫn này đúng

class AuthWrapper extends StatefulWidget {
  const AuthWrapper({super.key});

  @override
  State<AuthWrapper> createState() => _AuthWrapperState();
}

class _AuthWrapperState extends State<AuthWrapper> {
  final AuthService _authService = AuthService();

  @override
  void initState() {
    super.initState();
    // Dùng WidgetsBinding để đảm bảo context đã sẵn sàng
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _checkLoginStatus();
    });
  }

  /// Kiểm tra trạng thái đăng nhập và điều hướng
  Future<void> _checkLoginStatus() async {
    // Đợi một chút để màn hình loading hiển thị mượt mà hơn (tùy chọn)
    await Future.delayed(const Duration(milliseconds: 500));

    final bool isLoggedIn = await _authService.isLoggedIn();

    // Luôn kiểm tra widget còn tồn tại không trước khi điều hướng
    if (mounted) {
      if (isLoggedIn) {
        // Nếu đã đăng nhập, chuyển đến trang chủ (NavScreen)
        Navigator.pushReplacementNamed(context, '/nav');
      } else {
        // Nếu chưa, chuyển đến trang đăng nhập
        Navigator.pushReplacementNamed(context, '/login');
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    // Hiển thị màn hình loading trong khi kiểm tra
    return const Scaffold(
      body: Center(
        child: CircularProgressIndicator(),
      ),
    );
  }
}
