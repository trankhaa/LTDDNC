import 'package:sqflite/sqflite.dart';
import 'package:path/path.dart';

// Model để biểu diễn một thông báo
class AppNotification {
  final int? id;
  final String title;
  final String body;
  final String payload;
  final String receivedAt;
  int isRead; // 0 = chưa đọc, 1 = đã đọc

  AppNotification({
    this.id,
    required this.title,
    required this.body,
    required this.payload,
    required this.receivedAt,
    this.isRead = 0,
  });

  Map<String, dynamic> toMap() {
    return {
      'id': id,
      'title': title,
      'body': body,
      'payload': payload,
      'receivedAt': receivedAt,
      'isRead': isRead,
    };
  }

  // Factory để tạo AppNotification từ Map đọc từ DB
  factory AppNotification.fromMap(Map<String, dynamic> map) {
    return AppNotification(
      id: map['id'],
      title: map['title'],
      body: map['body'],
      payload: map['payload'] ?? '',
      receivedAt: map['receivedAt'],
      isRead: map['isRead'],
    );
  }
}

class DatabaseService {
  static final DatabaseService instance = DatabaseService._init();
  static Database? _database;

  DatabaseService._init();

  Future<Database> get database async {
    if (_database != null) return _database!;
    _database = await _initDB('notifications.db');
    return _database!;
  }

  Future<Database> _initDB(String filePath) async {
    final dbPath = await getDatabasesPath();
    final path = join(dbPath, filePath);

    return await openDatabase(path, version: 1, onCreate: _createDB);
  }

  Future _createDB(Database db, int version) async {
    const idType = 'INTEGER PRIMARY KEY AUTOINCREMENT';
    const textType = 'TEXT NOT NULL';
    const intType = 'INTEGER NOT NULL';

    await db.execute('''
      CREATE TABLE notifications (
        id $idType,
        title $textType,
        body $textType,
        payload $textType,
        receivedAt $textType,
        isRead $intType
      )
    ''');
  }

  Future<void> insertNotification(AppNotification notification) async {
    final db = await instance.database;
    await db.insert('notifications', notification.toMap());
  }

  Future<List<AppNotification>> getAllNotifications() async {
    final db = await instance.database;
    final result = await db.query('notifications', orderBy: 'id DESC');
    return result.map((json) => AppNotification.fromMap(json)).toList();
  }

  Future<void> markAsRead(int id) async {
    final db = await instance.database;
    await db.update(
      'notifications',
      {'isRead': 1},
      where: 'id = ?',
      whereArgs: [id],
    );
  }

  // ✅ THÊM: Hàm để đếm số lượng thông báo chưa đọc
  Future<int> getUnreadNotificationCount() async {
    final db = await instance.database;
    final result = await db.rawQuery(
      'SELECT COUNT(*) as count FROM notifications WHERE isRead = 0'
    );
    return Sqflite.firstIntValue(result) ?? 0;
  }

  // ✅ THÊM: Hàm để đánh dấu tất cả thông báo là đã đọc
  Future<void> markAllAsRead() async {
    final db = await instance.database;
    await db.update(
      'notifications',
      {'isRead': 1},
      where: 'isRead = ?',
      whereArgs: [0],
    );
  }

  // ✅ THÊM: Hàm để xóa thông báo theo ID
  Future<void> deleteNotification(int id) async {
    final db = await instance.database;
    await db.delete(
      'notifications',
      where: 'id = ?',
      whereArgs: [id],
    );
  }

  // ✅ THÊM: Hàm để xóa tất cả thông báo
  Future<void> deleteAllNotifications() async {
    final db = await instance.database;
    await db.delete('notifications');
  }

  Future close() async {
    final db = await instance.database;
    db.close();
  }
}