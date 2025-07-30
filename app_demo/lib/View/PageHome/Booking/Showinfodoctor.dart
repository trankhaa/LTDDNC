// lib/View/PageHome/Booking/Showinfodoctor.dart
import 'package:flutter/material.dart';
// Import các model
import '../../../Model/doctor_info_model.dart';
// ✅ Import DoctorApiService
import '../../../Services/doctor_api_service.dart';
import 'doctor_booking_screen.dart'; // ✅ Dòng này phải trỏ đến file 

// SelectedCriteriaDisplayInfo class giữ nguyên
class SelectedCriteriaDisplayInfo {
  final String branchName;
  final String departmentName;
  final String specialtyName;

  SelectedCriteriaDisplayInfo({
    required this.branchName,
    required this.departmentName,
    required this.specialtyName,
  });
}

class ShowInfoDoctorPage extends StatefulWidget {
  final String branchId;
  final String departmentId;
  final String specialtyId; // Giữ nguyên, có thể là rỗng hoặc null
  final SelectedCriteriaDisplayInfo selectedDisplayInfo;

  const ShowInfoDoctorPage({
    super.key,
    required this.branchId,
    required this.departmentId,
    required this.specialtyId, // Sẽ được truyền vào service
    required this.selectedDisplayInfo,
  });

  @override
  State<ShowInfoDoctorPage> createState() => _ShowInfoDoctorPageState();
}

class _ShowInfoDoctorPageState extends State<ShowInfoDoctorPage> {
  // ✅ Khởi tạo DoctorApiService
  final DoctorApiService _apiService = DoctorApiService();
  late Future<List<DoctorInfo>> _doctorsFuture;

  List<DoctorInfo> _allDoctors = [];
  List<DoctorInfo> _filteredDoctors = [];
  final TextEditingController _searchController = TextEditingController();
  String _selectedDegreeFilter = 'all';

  final List<String> _degreeOptions = [
    'all', 'giáo sư', 'tiến sĩ', 'thạc sĩ', 'ckii', 'cki', 'bác sĩ',
  ];
  final Map<String, String> _degreeDisplayMap = {
    'all': 'Tất cả bằng cấp', 'giáo sư': 'Giáo sư (GS, PGS)', 'tiến sĩ': 'Tiến sĩ (TS)',
    'thạc sĩ': 'Thạc sĩ (ThS)', 'ckii': 'Chuyên khoa II (CKII)', 'cki': 'Chuyên khoa I (CKI)',
    'bác sĩ': 'Bác sĩ (BS)',
  };

  static const String _defaultDoctorAvatarAsset = "assets/images/default_avatar.png";

  @override
  void initState() {
    super.initState();
    _loadDoctors();
    _searchController.addListener(_applyFilters);
  }

  void _loadDoctors() {
    print(
      "ShowInfoDoctorPage: Gọi API tìm bác sĩ với:\n"
      "  Branch ID: ${widget.branchId}\n"
      "  Department ID: ${widget.departmentId}\n"
      "  Specialty ID: ${widget.specialtyId}", // specialtyId có thể là rỗng
    );

    // ✅ Sử dụng _apiService để gọi hàm searchDoctorsByCriteria
    // Truyền cả widget.specialtyId (có thể rỗng hoặc null)
    _doctorsFuture = _apiService.searchDoctorsByCriteria(
      branchId: widget.branchId,
      departmentId: widget.departmentId,
      specialtyId: widget.specialtyId.isNotEmpty ? widget.specialtyId : null,
      // Nếu specialtyId rỗng, truyền null để service xử lý (hàm searchDoctorsByCriteria trong service đã cho phép specialtyId là nullable)
    );

    // Xử lý kết quả từ Future không thay đổi nhiều
    _doctorsFuture
        .then((doctors) {
          if (mounted) {
            setState(() {
              _allDoctors = doctors;
              _filteredDoctors = doctors;
            });
          }
        })
        .catchError((error) {
          print("ShowInfoDoctorPage: Lỗi khi tải danh sách bác sĩ: $error");
          if (mounted) {
            setState(() {
              _allDoctors = [];
              _filteredDoctors = [];
            });
            // Hiển thị SnackBar lỗi cho người dùng
            ScaffoldMessenger.of(context).showSnackBar(
              SnackBar(
                content: Text('Lỗi tải danh sách bác sĩ: ${error.toString().replaceFirst("Exception: ", "")}'),
                backgroundColor: Colors.red,
              ),
            );
          }
        });
  }

  // Hàm _applyFilters giữ nguyên
  void _applyFilters() {
    String searchTerm = _searchController.text.toLowerCase().trim();
    String degreeFilter = _selectedDegreeFilter.toLowerCase();

    setState(() {
      _filteredDoctors = _allDoctors.where((doctor) {
        bool matchesSearchTerm = true;
        if (searchTerm.isNotEmpty) {
          matchesSearchTerm =
              (doctor.name?.toLowerCase().contains(searchTerm) ?? false) ||
              (doctor.doctorDegree?.toLowerCase().contains(searchTerm) ?? false) ||
              (doctor.jobDescription?.toLowerCase().contains(searchTerm) ?? false);
        }

        bool matchesDegreeFilter = true;
        if (degreeFilter != 'all') {
          matchesDegreeFilter =
              doctor.doctorDegree?.toLowerCase().contains(degreeFilter) ?? false;
        }
        return matchesSearchTerm && matchesDegreeFilter;
      }).toList();
    });
  }


  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  // Hàm _calculateAge giữ nguyên
  String _calculateAge(String? dateOfBirthString) {
    if (dateOfBirthString == null || dateOfBirthString.isEmpty) return 'N/A';
    try {
      final birthDate = DateTime.parse(dateOfBirthString); // API DoctorInfo trả về String date
      final today = DateTime.now();
      int age = today.year - birthDate.year;
      if (today.month < birthDate.month ||
          (today.month == birthDate.month && today.day < birthDate.day)) {
        age--;
      }
      return age >= 0 ? '$age tuổi' : 'N/A';
    } catch (e) {
      print("ShowInfoDoctorPage: Lỗi tính tuổi cho '$dateOfBirthString': $e");
      return 'N/A';
    }
  }

  // Các hàm _getDegreeChipColor, _getDegreeTextColor giữ nguyên
  Color _getDegreeChipColor(String? degree) {
    if (degree == null) return Colors.grey.shade200;
    final d = degree.toLowerCase();
    if (d.contains('giáo sư') || d.contains('gs') || d.contains('pgs')) return Colors.purple.shade100;
    if (d.contains('tiến sĩ') || d.contains('ts')) return Colors.blue.shade100;
    if (d.contains('thạc sĩ') || d.contains('ths')) return Colors.green.shade100;
    if (d.contains('ckii') || d.contains('chuyên khoa 2')) return Colors.orange.shade100;
    if (d.contains('cki') || d.contains('chuyên khoa 1')) return Colors.yellow.shade100;
    if (d.contains('bác sĩ') || d.contains('bs')) return Colors.indigo.shade100;
    return Colors.grey.shade200;
  }

  Color _getDegreeTextColor(String? degree) {
    if (degree == null) return Colors.grey.shade800;
    final d = degree.toLowerCase();
    if (d.contains('giáo sư') || d.contains('gs') || d.contains('pgs')) return Colors.purple.shade700;
    if (d.contains('tiến sĩ') || d.contains('ts')) return Colors.blue.shade700;
    if (d.contains('thạc sĩ') || d.contains('ths')) return Colors.green.shade700;
    if (d.contains('ckii') || d.contains('chuyên khoa 2')) return Colors.orange.shade700;
    if (d.contains('cki') || d.contains('chuyên khoa 1')) return Colors.yellow.shade700;
    if (d.contains('bác sĩ') || d.contains('bs')) return Colors.indigo.shade700;
    return Colors.grey.shade800;
  }


  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Danh sách Bác sĩ'),
        leading: IconButton(
          icon: const Icon(Icons.arrow_back_ios_new),
          onPressed: () => Navigator.of(context).pop(),
        ),
      ),
      body: Column(
        children: [
          _buildSelectedInfoCard(),
          _buildSearchAndFilterSection(),
          Expanded(
            child: FutureBuilder<List<DoctorInfo>>(
              future: _doctorsFuture,
              builder: (context, snapshot) {
                if (snapshot.connectionState == ConnectionState.waiting) {
                  return const Center(child: CircularProgressIndicator());
                } else if (snapshot.hasError) {
                  // Hiển thị lỗi rõ ràng hơn, thông báo lỗi được lấy từ Exception ném ra bởi service
                  return Center(
                    child: Padding(
                      padding: const EdgeInsets.all(16.0),
                      child: Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          const Icon(Icons.error_outline, color: Colors.red, size: 48),
                          const SizedBox(height: 16),
                          Text(
                            'Lỗi tải danh sách bác sĩ!',
                            style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Colors.red),
                            textAlign: TextAlign.center,
                          ),
                          const SizedBox(height: 8),
                          Text(
                            // Lấy thông điệp lỗi từ snapshot.error
                            snapshot.error.toString().replaceFirst("Exception: ", ""),
                            textAlign: TextAlign.center,
                            style: TextStyle(color: Colors.grey[700]),
                          ),
                          const SizedBox(height: 20),
                          ElevatedButton.icon(
                            icon: const Icon(Icons.refresh),
                            label: const Text('Thử lại'),
                            onPressed: () {
                              setState(() {
                                _loadDoctors(); // Gọi lại API
                              });
                            },
                          ),
                        ],
                      ),
                    ),
                  );
                } else if (!snapshot.hasData || snapshot.data!.isEmpty) {
                  return _buildEmptyState('Không có bác sĩ nào được tìm thấy cho các tiêu chí này.');
                }

                if (_filteredDoctors.isEmpty && _allDoctors.isNotEmpty) {
                  return _buildEmptyState('Không tìm thấy bác sĩ nào phù hợp với bộ lọc hiện tại.');
                }

                return ListView.builder(
                  padding: const EdgeInsets.all(8.0),
                  itemCount: _filteredDoctors.length,
                  itemBuilder: (context, index) {
                    final doctor = _filteredDoctors[index];
                    return _buildDoctorCard(doctor);
                  },
                );
              },
            ),
          ),
        ],
      ),
    );
  }

  // _buildEmptyState giữ nguyên
  Widget _buildEmptyState(String message) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(20.0),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.search_off, size: 60, color: Colors.grey[400]),
            const SizedBox(height: 16),
            Text(
              message,
              textAlign: TextAlign.center,
              style: TextStyle(fontSize: 16, color: Colors.grey[600]),
            ),
          ],
        ),
      ),
    );
  }

  // _buildSelectedInfoCard và _buildInfoRow giữ nguyên
  Widget _buildSelectedInfoCard() {
    return Card(
      margin: const EdgeInsets.all(12.0),
      elevation: 2,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      child: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('Thông tin đã chọn:', style: Theme.of(context).textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold)),
            const SizedBox(height: 12),
            _buildInfoRow(Icons.business_outlined, widget.selectedDisplayInfo.branchName, Colors.blue.shade700),
            _buildInfoRow(Icons.medical_services_outlined, widget.selectedDisplayInfo.departmentName, Colors.purple.shade700),
            _buildInfoRow(Icons.star_border_outlined, widget.selectedDisplayInfo.specialtyName, Colors.indigo.shade700),
          ],
        ),
      ),
    );
  }

  Widget _buildInfoRow(IconData icon, String text, Color iconColor) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4.0),
      child: Row(
        children: [
          Icon(icon, color: iconColor, size: 20),
          const SizedBox(width: 10),
          Expanded(child: Text(text, style: TextStyle(fontSize: 14, color: Colors.grey[800]))),
        ],
      ),
    );
  }

  // _buildSearchAndFilterSection giữ nguyên
  Widget _buildSearchAndFilterSection() {
    return Card(
      margin: const EdgeInsets.symmetric(horizontal: 12.0, vertical: 8.0),
      elevation: 2,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      child: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          children: [
            TextField(
              controller: _searchController,
              decoration: InputDecoration(
                hintText: 'Tìm bác sĩ theo tên, bằng cấp...',
                prefixIcon: const Icon(Icons.search, color: Colors.grey),
                border: OutlineInputBorder(borderRadius: BorderRadius.circular(8.0), borderSide: BorderSide(color: Colors.grey.shade300)),
                enabledBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(8.0), borderSide: BorderSide(color: Colors.grey.shade300)),
                focusedBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(8.0), borderSide: BorderSide(color: Theme.of(context).primaryColor, width: 1.5)),
                contentPadding: const EdgeInsets.symmetric(vertical: 12.0, horizontal: 12.0),
              ),
            ),
            const SizedBox(height: 12),
            DropdownButtonFormField<String>(
              value: _selectedDegreeFilter,
              decoration: InputDecoration(
                prefixIcon: const Icon(Icons.filter_list, color: Colors.grey),
                border: OutlineInputBorder(borderRadius: BorderRadius.circular(8.0), borderSide: BorderSide(color: Colors.grey.shade300)),
                enabledBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(8.0), borderSide: BorderSide(color: Colors.grey.shade300)),
                focusedBorder: OutlineInputBorder(borderRadius: BorderRadius.circular(8.0), borderSide: BorderSide(color: Theme.of(context).primaryColor, width: 1.5)),
                contentPadding: const EdgeInsets.symmetric(vertical: 0, horizontal: 10),
              ),
              isExpanded: true,
              items: _degreeOptions.map((String value) {
                return DropdownMenuItem<String>(value: value, child: Text(_degreeDisplayMap[value] ?? value, overflow: TextOverflow.ellipsis));
              }).toList(),
              onChanged: (String? newValue) {
                if (newValue != null) {
                  setState(() { _selectedDegreeFilter = newValue; });
                  _applyFilters();
                }
              },
            ),
          ],
        ),
      ),
    );
  }


  // _buildDoctorCard và _buildDetailItem giữ nguyên
  Widget _buildDoctorCard(DoctorInfo doctor) {
    final String ageString = _calculateAge(doctor.dateOfBirth);
    final String imageUrl = doctor.fullImageUrl;

    return Card(
      margin: const EdgeInsets.symmetric(vertical: 8.0, horizontal: 0),
      elevation: 2,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12.0)),
      clipBehavior: Clip.antiAlias,
      child: InkWell(
        onTap: () {
          // ✅ CHUYỂN SANG MÀN HÌNH CHI TIẾT VÀ ĐẶT LỊCH
          // Chúng ta sẽ tạo màn hình DoctorBookingScreen ở bước sau
          // Truyền doctor.id sang màn hình đó
          Navigator.push(
            context,
            MaterialPageRoute(
              builder: (context) => DoctorBookingScreen(doctorId: doctor.id), // Giả sử DoctorBookingScreen đã được tạo
            ),
          );
          // ScaffoldMessenger.of(context).showSnackBar(
          //   SnackBar(content: Text('Chuyển đến chi tiết BS. ${doctor.name ?? "N/A"}')),
          // );
        },
        child: Padding(
          padding: const EdgeInsets.all(12.0),
          child: Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Stack(
                children: [
                  CircleAvatar(
                    radius: 35,
                    backgroundColor: Colors.grey.shade200,
                    backgroundImage: imageUrl.isNotEmpty
                        ? NetworkImage(imageUrl)
                        : const AssetImage(_defaultDoctorAvatarAsset) as ImageProvider,
                    onBackgroundImageError: imageUrl.isNotEmpty
                        ? (exception, stackTrace) {
                            print("ShowInfoDoctorPage: Lỗi tải ảnh NetworkImage '$imageUrl': $exception");
                          }
                        : null,
                    child: imageUrl.isEmpty ? const Icon(Icons.person, size: 35, color: Colors.white70) : null,
                  ),
                  Positioned(
                    bottom: 0,
                    right: 0,
                    child: Container(
                      padding: const EdgeInsets.all(2),
                      decoration: BoxDecoration(color: Colors.white, shape: BoxShape.circle, boxShadow: [BoxShadow(color: Colors.black.withOpacity(0.1), spreadRadius: 1, blurRadius: 1)]),
                      child: const CircleAvatar(radius: 6, backgroundColor: Colors.green),
                    ),
                  ),
                ],
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Expanded(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(doctor.name ?? 'Chưa có tên', style: Theme.of(context).textTheme.titleMedium?.copyWith(fontWeight: FontWeight.bold), maxLines: 1, overflow: TextOverflow.ellipsis),
                              if (doctor.doctorDegree != null && doctor.doctorDegree!.isNotEmpty) ...[
                                const SizedBox(height: 4),
                                Chip(
                                  label: Text(doctor.doctorDegree!, style: TextStyle(color: _getDegreeTextColor(doctor.doctorDegree), fontSize: 10, fontWeight: FontWeight.w500)),
                                  backgroundColor: _getDegreeChipColor(doctor.doctorDegree),
                                  padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 0),
                                  labelPadding: EdgeInsets.zero,
                                  materialTapTargetSize: MaterialTapTargetSize.shrinkWrap,
                                  visualDensity: VisualDensity.compact,
                                ),
                              ],
                            ],
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 6),
                    Row(children: [Icon(Icons.star, color: Colors.amber.shade600, size: 16), const SizedBox(width: 4)]), // Giữ nguyên phần rating (nếu có)
                    if (doctor.jobDescription != null && doctor.jobDescription!.isNotEmpty) ...[
                      const SizedBox(height: 8),
                      Text(doctor.jobDescription!, style: TextStyle(fontSize: 13, color: Colors.grey.shade800, height: 1.4), maxLines: 2, overflow: TextOverflow.ellipsis),
                    ],
                    const SizedBox(height: 10),
                    Wrap(
                      spacing: 12.0, runSpacing: 6.0,
                      children: [
                        if (ageString != 'N/A') _buildDetailItem(Icons.cake_outlined, ageString, Colors.blue.shade600),
                        if (doctor.gender != null && doctor.gender!.isNotEmpty) _buildDetailItem(Icons.person_outline, doctor.gender!, Colors.purple.shade600),
                        if (doctor.workingAtBranch != null && doctor.workingAtBranch!.isNotEmpty) _buildDetailItem(Icons.location_on_outlined, doctor.workingAtBranch!, Colors.green.shade600, flexText: true),
                        if (doctor.phone != null && doctor.phone!.isNotEmpty) _buildDetailItem(Icons.phone_outlined, doctor.phone!, Colors.orange.shade600),
                      ],
                    ),
                    const SizedBox(height: 8),
                    Align(
                      alignment: Alignment.centerRight,
                      child: ElevatedButton(
                        onPressed: () {
                          // ✅ CHUYỂN SANG MÀN HÌNH CHI TIẾT VÀ ĐẶT LỊCH
                          Navigator.push(
                            context,
                            MaterialPageRoute(
                              builder: (context) => DoctorBookingScreen(doctorId: doctor.id), // Giả sử DoctorBookingScreen đã được tạo
                            ),
          );
                        },
                        style: ElevatedButton.styleFrom(padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 8), textStyle: const TextStyle(fontSize: 13, fontWeight: FontWeight.w500)),
                        child: const Text('Xem chi tiết & Đặt lịch'), // Đổi tên nút
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildDetailItem(IconData icon, String text, Color iconColor, {bool flexText = false}) {
    Widget textWidget = Text(text, style: TextStyle(fontSize: 12, color: Colors.grey.shade700), overflow: TextOverflow.ellipsis, maxLines: 1);
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        Icon(icon, size: 14, color: iconColor),
        const SizedBox(width: 5),
        flexText ? Expanded(child: textWidget) : textWidget,
      ],
    );
  }
}

// ✅ Dummy DoctorBookingScreen để code trên không báo lỗi import
// Bạn sẽ tạo file này chi tiết ở bước sau
// class DoctorBookingScreen extends StatelessWidget {
//   final String doctorId;
//   const DoctorBookingScreen({Key? key, required this.doctorId}) : super(key: key);

//   @override
//   Widget build(BuildContext context) {
//     return Scaffold(
//       appBar: AppBar(title: Text('Chi tiết Bác sĩ ID: $doctorId')), // <-- TIÊU ĐỀ GIỐNG HÌNH BẠN GỬI
//       body: Center(child: Text('Đây là màn hình chi tiết và đặt lịch cho bác sĩ ID: $doctorId')), // <-- TEXT GIỐNG HÌNH BẠN GỬI
//     );
//   }
// }
