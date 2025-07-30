// lib/Model/doctor_model.dart

// Định nghĩa enum DoctorGender nếu chưa có
enum DoctorGender {
  male,
  female,
  other,
  unknown, // Giá trị mặc định nếu parse không thành công
}

DoctorGender _parseGender(String? genderString) {
  if (genderString == null) return DoctorGender.unknown;
  switch (genderString.toLowerCase()) {
    case 'male':
    case 'nam': // Thêm các biến thể tiếng Việt nếu API trả về vậy
      return DoctorGender.male;
    case 'female':
    case 'nữ':
      return DoctorGender.female;
    case 'other':
    case 'khác':
      return DoctorGender.other;
    default:
      return DoctorGender.unknown;
  }
}

String _genderToString(DoctorGender gender) {
  switch (gender) {
    case DoctorGender.male:
      return 'Male'; // Hoặc 'Nam' tùy theo logic hiển thị
    case DoctorGender.female:
      return 'Female'; // Hoặc 'Nữ'
    case DoctorGender.other:
      return 'Other'; // Hoặc 'Khác'
    default:
      return 'Unknown';
  }
}

class DoctorModel {
  final String id;
  final String? name;
  final String? cccd; // Căn cước công dân
  final String? phone;
  final String? email;
  final DateTime? dateOfBirth; // Sử dụng DateTime để dễ tính toán tuổi
  final DoctorGender gender;
  // Các trường khác từ Doctor.cs nếu có và cần thiết

  DoctorModel({
    required this.id,
    this.name,
    this.cccd,
    this.phone,
    this.email,
    this.dateOfBirth,
    this.gender = DoctorGender.unknown,
  });

  // lib/Model/doctor_model.dart
  factory DoctorModel.fromJson(Map<String, dynamic> json) {
    String? dobString = json['dateOfBirth']?.toString();
    DateTime? parsedDateOfBirth;
    if (dobString != null) {
      try {
        parsedDateOfBirth = DateTime.tryParse(dobString);
      } catch (e) {
        print("[DoctorModel] Lỗi parse dateOfBirth: $dobString - $e");
        parsedDateOfBirth = null;
      }
    }

    // ✅ SỬA LẠI CÁCH LẤY ID CHO KHỚP VỚI JSON TỪ API
    String doctorIdFromJson =
        json['idDoctor']?.toString() ?? // Thử key "idDoctor" trước
        json['id']?.toString() ?? // Rồi mới thử "id"
        json['_id']?.toString() ?? // Cuối cùng thử "_id"
        ''; // Mặc định là rỗng nếu không có key nào khớp

    print("[DoctorModel.fromJson] Parsing Doctor. JSON received: $json");
    print(
      "[DoctorModel.fromJson] Attempting to get ID. json['idDoctor']='${json['idDoctor']}', json['id']='${json['id']}', json['_id']='${json['_id']}'. Final doctorIdFromJson='$doctorIdFromJson'",
    );

    return DoctorModel(
      id: doctorIdFromJson, // Sử dụng ID đã lấy được
      name: json['name']?.toString(),
      cccd: json['cccd']?.toString(),
      phone: json['phone']?.toString(),
      email: json['email']?.toString(),
      dateOfBirth: parsedDateOfBirth,
      gender: _parseGender(json['gender']?.toString()),
    );
  }

  Map<String, dynamic> toJson() {
    final Map<String, dynamic> data = <String, dynamic>{};
    data['id'] = id;
    data['name'] = name;
    data['cccd'] = cccd;
    data['phone'] = phone;
    data['email'] = email;
    data['dateOfBirth'] =
        dateOfBirth?.toIso8601String(); // Chuyển về ISO string khi gửi đi
    data['gender'] = _genderToString(gender);
    return data;
  }
}
