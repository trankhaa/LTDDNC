// lib/Model/doctor_schedule_model.dart

class DoctorScheduleModel {
  final String? id; // MongoDB ObjectId, có thể là null nếu chưa lưu
  final String doctorId;
  // dayOfWeek, startTime, endTime có thể không cần thiết nếu backend
  // đã xử lý và chỉ trả về các slot khả dụng hoặc thông tin tổng quát.
  // Tuy nhiên, nếu API trả về chi tiết lịch, chúng ta sẽ cần:
  // final String dayOfWeek; // Ví dụ: "Monday", "Tuesday", etc. hoặc int 0-6
  // final String startTime; // Ví dụ: "08:00"
  // final String endTime;   // Ví dụ: "17:00"
  final int? examinationTime; // Thời gian khám mỗi lượt (phút), ví dụ: 30
  final double? consultationFee; // Phí khám
  final String? startTime; // Ví dụ: "08:00"
  final String? endTime; // Ví dụ: "17:00"
  final String? breakStartTime; // Ví dụ: "12:00" (nullable)
  final String? breakEndTime; // Ví dụ: "13:00" (nullable)

  DoctorScheduleModel({
    this.id,
    required this.doctorId,
    // required this.dayOfWeek,
    // required this.startTime,
    // required this.endTime,
    this.examinationTime,
    this.consultationFee,
    this.startTime,
    this.endTime,
    this.breakStartTime,
    this.breakEndTime,
  });

  factory DoctorScheduleModel.fromJson(Map<String, dynamic> json) {
    return DoctorScheduleModel(
      id:
          json['_id']?.toString() ??
          json['id']?.toString(), // Backend có thể dùng _id hoặc id
      doctorId: json['doctorId']?.toString() ?? '',
      // dayOfWeek: json['dayOfWeek']?.toString() ?? '',
      // startTime: json['startTime']?.toString() ?? '',
      // endTime: json['endTime']?.toString() ?? '',
      startTime: json['startTime']?.toString(),
      endTime: json['endTime']?.toString(),
      breakStartTime: json['breakStartTime']?.toString(), // Có thể null
      breakEndTime: json['breakEndTime']?.toString(), // Có thể null
      examinationTime:
          json['examinationTime'] != null
              ? int.tryParse(json['examinationTime'].toString())
              : null,
      consultationFee:
          json['consultationFee'] != null
              ? double.tryParse(json['consultationFee'].toString())
              : null,
    );
  }

  Map<String, dynamic> toJson() {
    final Map<String, dynamic> data = <String, dynamic>{};
    data['id'] = id;
    data['doctorId'] = doctorId;
    // data['dayOfWeek'] = dayOfWeek;
    // data['startTime'] = startTime;
    // data['endTime'] = endTime;
    data['examinationTime'] = examinationTime;
    data['consultationFee'] = consultationFee;
    return data;
  }
}
