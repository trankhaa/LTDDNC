// lib/services/department_api_service.dart
import 'dart:convert';
import 'package:http/http.dart' as http;
import '../Model/DepartmentModel.dart'; // Sửa đường dẫn nếu cần
import 'branch_api_service.dart'; // Để lấy host

class DepartmentApiService {
  static const String _baseApiUrl = '${BranchApiService.host}/api/department';

  Future<List<Department>> fetchAllDepartments() async {
    try {
      final response = await http.get(Uri.parse(_baseApiUrl));

      if (response.statusCode == 200) {
        final List<dynamic> data = json.decode(utf8.decode(response.bodyBytes));
        return data.map((json) => Department.fromJson(json)).toList();
      } else {
        throw Exception('Lỗi tải danh sách khoa: ${response.statusCode}');
      }
    } catch (e) {
      throw Exception('Không thể kết nối đến server (department): $e');
    }
  }

  // Nếu có API lấy department theo branchId, thêm vào đây
  // Future<List<DepartmentModel>> fetchDepartmentsByBranch(String branchId) async { ... }
}
