// lib/Model/appointment_model.dart
import 'package:intl/intl.dart'; // Cho DateFormat nếu bạn cần xử lý chuỗi slot phức tạp,
// nhưng ở đây chủ yếu dùng cho startTimeOfSlot.

class AppointmentModel {
  final String
  idConfirmAppointment; // Hoặc tên ID tương ứng từ backend (ví dụ: "id")
  final String doctorId;
  final String
  slot; // Rất quan trọng: Chuỗi biểu diễn slot, ví dụ: "08:00-08:30" hoặc chỉ "08:00"
  final DateTime date; // Ngày khám. Backend nên trả về dạng ISO 8601 UTC.

  // Các trường khác có thể có từ ConfirmAppointment nếu bạn cần chúng ở client
  // final String nameDr;
  // final String patientId;
  // final String patientEmail;
  // final String? symptoms;
  // final DateTime createdAt;
  // final DateTime updatedAt;

  AppointmentModel({
    required this.idConfirmAppointment,
    required this.doctorId,
    required this.slot,
    required this.date,
    // this.nameDr,
    // this.patientId,
    // this.patientEmail,
    // this.symptoms,
    // required this.createdAt,
    // required this.updatedAt,
  });

  factory AppointmentModel.fromJson(Map<String, dynamic> json) {
    // Kiểm tra các key có tồn tại không để tránh lỗi null
    final String id =
        json['idConfirmAppointment']?.toString() ??
        json['id']?.toString() ??
        '';
    final String DoctorId = json['doctorId']?.toString() ?? '';
    final String slotValue = json['slot']?.toString() ?? '';
    final String dateString = json['date']?.toString() ?? '';

    if (id.isEmpty ||
        DoctorId.isEmpty ||
        slotValue.isEmpty ||
        dateString.isEmpty) {
      // Ném lỗi hoặc trả về một giá trị mặc định không hợp lệ nếu dữ liệu không đủ
      // Việc ném lỗi sẽ giúp dễ debug hơn
      throw FormatException("Dữ liệu appointment không hợp lệ từ API: $json");
    }

    DateTime parsedDate;
    try {
      // Backend nên trả về Date dưới dạng chuỗi ISO 8601 (UTC).
      // DateTime.parse sẽ hiểu được định dạng này.
      // .toUtc() đảm bảo đối tượng DateTime trong Dart là UTC, giúp so sánh ngày dễ dàng hơn.
      parsedDate = DateTime.parse(dateString).toUtc();
    } catch (e) {
      print("Lỗi parse date trong AppointmentModel: '$dateString' - $e");
      // Nếu không parse được, có thể gán một giá trị mặc định hoặc ném lỗi lại
      throw FormatException("Định dạng ngày không hợp lệ từ API: $dateString");
    }

    return AppointmentModel(
      idConfirmAppointment: id,
      doctorId: DoctorId,
      slot: slotValue,
      date: parsedDate,
      // nameDr: json['nameDr']?.toString(),
      // patientId: json['patientId']?.toString(),
      // patientEmail: json['patientEmail']?.toString(),
      // symptoms: json['symptoms']?.toString(),
      // createdAt: DateTime.parse(json['createdAt'] as String).toUtc(),
      // updatedAt: DateTime.parse(json['updatedAt'] as String).toUtc(),
    );
  }

  // Tiện ích để lấy giờ bắt đầu của slot
  // Giả sử slot có thể là "08:00" hoặc "08:00-08:30"
  // Hàm này sẽ trả về "08:00" trong cả hai trường hợp
  String get startTimeOfSlot {
    if (slot.contains('-')) {
      return slot.split('-')[0];
    }
    return slot;
  }

  // Map<String, dynamic> toJson() { // Không cần thiết nếu model này chỉ dùng để đọc dữ liệu từ API
  //   final Map<String, dynamic> data = <String, dynamic>{};
  //   data['idConfirmAppointment'] = idConfirmAppointment;
  //   data['doctorId'] = doctorId;
  //   data['slot'] = slot;
  //   data['date'] = date.toIso8601String();
  //   return data;
  // }
}
