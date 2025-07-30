import 'dart:async';
import 'dart:convert';
import 'package:http/http.dart' as http;
import '../Model/specialty_suggestion_model.dart';
import '../mock_data/ai_mock_data.dart';

// SỬA ĐỔI 1: Cập nhật URL để sử dụng đúng model 'gemini-1.5-flash' và endpoint 'v1beta'
const String geminiApiUrl =
    'https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent';

// LƯU Ý QUAN TRỌNG: Hãy đảm bảo API Key này được kích hoạt cho Generative Language API.
const String geminiApiKey = 'AIzaSyB_ZwlyHFXgiQXVhTFKVXWoWRkS9Dr3GEs'; // <-- THAY BẰNG KEY THẬT CỦA BẠN
const int apiTimeoutSeconds = 20; // Tăng thời gian chờ một chút
const bool enableApiLogging = true; // Bật để xem log gỡ lỗi

class AiConsultationService {
  /// Hàm chính để lấy gợi ý chuyên khoa từ AI
  Future<SpecialtySuggestion?> getSuggestionForSymptoms(String symptoms) async {
    if (symptoms.trim().isEmpty) {
      throw Exception("Vui lòng nhập triệu chứng của bạn.");
    }

    final availableSpecialties = _getAvailableSpecialties();
    try {
      final aiResponse = await _getSpecialtyFromAI(
        symptoms,
        availableSpecialties,
      );
      final suggestion = _findSpecialtySuggestion(aiResponse);
      return suggestion;
    } catch (e) {
      // Ném lại lỗi để UI có thể bắt và hiển thị
      rethrow;
    }
  }

  /// Gọi Gemini AI API để phân tích triệu chứng
  Future<String> _getSpecialtyFromAI(
    String symptoms,
    List<String> availableSpecialties,
  ) async {
    final specialtyListString = availableSpecialties.join(', ');

    // SỬA ĐỔI 2: Sử dụng prompt đơn giản và trực tiếp, giống hệt phiên bản ReactJS
    final prompt =
        'Dựa trên triệu chứng của người dùng: "$symptoms", hãy chọn một chuyên khoa y tế phù hợp nhất từ danh sách sau: [$specialtyListString]. Chỉ trả về DUY NHẤT tên của chuyên khoa đó, không giải thích gì thêm. Ví dụ: Nội tiêu hóa';

    final requestBody = {
      'contents': [
        {
          'parts': [
            {'text': prompt},
          ],
        },
      ],
      // SỬA ĐỔI 3: Đồng bộ hóa cấu hình sinh nội dung với phiên bản ReactJS
      'generationConfig': {
        'temperature': 0.2,
        'maxOutputTokens': 50,
      },
    };

    try {
      final response = await http
          .post(
            Uri.parse("$geminiApiUrl?key=$geminiApiKey"),
            headers: {'Content-Type': 'application/json'},
            body: jsonEncode(requestBody),
          )
          .timeout(Duration(seconds: apiTimeoutSeconds));

      // THÊM LOG ĐỂ GỠ LỖI
      if (enableApiLogging) {
        print("--- AI DEBUG START ---");
        print("Request URL: $geminiApiUrl");
        print("Status Code: ${response.statusCode}");
        print("Response Body (Raw): ${response.body}");
        print("--- AI DEBUG END ---");
      }

      if (response.statusCode == 200) {
        final responseData = jsonDecode(response.body);

        if (responseData['candidates'] == null ||
            responseData['candidates'].isEmpty ||
            responseData['candidates'][0]['content'] == null) {
          // Kiểm tra xem có phải do bộ lọc an toàn không
          if (responseData['promptFeedback']?['blockReason'] != null) {
            throw Exception(
              "Yêu cầu bị chặn bởi bộ lọc an toàn của AI. Vui lòng thử lại với triệu chứng khác.",
            );
          }
          throw Exception(
            "Phản hồi từ AI không hợp lệ. Vui lòng thử lại.",
          );
        }

        final aiResponseText =
            responseData['candidates'][0]['content']['parts'][0]['text'];

        // Sửa lại cách làm sạch để đơn giản và hiệu quả hơn
        final cleanedResponse = aiResponseText.trim().replaceAll('"', '');

        if (enableApiLogging) {
          print("AI Response (Before cleaning): '$aiResponseText'");
          print("AI Response (After cleaning): '$cleanedResponse'");
        }

        return cleanedResponse;
      } else {
        // Cung cấp thông báo lỗi rõ ràng hơn
        final errorBody = jsonDecode(response.body);
        final errorMessage = errorBody['error']?['message'] ?? response.body;
        throw Exception(
          "Lỗi từ máy chủ AI (${response.statusCode}): $errorMessage",
        );
      }
    } on TimeoutException {
      throw Exception(
          "Yêu cầu tới AI mất quá nhiều thời gian. Vui lòng kiểm tra kết nối mạng và thử lại.");
    } on http.ClientException {
      throw Exception("Lỗi kết nối mạng. Vui lòng kiểm tra lại Internet.");
    } catch (e) {
      // Bắt lại các lỗi khác và ném ra để UI xử lý
      throw Exception("Đã xảy ra lỗi không xác định: ${e.toString()}");
    }
  }

  /// Danh sách chuyên khoa từ mock data
  List<String> _getAvailableSpecialties() {
    return mockSuggestions.map((s) => s.specialtyName).toList();
  }

  /// Tìm chuyên khoa tương ứng với kết quả từ AI
  SpecialtySuggestion? _findSpecialtySuggestion(String aiResponse) {
    if (aiResponse.isEmpty) return null;

    final specialtyNameFromAI = aiResponse.trim().toLowerCase();

    // Ưu tiên tìm kiếm khớp chính xác
    for (final suggestion in mockSuggestions) {
      if (suggestion.specialtyName.toLowerCase() == specialtyNameFromAI) {
        if (enableApiLogging) {
          print("✅ Tìm thấy chuyên khoa chính xác: ${suggestion.specialtyName}");
        }
        return suggestion;
      }
    }

    // Nếu không khớp chính xác, thử tìm kiếm chứa chuỗi (để linh hoạt hơn)
    for (final suggestion in mockSuggestions) {
      if (suggestion.specialtyName.toLowerCase().contains(specialtyNameFromAI)) {
        if (enableApiLogging) {
          print(
              "⚠️ Tìm thấy chuyên khoa tương tự (chứa chuỗi): ${suggestion.specialtyName}");
        }
        return suggestion;
      }
    }

    print("❌ Không tìm thấy chuyên khoa phù hợp cho: '$specialtyNameFromAI'");
    return null; // Trả về null nếu không tìm thấy
  }
}