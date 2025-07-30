// lib/Model/doctor_full_info_model.dart

import 'doctor_model.dart';
import 'doctor_detail_model.dart';
import 'doctor_schedule_model.dart';

// ✅ Hằng số URL máy chủ API (bạn muốn dùng cố định)
// Nên đặt ở một file utils/constants.dart riêng để dễ quản lý
// nhưng tạm thời để đây cho dễ theo dõi
const String apiBaseUrl = "http://localhost:2000";

class DoctorFullInfoModel {
  final DoctorModel doctor;
  final DoctorDetailModel? doctorDetail; // Có thể null nếu API không trả về
  final List<DoctorScheduleModel> doctorSchedules;
  final String? branchName;
  final String? departmentName;
  final String? specialtyName;

  DoctorFullInfoModel({
    required this.doctor,
    this.doctorDetail,
    this.doctorSchedules = const [], // Mặc định là danh sách rỗng
    this.branchName,
    this.departmentName,
    this.specialtyName,
  });

  factory DoctorFullInfoModel.fromJson(Map<String, dynamic> json) {
    // Parse Doctor
    final doctorData = json['doctor'];
    if (doctorData == null || !(doctorData is Map<String, dynamic>)) {
      throw Exception('Dữ liệu "doctor" không hợp lệ hoặc bị thiếu trong JSON');
    }
    final DoctorModel parsedDoctor = DoctorModel.fromJson(doctorData);

    // Parse DoctorDetail (nếu có)
    DoctorDetailModel? parsedDoctorDetail;
    if (json['doctorDetail'] != null &&
        json['doctorDetail'] is Map<String, dynamic>) {
      parsedDoctorDetail = DoctorDetailModel.fromJson(json['doctorDetail']);
    }

    // Parse DoctorSchedules (nếu có)
    List<DoctorScheduleModel> parsedSchedules = [];
    if (json['doctorSchedules'] != null && json['doctorSchedules'] is List) {
      parsedSchedules =
          (json['doctorSchedules'] as List)
              .map(
                (scheduleJson) => DoctorScheduleModel.fromJson(
                  scheduleJson as Map<String, dynamic>,
                ),
              )
              .toList();
    }

    return DoctorFullInfoModel(
      doctor: parsedDoctor,
      doctorDetail: parsedDoctorDetail,
      doctorSchedules: parsedSchedules,
      branchName: json['branchName']?.toString(),
      departmentName: json['departmentName']?.toString(),
      specialtyName: json['specialtyName']?.toString(),
    );
  }

  // Tiện ích để lấy URL đầy đủ của ảnh bác sĩ từ DoctorDetailModel
  String? get doctorImageUrl {
    if (doctorDetail?.img != null && doctorDetail!.img!.isNotEmpty) {
      if (doctorDetail!.img!.startsWith('http')) {
        return doctorDetail!.img!;
      }
      // Sử dụng hằng số apiBaseUrl đã định nghĩa
      return '$apiBaseUrl${doctorDetail!.img}';
    }
    return null; // hoặc trả về URL ảnh mặc định
  }

  // Tiện ích để lấy phí khám (ví dụ từ lịch đầu tiên)
  double? get consultationFee {
    return doctorSchedules.isNotEmpty
        ? doctorSchedules.first.consultationFee
        : null;
  }

  // Tiện ích để lấy thời gian khám (ví dụ từ lịch đầu tiên)
  int? get examinationTimePerSlot {
    return doctorSchedules.isNotEmpty
        ? doctorSchedules.first.examinationTime
        : null;
  }
}
