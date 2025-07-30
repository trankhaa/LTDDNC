// lib/Services/doctor_api_service.dart

import 'dart:convert';
import 'package:http/http.dart' as http;

// Import các model cần thiết
import '../Model/doctor_full_info_model.dart';
import '../Model/doctor_info_model.dart'; // Cho hàm searchDoctorsByCriteria

// Hằng số base URL đã được khai báo trong DoctorFullInfoModel
// Chúng ta có thể import nó hoặc định nghĩa lại ở đây (hoặc file constants)
// Để đơn giản, tạm định nghĩa lại:
const String _apiBaseUrl = "http://255.255.240.0"; // Chỉ là host và port

class DoctorApiService {
  // Hàm lấy thông tin đầy đủ của bác sĩ bằng ID
  Future<DoctorFullInfoModel> getDoctorFullInfoById(String doctorId) async {
    // Endpoint: /api/doctor/{doctorId}/fullinfo
    final Uri uri = Uri.parse('$_apiBaseUrl/api/doctor/$doctorId/fullinfo');
    print('Gọi API (GET): $uri');

    try {
      final response = await http.get(
        uri,
        headers: {
          'Accept': 'application/json',
        },
      ).timeout(const Duration(seconds: 15));

      if (response.statusCode == 200) {
        final String responseBody = utf8.decode(response.bodyBytes);
        final Map<String, dynamic> decodedJson = jsonDecode(responseBody);
        return DoctorFullInfoModel.fromJson(decodedJson);
      } else if (response.statusCode == 404) {
        print('Lỗi 404: Không tìm thấy bác sĩ với ID $doctorId. Body: ${utf8.decode(response.bodyBytes)}');
        throw Exception('Không tìm thấy thông tin bác sĩ (404)');
      } else {
        print('Lỗi server ${response.statusCode}: ${utf8.decode(response.bodyBytes)}');
        String errorMessage = 'Lỗi tải thông tin bác sĩ (${response.statusCode})';
        try {
          final errorJson = jsonDecode(utf8.decode(response.bodyBytes));
          if (errorJson['message'] != null) {
            errorMessage = errorJson['message'];
          } else if (errorJson['title'] != null) {
            errorMessage = errorJson['title'];
          }
        } catch (_) {
          errorMessage = 'Lỗi máy chủ không xác định (${response.statusCode}). Chi tiết: ${response.reasonPhrase ?? 'Không rõ'}';
        }
        throw Exception(errorMessage);
      }
    } catch (e) {
      print('Ngoại lệ khi gọi getDoctorFullInfoById: $e');
      if (e is http.ClientException || e.toString().contains('SocketException') || e.toString().contains('Failed host lookup')) {
        throw Exception('Không thể kết nối đến máy chủ. Vui lòng kiểm tra kết nối mạng.');
      }
      throw Exception('Lỗi kết nối hoặc xử lý dữ liệu: ${e.toString()}');
    }
  }

  // Sử dụng POST cho tìm kiếm bác sĩ theo tiêu chí
  // Đổi tên tham số specialtyId thành tùy chọn (nullable) cho nhất quán
  Future<List<DoctorInfo>> searchDoctorsByCriteria({
    required String branchId,
    required String departmentId,
    String? specialtyId, // Để là nullable nếu có thể không truyền
  }) async {
    // Endpoint: /api/DoctorDetail/search-by-criteria (POST)
    final Uri uri = Uri.parse('$_apiBaseUrl/api/DoctorDetail/search-by-criteria');

    // Body phải khớp với DTO SearchDoctorCriteriaDto.cs
    // Các key trong DTO là: BranchId, DepartmentId, SpecialtyId
    final Map<String, String?> body = {
      'BranchId': branchId,
      'DepartmentId': departmentId,
    };

    if (specialtyId != null && specialtyId.isNotEmpty) {
      body['SpecialtyId'] = specialtyId;
    } else {
      // Nếu backend yêu cầu SpecialtyId phải có (dù là null hoặc empty string)
      // thì bạn cần thêm vào body, ví dụ: body['SpecialtyId'] = null;
      // Hoặc nếu backend bỏ qua nếu không có key thì không cần thêm.
      // Dựa trên DTO C# thì có vẻ nó mong đợi giá trị,
      // nếu không truyền thì giá trị mặc định của string là null.
      // Tuy nhiên, khi gửi JSON, nếu key không tồn tại thì khác với key có giá trị null.
      // An toàn nhất là luôn gửi key nếu backend mong đợi.
      // Nếu backend có thể xử lý khi key này thiếu, thì không cần thêm vào body.
      // Giả sử backend sẽ bỏ qua nếu không có key 'SpecialtyId' hoặc
      // specialtyId là null/empty string và bạn không muốn gửi key rỗng.
    }


    print('Gọi API (POST): $uri với body: ${json.encode(body)}');

    try {
      final response = await http.post(
        uri,
        headers: {
          'Content-Type': 'application/json; charset=UTF-8',
          'Accept': 'application/json',
        },
        body: json.encode(body), // Gửi body đã được chuẩn bị
      ).timeout(const Duration(seconds: 15));

      if (response.statusCode == 200) {
        return parseDoctorsFromBytes(response.bodyBytes);
      } else if (response.statusCode == 404) {
        print('Tìm kiếm bác sĩ: Không tìm thấy kết quả (404). Body: ${utf8.decode(response.bodyBytes)}');
        return []; // Trả về danh sách rỗng nếu không tìm thấy
      } else {
        print('Lỗi server khi tìm kiếm bác sĩ ${response.statusCode}: ${utf8.decode(response.bodyBytes)}');
        String errorMessage = 'Lỗi tìm kiếm bác sĩ (${response.statusCode})';
        try {
          final errorJson = jsonDecode(utf8.decode(response.bodyBytes));
          if (errorJson['message'] != null) {
            errorMessage = errorJson['message'];
          } else if (errorJson['title'] != null) {
            errorMessage = errorJson['title'];
          }
        } catch (_) {
          errorMessage = 'Lỗi máy chủ không xác định khi tìm kiếm (${response.statusCode}).';
        }
        throw Exception(errorMessage);
      }
    } catch (e) {
      print('Ngoại lệ khi gọi searchDoctorsByCriteria (POST): $e');
      if (e is http.ClientException || e.toString().contains('SocketException') || e.toString().contains('Failed host lookup')) {
        throw Exception('Không thể kết nối đến máy chủ tìm kiếm. Vui lòng kiểm tra kết nối mạng.');
      }
      throw Exception('Lỗi kết nối hoặc xử lý dữ liệu khi tìm kiếm: ${e.toString()}');
    }
  }
}