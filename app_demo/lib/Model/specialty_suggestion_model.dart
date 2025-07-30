// lib/models/specialty_suggestion_model.dart

// Model cho tọa độ địa lý
class Coordinates {
  final double latitude;
  final double longitude;

  Coordinates({required this.latitude, required this.longitude});
}

// Model cho một chi nhánh bệnh viện/phòng khám
class Branch {
  final String branchName;
  final String address;
  final String phoneNumber;
  final Coordinates coordinates;

  Branch({
    required this.branchName,
    required this.address,
    required this.phoneNumber,
    required this.coordinates,
  });
}

// Model chính, bao gồm chuyên khoa và danh sách các chi nhánh
class SpecialtySuggestion {
  final String specialtyName;
  final String departmentName;
  final List<Branch> branches;

  SpecialtySuggestion({
    required this.specialtyName,
    required this.departmentName,
    required this.branches,
  });
}
