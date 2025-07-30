// lib/Model/booking_confirmation_model.dart

class BookingConfirmationModel {
  // IdConfirmAppointment sẽ được backend tự generate, không cần gửi từ client
  // final String? idConfirmAppointment;

  final String nameDr; // Tên bác sĩ
  final String doctorId; // ID bác sĩ
  final String slot; // Khung giờ đã chọn, ví dụ: "09:00-09:30"
  final String patientId; // ID bệnh nhân (hiện tại sẽ là "ảo")
  final String patientEmail; // Email bệnh nhân (hiện tại sẽ là "ảo")
  final DateTime date; // Ngày khám (UTC)
  final String? symptoms; // Triệu chứng/lý do khám (có thể null hoặc rỗng)
  final String patientName;
  final double consultationFee;

  // CreatedAt và UpdatedAt thường do backend quản lý, không cần gửi từ client

  BookingConfirmationModel({
    // this.idConfirmAppointment,
    required this.nameDr,
    required this.doctorId,
    required this.slot,
    required this.patientId,
    required this.patientEmail,
    required this.date,
    this.symptoms,
    required this.patientName,
    required this.consultationFee,
  });

  // Chuyển đối tượng thành JSON để gửi lên API
  Map<String, dynamic> toJson() {
    return {
      // 'IdConfirmAppointment': idConfirmAppointment, // Không gửi nếu backend tự tạo
      'NameDr': nameDr,
      'DoctorId': doctorId,
      'Slot': slot,
      'PatientId': patientId,
      'PatientEmail': patientEmail,
      // Đảm bảo gửi DateTime ở định dạng ISO 8601 mà backend có thể parse
      // (thường là UTC)
      'Date': date.toIso8601String(),
      'Symptoms': symptoms ?? '', // Gửi chuỗi rỗng nếu symptoms là null
      "patientName": patientName,
      "consultationFee": consultationFee,
    };
  }

  // Factory fromJson không thực sự cần thiết nếu bạn chỉ gửi dữ liệu này
  // và không nhận lại đối tượng tương tự từ API (thường API sẽ trả về thông báo thành công hoặc lỗi)
  // Nhưng nếu API trả về chính đối tượng đã tạo thì có thể thêm:
  /*
  factory BookingConfirmationModel.fromJson(Map<String, dynamic> json) {
    return BookingConfirmationModel(
      idConfirmAppointment: json['idConfirmAppointment'],
      nameDr: json['nameDr'],
      doctorId: json['doctorId'],
      slot: json['slot'],
      patientId: json['patientId'],
      patientEmail: json['patientEmail'],
      date: DateTime.parse(json['date']), // Cẩn thận với múi giờ
      symptoms: json['symptoms'],
    );
  }
  */
}
