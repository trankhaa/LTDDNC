// lib/services/news_service.dart

import 'dart:convert';
import 'package:http/http.dart' as http;
import '../Model/article_model.dart';

class NewsService {
  // LƯU Ý: Không nên để API Key trực tiếp trong code ở dự án thật.
  // Hãy sử dụng --dart-define hoặc các file .env để bảo mật.
  static const String _apiKey = "c06eb25e5d82b352e5ce1208916295b6";
  static const String _baseUrl = "https://gnews.io/api/v4/search";

  Future<List<Article>> fetchHealthNews() async {
    // Tương tự như code React, ta luôn tìm kiếm bằng tiếng Anh để có kết quả tốt nhất
    const String englishQuery =
        '"doctor" OR "health" OR "medical" OR "healthcare" OR "pharma" OR "hospital"';

    final uri = Uri.parse(
      '$_baseUrl?q=$englishQuery&lang=en&country=us&sortby=publishedAt&max=30&token=$_apiKey',
    );

    try {
      final response = await http.get(uri);

      if (response.statusCode == 200) {
        final data = json.decode(response.body);
        final List<dynamic> articlesJson = data['articles'];
        return articlesJson.map((json) => Article.fromJson(json)).toList();
      } else {
        final errorData = json.decode(response.body);
        final errorMessage = (errorData['errors'] as List<dynamic>).join(", ");
        throw Exception('API Error: $errorMessage');
      }
    } catch (e) {
      // Ném lại lỗi để UI có thể bắt và hiển thị
      throw Exception('Failed to load news: ${e.toString()}');
    }
  }
}
