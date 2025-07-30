// lib/services/branch_api_service.dart

import 'dart:convert';
import 'package:http/http.dart' as http;
// ✅ Sử dụng đường dẫn package
import 'package:app_demo/Model/branch_model.dart'; // Thay 'app_demo' bằng tên package của bạn

class BranchApiService {
  // ✅ Biến 'host' là static để có thể truy cập từ bên ngoài
  static const String host = "http://255.255.240.0:2000"; // Thay IP nếu cần
  static const String _apiUrl = '$host/api';

  Future<List<Branch>> fetchAllBranches() async {
    try {
      final response = await http.get(Uri.parse('$_apiUrl/Branch/all'));
      if (response.statusCode == 200) {
        final List<dynamic> data = json.decode(response.body);
        return data.map((json) => Branch.fromJson(json)).toList();
      } else {
        throw Exception('Lỗi tải dữ liệu: ${response.statusCode}');
      }
    } catch (e) {
      throw Exception('Không thể kết nối đến server tại $host. Lỗi: $e');
    }
  }
}