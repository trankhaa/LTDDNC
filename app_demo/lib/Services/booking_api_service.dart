// lib/Services/booking_api_service.dart

import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:intl/intl.dart';

// Import các model cần thiết
import '../Model/booking_confirmation_model.dart';
import '../Model/appointment_model.dart';

// QUAN TRỌNG: Đảm bảo đây là IP LAN của máy tính backend của bạn khi test trên thiết bị thật.
const String _apiBaseUrl = "http://255.255.240.0:2000";

class BookingApiService {
  /// ✅ HÀM MỚI CHÍNH: Gọi API để tạo lịch hẹn và lấy link thanh toán PayOS.
  /// Hàm này sẽ được dùng trong luồng thanh toán mới.
  Future<String> createAppointmentAndGetPaymentLink(
    BookingConfirmationModel bookingData,
  ) async {
    // Endpoint mới trên backend của bạn
    final Uri uri = Uri.parse(
      '$_apiBaseUrl/api/booking/create-and-get-payment-link',
    );
    print('[API Call] POST (Payment Link): $uri');

    try {
      final response = await http
          .post(
            uri,
            headers: {
              'Content-Type': 'application/json; charset=UTF-8',
              'Accept': 'application/json',
            },
            body: json.encode(bookingData.toJson()),
          )
          .timeout(const Duration(seconds: 25));

      final String responseBody = utf8.decode(response.bodyBytes);
      final Map<String, dynamic> decodedJson = jsonDecode(responseBody);

      if (response.statusCode == 200) {
        final String? checkoutUrl = decodedJson['checkoutUrl'] as String?;
        if (checkoutUrl != null && checkoutUrl.isNotEmpty) {
          print(
            '[API Response] createAppointmentAndGetPaymentLink SUCCESS. URL: $checkoutUrl',
          );
          return checkoutUrl;
        } else {
          throw Exception('Backend không trả về checkoutUrl hợp lệ.');
        }
      } else {
        final String errorMessage =
            decodedJson['message'] as String? ??
            'Tạo link thanh toán thất bại (${response.statusCode}).';
        print(
          '[API Response] createAppointmentAndGetPaymentLink FAILED (${response.statusCode}): $errorMessage',
        );
        throw Exception(errorMessage);
      }
    } catch (e) {
      print('[API Exception] createAppointmentAndGetPaymentLink: $e');
      if (e is http.ClientException ||
          e.toString().contains('SocketException')) {
        throw Exception(
          'Không thể kết nối đến máy chủ. Vui lòng kiểm tra lại.',
        );
      }
      throw Exception(
        'Lỗi khi tạo link thanh toán: ${e.toString().replaceFirst("Exception: ", "")}',
      );
    }
  }

  // --- CÁC HÀM CŨ VẪN HỮU ÍCH VÀ ĐƯỢC GIỮ LẠI ---

  /// [HÀM CŨ] Đặt lịch hẹn trực tiếp (có thể không dùng nữa nếu mọi thứ đều qua thanh toán)
  /// Giữ lại để tham khảo hoặc cho các trường hợp không cần thanh toán.
  Future<Map<String, dynamic>> bookAppointment(
    BookingConfirmationModel bookingData,
  ) async {
    final Uri uri = Uri.parse('$_apiBaseUrl/api/booking/create');
    print(
      '[API Call] POST: $uri with body: ${json.encode(bookingData.toJson())}',
    );
    try {
      final response = await http
          .post(
            uri,
            headers: {
              'Content-Type': 'application/json; charset=UTF-8',
              'Accept': 'application/json',
            },
            body: json.encode(bookingData.toJson()),
          )
          .timeout(const Duration(seconds: 20));

      final String responseBody = utf8.decode(response.bodyBytes);
      final Map<String, dynamic> decodedJson = jsonDecode(responseBody);

      if (response.statusCode == 200 || response.statusCode == 201) {
        print('[API Response] bookAppointment SUCCESS: $responseBody');
        return decodedJson;
      } else {
        String errorMessage =
            decodedJson['message'] as String? ??
            'Đặt lịch thất bại (${response.statusCode}).';
        throw Exception(errorMessage);
      }
    } catch (e) {
      throw Exception(
        'Lỗi khi đặt lịch: ${e.toString().replaceFirst("Exception: ", "")}',
      );
    }
  }

  /// ✅ CẦN THIẾT: Kiểm tra xem một slot đã có người đặt chưa.
  Future<bool> checkSlotTaken(
    String doctorId,
    DateTime date,
    String slot,
  ) async {
    final dateString = DateFormat('yyyy-MM-dd').format(date);
    final queryParameters = {
      'doctorId': doctorId,
      'date': dateString,
      'slot': slot,
    };
    final Uri uri = Uri.parse(
      '$_apiBaseUrl/api/booking/check-slot',
    ).replace(queryParameters: queryParameters);
    print('[API Call] GET: $uri');
    try {
      final response = await http
          .get(uri, headers: {'Accept': 'application/json'})
          .timeout(const Duration(seconds: 10));
      if (response.statusCode == 200) {
        return jsonDecode(response.body)['isTaken'] as bool? ?? true;
      }
      return true; // Mặc định là đã được đặt nếu có lỗi
    } catch (e) {
      print('[API Exception] checkSlotTaken: $e');
      return true;
    }
  }

  /// ✅ CẦN THIẾT: Kiểm tra xem bệnh nhân đã đặt lịch với bác sĩ này trong ngày chưa.
  Future<bool> checkUserDailyBookingLimit(
    String patientId,
    String doctorId,
    DateTime date,
  ) async {
    final dateString = DateFormat('yyyy-MM-dd').format(date);
    final queryParameters = {
      'patientId': patientId,
      'doctorId': doctorId,
      'date': dateString,
    };
    final Uri uri = Uri.parse(
      '$_apiBaseUrl/api/booking/check-daily-limit',
    ).replace(queryParameters: queryParameters);
    print('[API Call] GET: $uri');
    try {
      final response = await http
          .get(uri, headers: {'Accept': 'application/json'})
          .timeout(const Duration(seconds: 10));
      if (response.statusCode == 200) {
        return jsonDecode(response.body)['hasBookingToday'] as bool? ?? true;
      }
      return true;
    } catch (e) {
      print('[API Exception] checkUserDailyBookingLimit: $e');
      return true;
    }
  }

  /// ✅ CẦN THIẾT: Lấy danh sách các lịch hẹn đã đặt của bác sĩ để tính toán slot trống.
  Future<List<AppointmentModel>> getAppointmentsByDoctorId(
    String doctorId,
  ) async {
    final Uri uri = Uri.parse(
      '$_apiBaseUrl/api/booking/appointments/doctor/$doctorId',
    );
    print('[API Call] GET (lấy lịch hẹn đã đặt của bác sĩ): $uri');

    try {
      final response = await http
          .get(uri, headers: {'Accept': 'application/json'})
          .timeout(const Duration(seconds: 30));

      if (response.statusCode == 200) {
        final String responseBody = utf8.decode(response.bodyBytes);
        final List<dynamic> decodedList = jsonDecode(responseBody);
        return decodedList
            .map(
              (item) => AppointmentModel.fromJson(item as Map<String, dynamic>),
            )
            .toList();
      } else if (response.statusCode == 404) {
        return []; // Bác sĩ chưa có lịch hẹn nào, trả về mảng rỗng
      } else {
        print(
          '[API Response] getAppointmentsByDoctorId FAILED (${response.statusCode})',
        );
        return [];
      }
    } catch (e) {
      print('[API Exception] getAppointmentsByDoctorId: $e');
      return [];
    }
  }
}
