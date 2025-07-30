import 'dart:convert';
import 'dart:typed_data';

const String _apiHostForImages = "http://localhost:2000"; // ✅ Cố định PORT 2000

class DoctorInfo {
  final String id;
  final String? name;
  final String? doctorImage;
  final String? doctorDegree;
  final String? jobDescription;
  final String? dateOfBirth;
  final String? gender;
  final String? workingAtBranch;
  final String? phone;

  DoctorInfo({
    required this.id,
    this.name,
    this.doctorImage,
    this.doctorDegree,
    this.jobDescription,
    this.dateOfBirth,
    this.gender,
    this.workingAtBranch,
    this.phone,
  });

  factory DoctorInfo.fromJson(Map<String, dynamic> json) {
    return DoctorInfo(
      id: json['id']?.toString() ?? '',
      name: json['name']?.toString(),
      doctorImage: json['doctorImage']?.toString(),
      doctorDegree: json['doctorDegree']?.toString(),
      jobDescription: json['jobDescription']?.toString(),
      dateOfBirth: json['dateOfBirth']?.toString(),
      gender: json['gender']?.toString(),
      workingAtBranch: json['workingAtBranch']?.toString(),
      phone: json['phone']?.toString(),
    );
  }

  String get fullImageUrl {
    if (doctorImage != null && doctorImage!.isNotEmpty) {
      if (doctorImage!.startsWith('http')) {
        return doctorImage!;
      }
      return '$_apiHostForImages$doctorImage';
    }
    return '';
  }
}

// ✅ Hàm parse từ response body (dạng byte)
List<DoctorInfo> parseDoctorsFromBytes(Uint8List responseBodyBytes) {
  final String jsonString = utf8.decode(responseBodyBytes);
  final parsed = json.decode(jsonString).cast<Map<String, dynamic>>();
  return parsed
      .map<DoctorInfo>((jsonMap) => DoctorInfo.fromJson(jsonMap))
      .toList();
}
