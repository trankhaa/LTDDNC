// lib/Model/branch_model.dart

// ✅ Sử dụng đường dẫn package để import
import '../Services/branch_api_service.dart'; // Thay 'app_demo' bằng tên package của bạn
// Giả sử bạn có thư mục Services và file branch_api_service.dart
// và trong đó có định nghĩa `BranchApiService.host`

// Lớp GeoPoint tương ứng với backend
class GeoPoint {
  final double latitude;
  final double longitude;

  GeoPoint({required this.latitude, required this.longitude});

  factory GeoPoint.fromJson(Map<String, dynamic> json) {
    return GeoPoint(
      // Đảm bảo ép kiểu an toàn và cung cấp giá trị mặc định nếu cần
      latitude: (json['latitude'] as num?)?.toDouble() ?? 0.0,
      longitude: (json['longitude'] as num?)?.toDouble() ?? 0.0,
    );
  }

  Map<String, dynamic> toJson() {
    return {'latitude': latitude, 'longitude': longitude};
  }
}

class Branch {
  final String idBranch; // Tương ứng IdBranch (MongoDB _id)
  final String branchName; // Tương ứng BranchName
  final String branchAddress; // Tương ứng BranchAddress
  final String? branchHotline; // Tương ứng BranchHotline
  final String? branchEmail; // Tương ứng BranchEmail
  final String? description; // Tương ứng Description
  final GeoPoint? coordinates; // Tương ứng Coordinates
  final String? imageUrl; // Tương ứng ImageUrl (đã xử lý URL đầy đủ)
  final DateTime? createdAt; // Tương ứng CreatedAt
  final DateTime? updatedAt; // Tương ứng UpdatedAt

  Branch({
    required this.idBranch,
    required this.branchName,
    required this.branchAddress,
    this.branchHotline,
    this.branchEmail,
    this.description,
    this.coordinates,
    this.imageUrl,
    this.createdAt,
    this.updatedAt,
  });

  factory Branch.fromJson(Map<String, dynamic> json) {
    String? relativeImagePath = json['imageUrl'];
    String? fullImageUrl;

    if (relativeImagePath != null && relativeImagePath.isNotEmpty) {
      // Giả sử BranchApiService.host được định nghĩa và là một chuỗi không rỗng
      // Ví dụ: static const String host = "http://yourdomain.com";
      if (!relativeImagePath.startsWith('http') &&
          BranchApiService.host.isNotEmpty) {
        fullImageUrl =
            BranchApiService.host +
            (relativeImagePath.startsWith('/')
                ? relativeImagePath
                : '/$relativeImagePath');
      } else {
        fullImageUrl = relativeImagePath;
      }
    }

    return Branch(
      // Backend thường trả về _id cho IdBranch, hoặc đã map sang idBranch.
      // Ưu tiên idBranch nếu có, nếu không thì dùng _id.
      idBranch: json['idBranch'] as String? ?? json['_id'] as String? ?? '',
      branchName: json['branchName'] as String? ?? 'Tên không xác định',
      branchAddress:
          json['branchAddress'] as String? ?? 'Địa chỉ không xác định',
      branchHotline: json['branchHotline'] as String?,
      branchEmail: json['branchEmail'] as String?,
      description: json['description'] as String?,
      coordinates:
          json['coordinates'] != null
              ? GeoPoint.fromJson(json['coordinates'] as Map<String, dynamic>)
              : null,
      imageUrl: fullImageUrl,
      createdAt:
          json['createdAt'] != null
              ? DateTime.tryParse(json['createdAt'] as String)
              : null,
      updatedAt:
          json['updatedAt'] != null
              ? DateTime.tryParse(json['updatedAt'] as String)
              : null,
    );
  }

  // (Tùy chọn) Thêm phương thức toJson nếu bạn cần gửi đối tượng này lên server
  Map<String, dynamic> toJson() {
    return {
      'idBranch':
          idBranch, // Hoặc '_id' tùy theo backend mong đợi khi tạo/cập nhật
      'branchName': branchName,
      'branchAddress': branchAddress,
      'branchHotline': branchHotline,
      'branchEmail': branchEmail,
      'description': description,
      'coordinates': coordinates?.toJson(),
      // Khi gửi lên, có thể bạn muốn gửi đường dẫn tương đối thay vì URL đầy đủ
      // Cái này tùy thuộc vào logic xử lý ở backend
      'imageUrl': imageUrl, // Cân nhắc việc chuyển đổi về relative path nếu cần
      'createdAt': createdAt?.toIso8601String(),
      'updatedAt': updatedAt?.toIso8601String(),
    };
  }
}
