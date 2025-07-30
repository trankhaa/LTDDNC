import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:flutter_local_notifications/flutter_local_notifications.dart';
import 'package:permission_handler/permission_handler.dart';
import 'package:timezone/data/latest.dart' as tz;
import 'package:timezone/timezone.dart' as tz;
import 'package:intl/intl.dart';

// ✅ BƯỚC 1: IMPORT CÁC SERVICE CẦN THIẾT
// Đảm bảo các đường dẫn này chính xác với cấu trúc dự án của bạn
import 'navigation_service.dart';
import 'database_service.dart';

class NotificationService {
  static final FlutterLocalNotificationsPlugin _notificationsPlugin =
      FlutterLocalNotificationsPlugin();

  /// Khởi tạo dịch vụ thông báo, cấu hình, và xin quyền.
  static Future<void> initialize() async {
    const AndroidInitializationSettings initializationSettingsAndroid =
        AndroidInitializationSettings('@mipmap/ic_launcher');

    const DarwinInitializationSettings initializationSettingsIOS =
        DarwinInitializationSettings(
          requestAlertPermission: true,
          requestBadgePermission: true,
          requestSoundPermission: true,
        );

    const InitializationSettings initializationSettings =
        InitializationSettings(
          android: initializationSettingsAndroid,
          iOS: initializationSettingsIOS,
        );

    await _notificationsPlugin.initialize(
      initializationSettings,
      onDidReceiveNotificationResponse: _onNotificationTap,
    );

    await _configureLocalTimeZone();
    await _createNotificationChannels();
    await requestPermissions();
  }

  /// Hàm callback được gọi khi người dùng nhấn vào một thông báo của hệ thống.
  static void _onNotificationTap(NotificationResponse notificationResponse) {
    debugPrint(
      'Notification tapped with payload: ${notificationResponse.payload}',
    );
    final NavigatorState? navigator =
        NavigationService.navigatorKey.currentState;

    if (navigator != null) {
      final String? payload = notificationResponse.payload;

      if (payload == 'navigate_to_thong_bao') {
        navigator.pushNamedAndRemoveUntil(
          '/nav',
          (route) => false,
          arguments: 2,
        );
      } else if (payload == 'navigate_to_phieu_kham') {
        navigator.pushNamedAndRemoveUntil(
          '/nav',
          (route) => false,
          arguments: 1,
        );
      } else if (payload != null &&
          payload.startsWith('appointment_reminder_')) {
        try {
          final String appointmentId = payload.split('_').last;
          debugPrint(
            'Navigating to appointment details for ID: $appointmentId',
          );
          navigator.pushNamedAndRemoveUntil(
            '/nav',
            (route) => false,
            arguments: 1,
          );
          // Ví dụ: navigator.push(MaterialPageRoute(builder: (_) => AppointmentDetailsPage(id: appointmentId)));
        } catch (e) {
          debugPrint('Error parsing appointment ID from payload: $e');
        }
      }
    }
  }

  static Future<void> _configureLocalTimeZone() async {
    tz.initializeTimeZones();
    tz.setLocalLocation(tz.getLocation('Asia/Ho_Chi_Minh'));
    debugPrint('✅ Timezone configured: Asia/Ho_Chi_Minh');
  }

  static Future<void> _createNotificationChannels() async {
    final AndroidFlutterLocalNotificationsPlugin? androidImplementation =
        _notificationsPlugin
            .resolvePlatformSpecificImplementation<
              AndroidFlutterLocalNotificationsPlugin
            >();

    if (androidImplementation != null) {
      const AndroidNotificationChannel immediateChannel =
          AndroidNotificationChannel(
            'immediate_channel_id',
            'Thông báo tức thì',
            description: 'Kênh cho các thông báo quan trọng tức thời.',
            importance: Importance.max,
          );
      await androidImplementation.createNotificationChannel(immediateChannel);

      const AndroidNotificationChannel scheduledChannel =
          AndroidNotificationChannel(
            'scheduled_channel_id',
            'Nhắc nhở & Lịch hẹn',
            description: 'Kênh cho các thông báo nhắc nhở đã được lên lịch.',
            importance: Importance.max,
          );
      await androidImplementation.createNotificationChannel(scheduledChannel);
      debugPrint('✅ Notification channels created');
    }
  }

  static Future<void> requestPermissions() async {
    await Permission.notification.request();
    if (defaultTargetPlatform == TargetPlatform.android) {
      await Permission.scheduleExactAlarm.request();
    }
  }

  /// Hiển thị một thông báo ngay lập tức VÀ lưu nó vào database.
  static Future<void> showImmediateNotification({
    required int id,
    required String title,
    required String body,
    String? payload,
  }) async {
    const NotificationDetails notificationDetails = NotificationDetails(
      android: AndroidNotificationDetails(
        'immediate_channel_id',
        'Thông báo tức thì',
        importance: Importance.max,
        priority: Priority.high,
      ),
      iOS: DarwinNotificationDetails(
        presentAlert: true,
        presentBadge: true,
        presentSound: true,
      ),
    );

    // 1. Hiển thị thông báo hệ thống
    await _notificationsPlugin.show(
      id,
      title,
      body,
      notificationDetails,
      payload: payload,
    );

    // 2. Tạo một bản ghi thông báo và lưu vào database cục bộ
    final notificationToSave = AppNotification(
      // id không cần thiết khi insert vì nó là AUTOINCREMENT
      title: title,
      body: body,
      payload: payload ?? '',
      receivedAt: DateTime.now().toIso8601String(), // Lưu thời điểm nhận
      isRead: 0, // Mặc định là chưa đọc
    );
    await DatabaseService.instance.insertNotification(notificationToSave);

    debugPrint(
      '✅ Shown and SAVED immediate notification ID $id with payload: $payload',
    );
  }

  /// Lên lịch cho một thông báo trong tương lai VÀ lưu nó vào database.
  static Future<void> scheduleAppointmentReminder({
    required int id, // Đây là ID của lịch hẹn, dùng để quản lý
    required String doctorName,
    required DateTime appointmentDate,
    required String appointmentSlot,
    int hoursBefore = 8,
  }) async {
    try {
      final startTimeString = appointmentSlot.split('-')[0].trim();
      final timeParts = startTimeString.split(':');
      final hour = int.parse(timeParts[0]);
      final minute = int.parse(timeParts[1]);

      final appointmentDateTime = DateTime(
        appointmentDate.year,
        appointmentDate.month,
        appointmentDate.day,
        hour,
        minute,
      );

      final scheduledTime = appointmentDateTime.subtract(
        Duration(hours: hoursBefore),
      );

      if (scheduledTime.isBefore(DateTime.now())) {
        debugPrint(
          '⚠️ Scheduled time is in the past. Not scheduling notification ID $id.',
        );
        return;
      }

      final tzScheduledTime = tz.TZDateTime.from(scheduledTime, tz.local);

      final String title = '🏥 Nhắc nhở lịch hẹn';
      final String body =
          'Bạn có lịch hẹn với Bác sĩ $doctorName vào lúc $appointmentSlot, ${_formatDate(appointmentDate)}';
      final String payload = 'appointment_reminder_$id';

      // 1. Lên lịch thông báo hệ thống
      await _notificationsPlugin.zonedSchedule(
        id,
        title,
        body,
        tzScheduledTime,
        const NotificationDetails(
          android: AndroidNotificationDetails(
            'scheduled_channel_id',
            'Nhắc nhở & Lịch hẹn',
            importance: Importance.max,
            priority: Priority.high,
          ),
          iOS: DarwinNotificationDetails(
            presentAlert: true,
            presentBadge: true,
            presentSound: true,
          ),
        ),
        androidScheduleMode: AndroidScheduleMode.exactAllowWhileIdle,
        uiLocalNotificationDateInterpretation:
            UILocalNotificationDateInterpretation.absoluteTime,
        payload: payload,
      );

      // 2. Tạo một bản ghi thông báo và lưu vào database cục bộ
      final notificationToSave = AppNotification(
        title: title,
        body: body,
        payload: payload,
        receivedAt:
            DateTime.now().toIso8601String(), // Lưu thời điểm nó được LÊN LỊCH
        isRead: 0, // Mặc định là chưa đọc
      );
      await DatabaseService.instance.insertNotification(notificationToSave);

      debugPrint(
        '✅ Scheduled and SAVED notification ID $id for: ${_formatDateTime(scheduledTime)}',
      );
    } catch (e) {
      debugPrint('❌ Error scheduling and saving notification ID $id: $e');
    }
  }

  static String _formatDate(DateTime date) =>
      DateFormat('dd/MM/yyyy').format(date);
  static String _formatDateTime(DateTime dateTime) =>
      DateFormat('HH:mm - dd/MM/yyyy').format(dateTime);

  /// Hủy một thông báo hệ thống đã lên lịch.
  /// Lưu ý: Hành động này KHÔNG xóa thông báo khỏi database.
  /// Bạn sẽ cần một logic riêng để xóa nó khỏi `ThongBaoPage` nếu cần.
  static Future<void> cancelNotification(int id) async {
    await _notificationsPlugin.cancel(id);
    debugPrint('ℹ️ Cancelled scheduled system notification ID $id');
  }

  static Future<void> cancelAllNotifications() async {
    await _notificationsPlugin.cancelAll();
    debugPrint('ℹ️ Cancelled all scheduled system notifications');
  }
}
