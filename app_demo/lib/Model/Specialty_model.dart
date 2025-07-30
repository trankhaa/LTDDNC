// lib/Model/Specialty_model.dart
const String _apiHostForImages = "";

class Specialty {
  final String idSpecialty;
  final String specialtyName;
  final String? description;
  final String? imageUrlSpecialty; // Tên thuộc tính trong Dart
  final String? idDepartment;
  final String? departmentName;

  Specialty({
    required this.idSpecialty,
    required this.specialtyName,
    this.description,
    this.imageUrlSpecialty,
    this.idDepartment,
    this.departmentName,
  });

  factory Specialty.fromJson(Map<String, dynamic> json) {
    // Đọc đối tượng department lồng nhau từ JSON, key là 'department' (chữ d thường)
    final departmentData = json['department'] as Map<String, dynamic>?;

    // In ra để debug
    // print("Parsing Specialty JSON: $json");
    // print("Attempting to read idSpecialty: ${json['idSpecialty']}");
    // print("Attempting to read specialtyName: ${json['specialtyName']}");
    // print("Attempting to read imageUrl: ${json['imageUrl']}");
    // if (departmentData != null) {
    //   print("Attempting to read department.idDepartment: ${departmentData['idDepartment']}");
    //   print("Attempting to read department.departmentName: ${departmentData['departmentName']}");
    // }

    return Specialty(
      idSpecialty:
          json['idSpecialty'] as String? ?? '', // Sửa key thành 'idSpecialty'
      specialtyName:
          json['specialtyName'] as String? ??
          'Chuyên khoa không xác định', // Sửa key thành 'specialtyName'
      description:
          json['description'] as String?, // Sửa key thành 'description'
      imageUrlSpecialty:
          json['imageUrl'] as String?, // Sửa key thành 'imageUrl'
      idDepartment:
          departmentData?['idDepartment']
              as String?, // Sửa key thành 'idDepartment'
      departmentName:
          departmentData?['departmentName']
              as String?, // Sửa key thành 'departmentName'
    );
  }

  Map<String, dynamic> toJson() {
    // Đảm bảo các key này khớp với mong đợi của backend nếu bạn có gửi dữ liệu Specialty lên
    return {
      'idSpecialty': idSpecialty,
      'specialtyName': specialtyName,
      'description': description,
      'imageUrl': imageUrlSpecialty, // Key này là 'imageUrl' trong JSON
      'department': {
        // Key này là 'department' trong JSON
        'idDepartment': idDepartment,
        'departmentName': departmentName,
      },
    };
  }

  String get fullImageUrlSpecialty {
    if (imageUrlSpecialty != null && imageUrlSpecialty!.isNotEmpty) {
      if (imageUrlSpecialty!.startsWith('http://') ||
          imageUrlSpecialty!.startsWith('https://')) {
        return imageUrlSpecialty!;
      }
      // Đảm bảo _apiHostForImages không có dấu / ở cuối nếu imageUrlSpecialty bắt đầu bằng /
      // Hoặc imageUrlSpecialty không bắt đầu bằng / nếu _apiHostForImages có dấu / ở cuối.
      // Logic hiện tại là an toàn nếu imageUrlSpecialty luôn bắt đầu bằng /
      String host = _apiHostForImages;
      if (host.endsWith('/')) {
        host = host.substring(0, host.length - 1);
      }
      return '$host$imageUrlSpecialty'; // imageUrlSpecialty từ JSON đã có dấu / ở đầu
    }
    return ''; // Hoặc trả về một ảnh placeholder mặc định
  }
}
