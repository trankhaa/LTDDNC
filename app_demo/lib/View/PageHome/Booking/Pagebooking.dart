import 'package:flutter/material.dart';

// Import các màn hình con từ cùng thư mục Booking
import 'TuvanAI.dart';
import 'ChoseBranch.dart';

class Pagebooking extends StatelessWidget {
  const Pagebooking({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF0F4F7),
      appBar: AppBar(
        title: const Text(
          "Hình thức đặt khám",
          style: TextStyle(fontWeight: FontWeight.bold),
        ),
        backgroundColor: const Color(0xFF2D6A8E),
        foregroundColor: Colors.white,
        elevation: 0,
        centerTitle: true,
      ),
      body: Padding(
        padding: const EdgeInsets.all(30.0),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            // Lựa chọn 1: Tư vấn qua AI
            _buildBookingOption(
              context: context,
              icon: Icons.auto_awesome,
              title: "Tư vấn qua AI",
              subtitle: "Nhận tư vấn sơ bộ dựa trên triệu chứng của bạn.",
              iconColor: Colors.purple,
              onTap: () {
                // Điều hướng đến trang TuvanAI.dart
                Navigator.push(
                  context,
                  MaterialPageRoute(
                    builder: (context) => const TuvanAIScreen(),
                  ),
                );
              },
            ),
            const SizedBox(height: 30),

            // Lựa chọn 2: Đặt lịch tại chi nhánh
            _buildBookingOption(
              context: context,
              icon: Icons.local_hospital,
              title: "Đặt tại chi nhánh",
              subtitle: "Chọn bác sĩ, chuyên khoa và thời gian khám.",
              iconColor: Colors.blue,
              onTap: () {
                // Điều hướng đến trang ChoseBranch.dart
                Navigator.push(
                  context,
                  MaterialPageRoute(
                    builder: (context) => const ChoseBranchScreen(),
                  ),
                );
              },
            ),
          ],
        ),
      ),
    );
  }

  // Widget con để xây dựng mỗi lựa chọn
  Widget _buildBookingOption({
    required BuildContext context,
    required IconData icon,
    required String title,
    required String subtitle,
    required Color iconColor,
    required VoidCallback onTap,
  }) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(20),
      child: Container(
        padding: const EdgeInsets.all(24),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(20),
          boxShadow: [
            BoxShadow(
              color: Colors.grey.withOpacity(0.15),
              spreadRadius: 2,
              blurRadius: 10,
              offset: const Offset(0, 5),
            ),
          ],
        ),
        child: Row(
          children: [
            Container(
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(
                color: iconColor.withOpacity(0.1),
                shape: BoxShape.circle,
              ),
              child: Icon(icon, size: 30, color: iconColor),
            ),
            const SizedBox(width: 20),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    title,
                    style: const TextStyle(
                      fontSize: 18,
                      fontWeight: FontWeight.bold,
                      color: Colors.black87,
                    ),
                  ),
                  const SizedBox(height: 5),
                  Text(
                    subtitle,
                    style: TextStyle(fontSize: 14, color: Colors.grey[600]),
                  ),
                ],
              ),
            ),
            const Icon(Icons.arrow_forward_ios, color: Colors.grey, size: 16),
          ],
        ),
      ),
    );
  }
}
