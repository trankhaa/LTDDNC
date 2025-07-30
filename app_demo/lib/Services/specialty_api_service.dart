// lib/services/specialty_api_service.dart
import 'dart:convert';
import 'package:http/http.dart' as http;
import '../Model/Specialty_model.dart';
// Giả sử bạn có file này và nó export 'host'
import 'package:app_demo/services/branch_api_service.dart';

class SpecialtyApiService {
  static const String _baseApiUrl =
      '${BranchApiService.host}/api'; // Đảm bảo BranchApiService.host đúng

  Future<List<Specialty>> fetchAllSpecialties() async {
    final String fullUrl = '$_baseApiUrl/Specialty/all';
    print('Fetching all specialties from: $fullUrl');

    try {
      final response = await http.get(Uri.parse(fullUrl));
      print('fetchAllSpecialties - Status Code: ${response.statusCode}');

      if (response.statusCode == 200) {
        final String rawJsonResponse = utf8.decode(response.bodyBytes);
        print(
          'RAW JSON RESPONSE from fetchAllSpecialties (Status 200):\n$rawJsonResponse',
        );
        final List<dynamic> data = json.decode(rawJsonResponse);
        print('Parsed ${data.length} total specialties from JSON.');
        return data.map((jsonItem) {
          print('Parsing all_specialty item: $jsonItem');
          Specialty spec = Specialty.fromJson(jsonItem);
          print(
            'Parsed all_specialty: ID=${spec.idSpecialty}, Name=${spec.specialtyName}',
          );
          return spec;
        }).toList();
      } else {
        print(
          'Lỗi tải danh sách tất cả chuyên khoa: ${response.statusCode}, Body: ${response.body}',
        );
        throw Exception(
          'Lỗi tải danh sách tất cả chuyên khoa: ${response.statusCode}',
        );
      }
    } catch (e) {
      print('Lỗi trong fetchAllSpecialties (catch block): $e');
      throw Exception(
        'Không thể kết nối đến server hoặc lỗi parsing (fetchAllSpecialties): $e',
      );
    }
  }

  Future<List<Specialty>> fetchSpecialtiesByDepartment(
    String idDepartment,
  ) async {
    final String fullUrl = '$_baseApiUrl/Specialty/by-department/$idDepartment';
    print('Fetching specialties by department from: $fullUrl');

    try {
      final response = await http.get(Uri.parse(fullUrl));
      print(
        'fetchSpecialtiesByDepartment - Status Code: ${response.statusCode}',
      );

      if (response.statusCode == 200) {
        final String rawJsonResponse = utf8.decode(response.bodyBytes);
        print(
          'RAW JSON RESPONSE from fetchSpecialtiesByDepartment (Status 200):\n$rawJsonResponse',
        );
        final List<dynamic> data = json.decode(rawJsonResponse);
        print(
          'Parsed ${data.length} specialties from JSON for department $idDepartment.',
        );

        List<Specialty> specialties =
            data.map((jsonItem) {
              print('Parsing specialty_by_dept item: $jsonItem');
              Specialty spec = Specialty.fromJson(jsonItem);
              print(
                'Parsed specialty_by_dept: ID=${spec.idSpecialty}, Name=${spec.specialtyName}',
              );
              return spec;
            }).toList();
        return specialties;
      } else if (response.statusCode == 404) {
        print(
          'Không tìm thấy chuyên khoa cho khoa $idDepartment (404). Response body: ${response.body}',
        );
        return [];
      } else {
        print(
          'Lỗi tải danh sách chuyên khoa theo khoa $idDepartment: ${response.statusCode}, Body: ${response.body}',
        );
        throw Exception(
          'Lỗi tải danh sách chuyên khoa theo khoa: ${response.statusCode}',
        );
      }
    } catch (e) {
      print(
        'Lỗi trong fetchSpecialtiesByDepartment (catch block) for $idDepartment: $e',
      );
      throw Exception(
        'Không thể kết nối đến server hoặc lỗi parsing (fetchSpecialtiesByDepartment): $e',
      );
    }
  }
}
