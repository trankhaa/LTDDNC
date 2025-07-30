// lib/Model/doctor_detail_model.dart

class DoctorDetailModel {
  final String?
  id; // ID của bản ghi DoctorDetail (thường là ObjectId từ MongoDB)
  final String doctorId; // ID của bác sĩ mà chi tiết này thuộc về
  final String? degree; // Bằng cấp (ví dụ: "Tiến sĩ", "Thạc sĩ")
  final String? description; // Mô tả thêm về bác sĩ
  final String? img; // Đường dẫn tương đối đến ảnh đại diện bác sĩ
  // Các trường sau có thể có hoặc không, tùy thuộc vào API của bạn
  final String? certificateImg; // Đường dẫn ảnh chứng chỉ (nếu có)
  final String? degreeImg; // Đường dẫn ảnh bằng cấp (nếu có)

  // Các ID tham chiếu, có thể hữu ích nhưng tên đã có trong DoctorFullInfoModel sau này
  // final String? branchId;
  // final String? departmentId;
  // final String? specialtyId;

  DoctorDetailModel({
    this.id,
    required this.doctorId,
    this.degree,
    this.description,
    this.img,
    this.certificateImg,
    this.degreeImg,
    // this.branchId,
    // this.departmentId,
    // this.specialtyId,
  });

  factory DoctorDetailModel.fromJson(Map<String, dynamic> json) {
    return DoctorDetailModel(
      id: json['_id']?.toString() ?? json['id']?.toString(),
      doctorId: json['doctorId']?.toString() ?? '',
      degree: json['degree']?.toString(),
      description: json['description']?.toString(),
      img: json['img']?.toString(),
      certificateImg: json['certificateImg']?.toString(),
      degreeImg: json['degreeImg']?.toString(),
      // branchId: json['branchId']?.toString(),
      // departmentId: json['departmentId']?.toString(),
      // specialtyId: json['specialtyId']?.toString(),
    );
  }

  Map<String, dynamic> toJson() {
    final Map<String, dynamic> data = <String, dynamic>{};
    data['id'] = id;
    data['doctorId'] = doctorId;
    data['degree'] = degree;
    data['description'] = description;
    data['img'] = img;
    data['certificateImg'] = certificateImg;
    data['degreeImg'] = degreeImg;
    // data['branchId'] = branchId;
    // data['departmentId'] = departmentId;
    // data['specialtyId'] = specialtyId;
    return data;
  }
}
