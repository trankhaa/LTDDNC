// lib/models/user_model.dart
import 'dart:convert';

// Helper function to decode from JSON string
User userFromJson(String str) => User.fromJson(json.decode(str));

// Helper function to encode to JSON string
String userToJson(User data) => json.encode(data.toJson());

class User {
  final String id;
  final String? googleId;
  final String email;
  final String? name;
  final String role;
  final bool isActive;
  final DateTime createdAt;
  final DateTime updatedAt;

  User({
    required this.id,
    this.googleId,
    required this.email,
    this.name,
    required this.role,
    required this.isActive,
    required this.createdAt,
    required this.updatedAt,
  });

  /// Factory constructor đã được làm cho "an toàn" hơn.
  /// Nó sẽ cung cấp giá trị mặc định nếu một trường bị thiếu trong JSON.
  factory User.fromJson(Map<String, dynamic> json) {
    return User(
      // Các trường này được giả định là luôn có trong JSON trả về
      id: json["id"],
      email: json["email"],
      name: json["name"], // name đã là nullable (String?) nên không cần xử lý

      // Cung cấp giá trị mặc định cho các trường có thể thiếu
      googleId: json["googleId"], // googleId đã là nullable (String?)
      role: json["role"] ?? "Patient", // Nếu 'role' là null, mặc định là "Patient"
      isActive: json["isActive"] ?? true, // Nếu 'isActive' là null, mặc định là true
      
      // Đối với DateTime, chúng ta cần kiểm tra null trước khi parse
      // Nếu giá trị là null, ta sẽ dùng thời gian hiện tại làm mặc định
      createdAt: json["createdAt"] != null
          ? DateTime.parse(json["createdAt"])
          : DateTime.now(),
      
      updatedAt: json["updatedAt"] != null
          ? DateTime.parse(json["updatedAt"])
          : DateTime.now(),
    );
  }

  Map<String, dynamic> toJson() => {
        "id": id,
        "googleId": googleId,
        "email": email,
        "name": name,
        "role": role,
        "isActive": isActive,
        "createdAt": createdAt.toIso8601String(),
        "updatedAt": updatedAt.toIso8601String(),
      };
}