// lib/LoginRegister/login.dart
import 'package:flutter/material.dart';
import '../../services/auth_service.dart'; // Đảm bảo đường dẫn này chính xác

class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final TextEditingController _emailController = TextEditingController();
  final TextEditingController _passwordController = TextEditingController();
  final AuthService _authService = AuthService();
  bool _isLoading = false;

  /// Xử lý logic đăng nhập bằng Email/Mật khẩu
  void handleLogin() async {
    // TODO: Triển khai logic gọi API đăng nhập bằng email/pass
    // Ví dụ:
    // setState(() { _isLoading = true; });
    // final user = await _authService.signInWithEmailPassword(
    //   _emailController.text,
    //   _passwordController.text,
    // );
    // setState(() { _isLoading = false; });
    // if (user != null && mounted) {
    //   Navigator.pushReplacementNamed(context, '/nav');
    // } else {
    //   // Hiển thị lỗi
    // }

    // Tạm thời chuyển trang
    Navigator.pushReplacementNamed(context, '/nav');
  }

  /// Chuyển đến trang Đăng ký
  void handleRegisterPress() {
    Navigator.pushNamed(context, '/register');
  }

  /// Chuyển đến trang Quên mật khẩu
  void handleForgotPassPress() {
    // TODO: Triển khai logic chuyển đến trang quên mật khẩu
  }

  /// Xử lý logic đăng nhập bằng Google
  void handleGoogleSignIn() async {
    // Ngăn người dùng nhấn nút nhiều lần khi đang xử lý
    if (_isLoading) return;

    setState(() {
      _isLoading = true;
    });

    final user = await _authService.signInWithGoogle();

    // Kiểm tra xem widget có còn tồn tại không trước khi cập nhật UI
    if (mounted) {
      setState(() {
        _isLoading = false;
      });

      if (user != null) {
        // Đăng nhập thành công, chuyển đến màn hình chính
        Navigator.pushReplacementNamed(context, '/nav');
      } else {
        // Đăng nhập thất bại, hiển thị thông báo lỗi
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Đăng nhập bằng Google thất bại. Vui lòng thử lại.'),
            backgroundColor: Colors.red,
          ),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    // Sửa lỗi asset tạm thời bằng cách dùng Icon thay thế Image
    // Sau khi bạn đã thêm ảnh vào assets, hãy đổi Icon lại thành Image.asset
    final Widget googleIcon = Image.asset(
      'assets/google_logo.png',
      height: 24.0,
      // Xử lý lỗi nếu không tìm thấy ảnh
      errorBuilder: (context, error, stackTrace) {
        return const Icon(Icons.error_outline, color: Colors.red);
      },
    );

    return SafeArea(
      child: Scaffold(
        resizeToAvoidBottomInset: false,
        body: Stack(
          children: [
            // Giả sử bạn có file này, nếu không hãy comment lại
            // Positioned.fill(
            //   child: Image.asset('assets/background.png', fit: BoxFit.cover),
            // ),

            // Lớp phủ khi đang tải
            if (_isLoading)
              Container(
                color: Colors.black.withOpacity(0.5),
                child: const Center(
                  child: CircularProgressIndicator(),
                ),
              ),

            Center(
              child: SingleChildScrollView(
                padding: const EdgeInsets.symmetric(horizontal: 20.0),
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    // Giả sử bạn có file này, nếu không hãy comment lại
                    // ClipOval(
                    //   child: Image.asset(
                    //     'assets/logo.png',
                    //     height: 150,
                    //     width: 150,
                    //   ),
                    // ),
                    const SizedBox(height: 10),
                    const Text(
                      'Healthy Care',
                      style: TextStyle(
                        color: Color(0xFF22668E),
                        fontSize: 25,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    const SizedBox(height: 50),
                    TextField(
                      controller: _emailController,
                      decoration: const InputDecoration(labelText: "Nhập email"),
                      keyboardType: TextInputType.emailAddress,
                    ),
                    const SizedBox(height: 15),
                    TextField(
                      controller: _passwordController,
                      obscureText: true,
                      decoration: const InputDecoration(labelText: "Mật khẩu"),
                    ),
                    const SizedBox(height: 20),
                    SizedBox(
                      height: 50,
                      width: double.infinity,
                      child: ElevatedButton(
                        onPressed: _isLoading ? null : handleLogin,
                        child: const Text(
                          "Đăng Nhập",
                          style: TextStyle(color: Colors.white, fontSize: 18),
                        ),
                      ),
                    ),
                    const SizedBox(height: 10),
                    SizedBox(
                      height: 50,
                      width: double.infinity,
                      child: OutlinedButton.icon(
                        icon: googleIcon,
                        onPressed: _isLoading ? null : handleGoogleSignIn,
                        label: const Text(
                          "Đăng Nhập với Google",
                          style: TextStyle(
                            fontSize: 18,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                        style: OutlinedButton.styleFrom(
                          side: BorderSide(color: Colors.grey.shade400),
                        ),
                      ),
                    ),
                    const SizedBox(height: 15),
                    const Text("Bạn chưa có tài khoản?"),
                    const SizedBox(height: 10),
                    SizedBox(
                      height: 50,
                      width: double.infinity,
                      child: OutlinedButton(
                        onPressed: _isLoading ? null : handleRegisterPress,
                        child: const Text(
                          "Đăng Ký",
                          style: TextStyle(
                            fontSize: 18,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                      ),
                    ),
                    TextButton(
                      onPressed: _isLoading ? null : handleForgotPassPress,
                      child: const Text(
                        "Quên mật khẩu?",
                        style: TextStyle(
                          color: Color(0xFF22668E),
                          decoration: TextDecoration.underline,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}