import 'package:flutter/material.dart';

class Screenwait extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    // Lấy kích thước màn hình
    final Size size = MediaQuery.of(context).size;

    return Scaffold(
      body: SizedBox(
        width: size.width,
        height: size.height,
        child: Image.asset('assets/splash.png', fit: BoxFit.cover),
      ),
    );
  }
}
