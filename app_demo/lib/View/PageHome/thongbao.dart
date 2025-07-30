import 'package:flutter/material.dart';
// Import các service và model cần thiết
import '../../services/database_service.dart';
import '../../services/navigation_service.dart';

class ThongBaoPage extends StatefulWidget {
  final VoidCallback? onBadgeUpdate;
  
  const ThongBaoPage({super.key, this.onBadgeUpdate});

  @override
  State<ThongBaoPage> createState() => _ThongBaoPageState();
}

class _ThongBaoPageState extends State<ThongBaoPage> {
  // Sử dụng FutureBuilder để xử lý việc tải dữ liệu bất đồng bộ
  late Future<List<AppNotification>> _notificationsFuture;

  @override
  void initState() {
    super.initState();
    _loadNotifications();
  }

  // Hàm để tải danh sách thông báo từ database
  void _loadNotifications() {
    setState(() {
      _notificationsFuture = DatabaseService.instance.getAllNotifications();
    });
  }

  // ✅ THÊM: Hàm để thông báo cho NavScreen cập nhật badge
  void _updateParentBadge() {
    if (widget.onBadgeUpdate != null) {
      widget.onBadgeUpdate!();
    }
  }

  // Hàm xử lý khi người dùng nhấn vào một thông báo
  Future<void> _onNotificationTapped(AppNotification notification) async {
    // Đánh dấu là đã đọc nếu chưa đọc
    if (notification.isRead == 0) {
      await DatabaseService.instance.markAsRead(notification.id!);
      _loadNotifications(); // Tải lại danh sách để cập nhật UI
      
      // ✅ THÊM: Cập nhật badge ở NavScreen
      _updateParentBadge();
    }

    // Thực hiện điều hướng dựa trên payload
    final navigator = NavigationService.navigatorKey.currentState;
    if (navigator != null) {
      if (notification.payload.startsWith('appointment_reminder_')) {
        // Ví dụ: điều hướng đến chi tiết lịch hẹn
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Điều hướng đến chi tiết lịch hẹn ${notification.id}'),
          ),
        );
      }
    }
  }

  // ✅ THÊM: Hàm để đánh dấu tất cả là đã đọc
  Future<void> _markAllAsRead() async {
    await DatabaseService.instance.markAllAsRead();
    _loadNotifications();
    _updateParentBadge();
    
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(
        content: Text('Đã đánh dấu tất cả thông báo là đã đọc'),
      ),
    );
  }

  // ✅ THÊM: Hàm để xóa thông báo
  Future<void> _deleteNotification(int id) async {
    await DatabaseService.instance.deleteNotification(id);
    _loadNotifications();
    _updateParentBadge();
    
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(
        content: Text('Đã xóa thông báo'),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Thông Báo'),
        backgroundColor: Colors.white,
        elevation: 1,
        // ✅ THÊM: Nút actions để đánh dấu tất cả đã đọc
        actions: [
          IconButton(
            icon: const Icon(Icons.done_all),
            onPressed: _markAllAsRead,
            tooltip: 'Đánh dấu tất cả đã đọc',
          ),
        ],
      ),
      body: FutureBuilder<List<AppNotification>>(
        future: _notificationsFuture,
        builder: (context, snapshot) {
          // Trường hợp đang tải dữ liệu
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          }
          // Trường hợp có lỗi
          if (snapshot.hasError) {
            return Center(child: Text('Đã xảy ra lỗi: ${snapshot.error}'));
          }
          // Trường hợp không có dữ liệu hoặc danh sách rỗng
          if (!snapshot.hasData || snapshot.data!.isEmpty) {
            return const Center(
              child: Text(
                'Bạn chưa có thông báo nào.',
                style: TextStyle(fontSize: 16, color: Colors.grey),
              ),
            );
          }

          // Khi có dữ liệu, hiển thị ListView
          final notifications = snapshot.data!;
          return ListView.builder(
            itemCount: notifications.length,
            itemBuilder: (context, index) {
              final notification = notifications[index];
              final bool isRead = notification.isRead == 1;

              return Card(
                margin: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                elevation: 2,
                child: ListTile(
                  onTap: () => _onNotificationTapped(notification),
                  leading: CircleAvatar(
                    backgroundColor: isRead 
                        ? Colors.grey.shade300 
                        : Theme.of(context).primaryColor,
                    child: Icon(
                      isRead 
                          ? Icons.notifications_off_outlined 
                          : Icons.notifications_active,
                      color: isRead ? Colors.grey.shade600 : Colors.white,
                    ),
                  ),
                  title: Text(
                    notification.title,
                    style: TextStyle(
                      fontWeight: isRead ? FontWeight.normal : FontWeight.bold,
                    ),
                  ),
                  subtitle: Text(
                    notification.body,
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                    style: TextStyle(
                      color: isRead ? Colors.grey : Colors.black87,
                    ),
                  ),
                  // ✅ THÊM: Nút xóa thông báo
                  trailing: IconButton(
                    icon: const Icon(Icons.delete_outline, color: Colors.red),
                    onPressed: () => _showDeleteDialog(notification),
                  ),
                ),
              );
            },
          );
        },
      ),
    );
  }

  // ✅ THÊM: Dialog xác nhận xóa thông báo
  void _showDeleteDialog(AppNotification notification) {
    showDialog(
      context: context,
      builder: (BuildContext context) {
        return AlertDialog(
          title: const Text('Xác nhận xóa'),
          content: const Text('Bạn có chắc chắn muốn xóa thông báo này?'),
          actions: [
            TextButton(
              onPressed: () => Navigator.of(context).pop(),
              child: const Text('Hủy'),
            ),
            TextButton(
              onPressed: () {
                Navigator.of(context).pop();
                _deleteNotification(notification.id!);
              },
              child: const Text('Xóa', style: TextStyle(color: Colors.red)),
            ),
          ],
        );
      },
    );
  }
}