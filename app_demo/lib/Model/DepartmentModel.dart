// lib/Model/department_model.dart
import '../services/branch_api_service.dart';

class Department {
  final String idDepartment;
  final String departmentName;
  final String? imageUrl; // Giữ nguyên, JSON cũng dùng 'imageUrl'
  final DateTime? createdAt; // Giữ nguyên, JSON cũng dùng 'createdAt'
  final DateTime? updatedAt; // Giữ nguyên, JSON cũng dùng 'updatedAt'

  Department({
    required this.idDepartment,
    required this.departmentName,
    this.imageUrl,
    this.createdAt,
    this.updatedAt,
  });

  factory Department.fromJson(Map<String, dynamic> json) {
    String? relativeImagePath = json['imageUrl'] as String?; // Sửa key thành 'imageUrl' (chữ i thường)
    String? fullImageUrl;

    if (relativeImagePath != null && relativeImagePath.isNotEmpty) {
      if (!relativeImagePath.startsWith('http') &&
          BranchApiService.host.isNotEmpty) {
        String host = BranchApiService.host;
        if (host.endsWith('/')) {
          host = host.substring(
            0,
            host.length - 1,
          );
        }
        fullImageUrl =
            host +
            (relativeImagePath.startsWith('/')
                ? relativeImagePath
                : '/$relativeImagePath');
      } else {
        fullImageUrl = relativeImagePath;
      }
    }

    // In ra để debug (có thể xóa sau khi đã chạy đúng)
    // print("Parsing Department JSON: $json");
    // print("Attempting to read idDepartment: ${json['idDepartment']}");
    // print("Attempting to read departmentName: ${json['departmentName']}");

    return Department(
      idDepartment:
          json['idDepartment'] as String? ?? '', // SỬA KEY thành 'idDepartment' (chữ i thường)
      departmentName:
          json['departmentName'] as String? ?? // SỬA KEY thành 'departmentName' (chữ d thường)
          'Tên khoa không xác định',
      imageUrl: fullImageUrl, // Key này đã đúng với JSON ('imageUrl')
      createdAt:
          json['createdAt'] != null // Key này đã đúng với JSON ('createdAt')
              ? DateTime.tryParse(json['createdAt'] as String)
              : null,
      updatedAt:
          json['updatedAt'] != null // Key này đã đúng với JSON ('updatedAt')
              ? DateTime.tryParse(json['updatedAt'] as String)
              : null,
    );
  }

  Map<String, dynamic> toJson() {
    // Nếu bạn có gửi dữ liệu lên server, cũng nên đảm bảo các key này khớp với mong đợi của server
    return {
      'idDepartment': idDepartment,    // Cân nhắc đổi thành 'idDepartment' nếu server mong đợi vậy
      'departmentName': departmentName, // Cân nhắc đổi thành 'departmentName' nếu server mong đợi vậy
      'imageUrl': imageUrl,
      'createdAt': createdAt?.toIso8601String(),
      'updatedAt': updatedAt?.toIso8601String(),
    };
  }
}