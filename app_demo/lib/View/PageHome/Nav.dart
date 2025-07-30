import 'package:flutter/material.dart';
import 'package:app_demo/Services/database_service.dart';

// Import các trang con từ cùng thư mục PageHome
import 'trangchu.dart';
import 'phieukham.dart';
import 'thongbao.dart';
import 'taikhoan.dart';

// Import màn hình Pagebooking từ thư mục Booking
import 'Booking/Pagebooking.dart';

class NavScreen extends StatefulWidget {
  const NavScreen({super.key});

  @override
  State<NavScreen> createState() => _NavScreenState();
}

class _NavScreenState extends State<NavScreen> {
  int _selectedIndex = 0;
  bool _isInitialRoute = true;

  // ✅ THÊM: Biến để lưu số lượng thông báo chưa đọc
  int _unreadCount = 0;

  static const List<Widget> _pages = <Widget>[
    TrangChuPage(),
    HealthNewsScreen(),
    ThongBaoPage(),
    TaiKhoanPage(),
  ];

  @override
  void initState() {
    super.initState();
    // Tải số lượng thông báo chưa đọc khi khởi tạo
    _loadUnreadCount();
  }

  // ✅ THÊM: Hàm để tải số lượng thông báo chưa đọc
  Future<void> _loadUnreadCount() async {
    try {
      final count = await DatabaseService.instance.getUnreadNotificationCount();
      if (mounted) {
        setState(() {
          _unreadCount = count;
        });
      }
    } catch (e) {
      print('Error loading unread count: $e');
    }
  }

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    if (_isInitialRoute) {
      final arguments = ModalRoute.of(context)?.settings.arguments;
      if (arguments is int) {
        WidgetsBinding.instance.addPostFrameCallback((_) {
          if (mounted) {
            setState(() {
              _selectedIndex = arguments;
            });
          }
        });
      }
      _isInitialRoute = false;
    }
  }

  void _onItemTapped(int index) {
    setState(() {
      _selectedIndex = index;
    });

    // ✅ THÊM: Khi nhấn vào tab thông báo, tải lại số lượng
    if (index == 2) {
      _loadUnreadCount();
    }
  }

  void _onFabTapped() {
    Navigator.push(
      context,
      MaterialPageRoute(builder: (context) => const Pagebooking()),
    );
  }

  Widget _buildNavItem({
    required IconData icon,
    required String label,
    required int index,
    bool showBadge = false,
    int badgeCount = 0,
  }) {
    final bool isSelected = _selectedIndex == index;
    final Color color = isSelected ? const Color(0xFF22668E) : Colors.grey;

    return Expanded(
      child: InkWell(
        onTap: () => _onItemTapped(index),
        splashColor: Colors.transparent,
        highlightColor: Colors.transparent,
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: <Widget>[
            // ✅ THÊM: Stack để hiển thị badge
            Stack(
              children: [
                Icon(icon, color: color),
                // Hiển thị badge nếu có thông báo chưa đọc
                if (showBadge && badgeCount > 0)
                  Positioned(
                    right: 0,
                    top: 0,
                    child: Container(
                      padding: const EdgeInsets.all(2),
                      decoration: BoxDecoration(
                        color: Colors.red,
                        borderRadius: BorderRadius.circular(10),
                      ),
                      constraints: const BoxConstraints(
                        minWidth: 16,
                        minHeight: 16,
                      ),
                      child: Text(
                        badgeCount > 99 ? '99+' : badgeCount.toString(),
                        style: const TextStyle(
                          color: Colors.white,
                          fontSize: 10,
                          fontWeight: FontWeight.bold,
                        ),
                        textAlign: TextAlign.center,
                      ),
                    ),
                  ),
              ],
            ),
            const SizedBox(height: 4),
            Text(
              label,
              style: TextStyle(
                color: color,
                fontSize: 12,
                fontWeight: isSelected ? FontWeight.bold : FontWeight.normal,
              ),
            ),
          ],
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: _pages.elementAt(_selectedIndex),
      floatingActionButton: FloatingActionButton(
        onPressed: _onFabTapped,
        backgroundColor: const Color(0xFF22668E),
        elevation: 4.0,
        shape: const CircleBorder(),
        child: const Icon(Icons.calendar_today, color: Colors.white),
      ),
      floatingActionButtonLocation: FloatingActionButtonLocation.centerDocked,
      bottomNavigationBar: BottomAppBar(
        shape: const CircularNotchedRectangle(),
        notchMargin: 8.0,
        child: SizedBox(
          height: 60.0,
          child: Row(
            children: <Widget>[
              _buildNavItem(
                icon: Icons.home_outlined,
                label: 'Trang chủ',
                index: 0,
              ),
              _buildNavItem(
                icon: Icons.assignment_outlined,
                label: 'Phiếu khám',
                index: 1,
              ),
              const SizedBox(width: 60),
              // ✅ THÊM: Badge cho tab thông báo
              _buildNavItem(
                icon: Icons.notifications_outlined,
                label: 'Thông báo',
                index: 2,
                showBadge: true,
                badgeCount: _unreadCount,
              ),
              _buildNavItem(
                icon: Icons.person_outline,
                label: 'Tài khoản',
                index: 3,
              ),
            ],
          ),
        ),
      ),
    );
  }
}
