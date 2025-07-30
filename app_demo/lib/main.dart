import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
// Import các màn hình
import 'View/Login&register/login.dart';
import 'View/Login&register/register.dart';
import 'View/PageHome/Nav.dart';
import 'package:app_demo/services/notification_service.dart';
import 'package:app_demo/services/navigation_service.dart'; // SỬA LẠI ĐƯỜNG DẪN NÀY
import 'auth_wrapper.dart'; // Đảm bảo đường dẫn này đúng
// Nếu bạn dùng định dạng ngày giờ tiếng Việt:

// Nếu bạn dùng định dạng ngày giờ tiếng Việt:
// import 'package:intl/date_symbol_data_local.dart';

// lib/main.dart

import 'package:easy_localization/easy_localization.dart';

void main() async {
  // Đảm bảo Flutter sẵn sàng
  WidgetsFlutterBinding.ensureInitialized();

  // Khởi tạo EasyLocalization
  await EasyLocalization.ensureInitialized();

  // Khởi tạo các dịch vụ hiện có của bạn
  await NotificationService.initialize();

  runApp(
    // Bọc ứng dụng của bạn trong EasyLocalization
    EasyLocalization(
      supportedLocales: const [Locale('en', 'US'), Locale('vi', 'VN')],
      path: 'assets/translations', // Đường dẫn đến thư mục chứa file dịch
      fallbackLocale: const Locale('en', 'US'), // Ngôn ngữ dự phòng
      child: const MyApp(),
    ),
  );
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Healthy Care App',
      debugShowCheckedModeBanner: false,

      // === TÍCH HỢP EASY_LOCALIZATION VÀO MATERIALAPP ===
      localizationsDelegates: context.localizationDelegates,
      supportedLocales: context.supportedLocales,
      locale: context.locale,
      // ===================================================

      // Giữ nguyên các cấu hình hiện có của bạn
      navigatorKey: NavigationService.navigatorKey,

      // === THAY ĐỔI QUAN TRỌNG ===
      // Đặt AuthWrapper làm màn hình chính. Nó sẽ quyết định hiển thị
      // LoginScreen hay NavScreen.
      home: const AuthWrapper(),
      // ===========================

      // Định nghĩa tất cả các routes để Navigator.pushNamed hoạt động
      routes: {
        '/login': (context) => const LoginScreen(), // <-- THÊM ROUTE CHO LOGIN
        '/nav': (context) => const NavScreen(),
        '/register': (context) => const RegisterScreen(),
      },
    );
  }
}
