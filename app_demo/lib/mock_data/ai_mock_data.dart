// lib/mock_data/ai_mock_data.dart

import '../Model/specialty_suggestion_model.dart';

// Đây là dữ liệu từ mockData.js, được chuyển đổi sang dạng các đối tượng Dart
final List<SpecialtySuggestion> mockSuggestions = [
  SpecialtySuggestion(
    specialtyName: "Nội tiêu hóa",
    departmentName: "Khoa Nội Tổng Hợp",
    branches: [
      Branch(
        branchName: "Bệnh viện A - Chi nhánh trung tâm",
        address: "123 Nguyễn Văn Cừ, Quận 1, TP.HCM",
        phoneNumber: "0123456789",
        coordinates: Coordinates(latitude: 10.7769, longitude: 106.7009),
      ),
      Branch(
        branchName: "Phòng khám Đa khoa B",
        address: "456 Lê Lợi, Quận 3, TP.HCM",
        phoneNumber: "0987654321",
        coordinates: Coordinates(latitude: 10.782, longitude: 106.695),
      ),
    ],
  ),
  SpecialtySuggestion(
    specialtyName: "Tim mạch",
    departmentName: "Khoa Tim Mạch Can Thiệp",
    branches: [
      Branch(
        branchName: "Bệnh viện A - Chi nhánh trung tâm",
        address: "123 Nguyễn Văn Cừ, Quận 1, TP.HCM",
        phoneNumber: "0123456789",
        coordinates: Coordinates(latitude: 10.7769, longitude: 106.7009),
      ),
    ],
  ),
  SpecialtySuggestion(
    specialtyName: "Da liễu",
    departmentName: "Khoa Da Liễu",
    branches: [
      Branch(
        branchName: "Phòng khám Da Liễu Sài Gòn",
        address: "101 Pasteur, Quận 3, TP.HCM",
        phoneNumber: "02838383838",
        coordinates: Coordinates(latitude: 10.78, longitude: 106.69),
      ),
    ],
  ),
  SpecialtySuggestion(
    specialtyName: "Tai Mũi Họng",
    departmentName: "Khoa Tai Mũi Họng",
    branches: [
      Branch(
        branchName: "Bệnh viện Tai Mũi Họng TP.HCM",
        address: "155B Trần Quốc Thảo, Quận 3, TP.HCM",
        phoneNumber: "02839312345",
        coordinates: Coordinates(latitude: 10.787, longitude: 106.686),
      ),
    ],
  ),
  // Thêm các chuyên khoa khác từ file JS của bạn vào đây nếu cần
];
