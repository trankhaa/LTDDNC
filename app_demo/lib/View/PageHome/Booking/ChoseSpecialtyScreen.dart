// lib/View/PageHome/Booking/ChoseSpecialtyScreen.dart
import 'package:flutter/material.dart';
// Đảm bảo bạn đã có các model và service này ở đúng đường dẫn
import '../../../Model/branch_model.dart';
import '../../../Model/DepartmentModel.dart';
import '../../../Model/Specialty_model.dart'; // Đổi tên file import nếu cần
import '../../../Services/specialty_api_service.dart';
import '../../../Services/branch_api_service.dart'; // Đổi tên file import nếu cần

// IMPORT LỚP TỪ Showinfodoctor.dart
import './Showinfodoctor.dart'; // Giả sử cùng thư mục

class ChoseSpecialtyScreen extends StatefulWidget {
  final Branch selectedBranch;
  final Department selectedDepartment;

  const ChoseSpecialtyScreen({
    super.key,
    required this.selectedBranch,
    required this.selectedDepartment,
  });

  @override
  State<ChoseSpecialtyScreen> createState() => _ChoseSpecialtyScreenState();
}

class _ChoseSpecialtyScreenState extends State<ChoseSpecialtyScreen>
    with TickerProviderStateMixin {
  late Future<List<Specialty>> _specialtiesFuture;
  final SpecialtyApiService _apiService = SpecialtyApiService();

  AnimationController? _gridItemAnimationController;
  int? _selectedIndex;
  final Color _primaryColor = const Color(
    0xFF1976D2,
  ); // Giữ màu này nếu bạn thích

  @override
  void initState() {
    super.initState();
    _specialtiesFuture = _fetchSpecialtiesByDepartment();

    _gridItemAnimationController = AnimationController(
      duration: const Duration(milliseconds: 500),
      vsync: this,
    );
  }

  Future<List<Specialty>> _fetchSpecialtiesByDepartment() {
    // Bỏ async ở đây
    try {
      // Gọi service, service đã là async và trả về Future<List<Specialty>>
      return _apiService // Trả về trực tiếp Future từ service
          .fetchSpecialtiesByDepartment(widget.selectedDepartment.idDepartment);
    } catch (e) {
      print("ChoseSpecialtyScreen: Error in _fetchSpecialtiesByDepartment: $e");
      // Khi ném lỗi, FutureBuilder sẽ tự động bắt và vào nhánh snapshot.hasError
      // Trả về một Future bị lỗi
      return Future.error(
        Exception(
          "Lỗi tải chuyên khoa: ${e.toString().replaceFirst("Exception: ", "")}",
        ),
      );
    }
  }

  void _retryFetchSpecialties() {
    setState(() {
      _specialtiesFuture = _fetchSpecialtiesByDepartment();
      _selectedIndex = null;
      _gridItemAnimationController?.reset();
    });
  }

  @override
  void dispose() {
    _gridItemAnimationController?.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.grey[100],
      appBar: AppBar(
        backgroundColor: _primaryColor,
        elevation: 1,
        title: const Text(
          "Chọn Chuyên Khoa",
          style: TextStyle(
            fontSize: 20,
            fontWeight: FontWeight.bold,
            color: Colors.white,
          ),
        ),
        leading: IconButton(
          icon: const Icon(
            Icons.arrow_back_ios_new,
            color: Colors.white,
            size: 20,
          ),
          onPressed: () => Navigator.pop(context),
        ),
      ),
      body: SafeArea(
        child: Column(
          children: [
            _buildSelectionSummaryCard(),
            Padding(
              padding: const EdgeInsets.fromLTRB(20, 16, 20, 12),
              child: Row(
                children: [
                  Icon(
                    Icons.medical_information_outlined,
                    color: _primaryColor,
                    size: 22,
                  ),
                  const SizedBox(width: 10),
                  Text(
                    "Chọn chuyên khoa phù hợp",
                    style: TextStyle(
                      fontSize: 18,
                      fontWeight: FontWeight.bold,
                      color: Colors.grey[800],
                    ),
                  ),
                ],
              ),
            ),
            Expanded(
              child: FutureBuilder<List<Specialty>>(
                future: _specialtiesFuture,
                builder: (context, snapshot) {
                  if (snapshot.connectionState == ConnectionState.waiting) {
                    return Center(
                      child: CircularProgressIndicator(
                        valueColor: AlwaysStoppedAnimation<Color>(
                          _primaryColor,
                        ),
                      ),
                    );
                  }
                  if (snapshot.hasError) {
                    // Lỗi đã được xử lý và log trong _fetchSpecialtiesByDepartment
                    // và SpecialtyApiService, ở đây chỉ cần hiển thị UI lỗi
                    return _buildErrorWidget(
                      "Lỗi Tải Chuyên Khoa", // Tiêu đề lỗi chung
                      snapshot.error
                          .toString(), // Hiển thị thông báo lỗi từ Exception
                    );
                  }
                  // Service đã xử lý trường hợp 404 và trả về []
                  // nên không cần kiểm tra snapshot.data == null ở đây nữa
                  // nếu API trả về 200 nhưng danh sách rỗng.
                  if (snapshot.data!.isEmpty) {
                    return _buildEmptyWidget("chuyên khoa");
                  }

                  final specialties = snapshot.data!;
                  WidgetsBinding.instance.addPostFrameCallback((_) {
                    if (mounted &&
                        _gridItemAnimationController?.status !=
                            AnimationStatus.forward &&
                        _gridItemAnimationController?.status !=
                            AnimationStatus.completed) {
                      _gridItemAnimationController?.forward();
                    }
                  });

                  return GridView.builder(
                    padding: const EdgeInsets.all(16),
                    physics: const BouncingScrollPhysics(),
                    gridDelegate:
                        const SliverGridDelegateWithFixedCrossAxisCount(
                          crossAxisCount: 2,
                          crossAxisSpacing: 12,
                          mainAxisSpacing: 12,
                          childAspectRatio: 0.9,
                        ),
                    itemCount: specialties.length,
                    itemBuilder: (context, index) {
                      return AnimatedBuilder(
                        animation: _gridItemAnimationController!,
                        builder: (context, child) {
                          final animationValue =
                              _gridItemAnimationController?.value ?? 0.0;
                          double itemDelayFactor = (index /
                                  (specialties.length * 1.5))
                              .clamp(0.0, 0.7);
                          double currentItemAnimationValue = (animationValue -
                                  itemDelayFactor)
                              .clamp(0.0, 1.0);

                          return Transform.translate(
                            offset: Offset(
                              0,
                              (1 - currentItemAnimationValue) * 25,
                            ),
                            child: Opacity(
                              opacity: currentItemAnimationValue,
                              child: _buildSpecialtyCard(
                                specialties[index],
                                index,
                              ),
                            ),
                          );
                        },
                      );
                    },
                  );
                },
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildSelectionSummaryCard() {
    return Card(
      margin: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      elevation: 1,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      child: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                ClipRRect(
                  borderRadius: BorderRadius.circular(8),
                  child: SizedBox(
                    width: 40,
                    height: 40,
                    child:
                        (widget.selectedBranch.imageUrl != null &&
                                widget.selectedBranch.imageUrl!.isNotEmpty)
                            ? Image.network(
                              // Giả sử model Branch đã có getter fullImageUrl hoặc bạn ghép host ở đây
                              widget.selectedBranch.imageUrl!.startsWith('http')
                                  ? widget.selectedBranch.imageUrl!
                                  : BranchApiService.host +
                                      widget.selectedBranch.imageUrl!,
                              fit: BoxFit.cover,
                              errorBuilder:
                                  (c, e, s) => Icon(
                                    Icons.business,
                                    size: 24,
                                    color: Colors.grey[400],
                                  ),
                            )
                            : Icon(
                              Icons.business,
                              size: 24,
                              color: Colors.grey[400],
                            ),
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: Text(
                    "Chi nhánh: ${widget.selectedBranch.branchName}",
                    style: const TextStyle(
                      fontWeight: FontWeight.w500,
                      fontSize: 14,
                    ),
                  ),
                ),
              ],
            ),
            const Divider(height: 16, thickness: 0.5),
            Row(
              children: [
                ClipRRect(
                  borderRadius: BorderRadius.circular(8),
                  child: SizedBox(
                    width: 40,
                    height: 40,
                    child:
                        (widget.selectedDepartment.imageUrl != null &&
                                widget
                                    .selectedDepartment
                                    .imageUrl!
                                    .isNotEmpty)
                            ? Image.network(
                              // Giả sử model DepartmentModel đã có getter fullImageUrlDepartment hoặc bạn ghép host ở đây
                              widget.selectedDepartment.imageUrl!
                                      .startsWith('http')
                                  ? widget
                                      .selectedDepartment
                                      .imageUrl!
                                  : BranchApiService.host +
                                      widget
                                          .selectedDepartment
                                          .imageUrl!,
                              fit: BoxFit.cover,
                              errorBuilder:
                                  (c, e, s) => Icon(
                                    Icons.local_hospital_outlined,
                                    size: 24,
                                    color: Colors.grey[400],
                                  ),
                            )
                            : Icon(
                              Icons.local_hospital_outlined,
                              size: 24,
                              color: Colors.grey[400],
                            ),
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: Text(
                    "Khoa: ${widget.selectedDepartment.departmentName}",
                    style: const TextStyle(
                      fontWeight: FontWeight.w500,
                      fontSize: 14,
                    ),
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildSpecialtyCard(Specialty specialty, int index) {
    final isSelected = _selectedIndex == index;
    return Card(
      elevation: isSelected ? 4 : 1.5,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(16),
        side: BorderSide(
          color: isSelected ? _primaryColor : Colors.grey[300]!,
          width: isSelected ? 1.5 : 0.8,
        ),
      ),
      child: InkWell(
        borderRadius: BorderRadius.circular(16),
        onTap: () {
          setState(() => _selectedIndex = index);

          print(
            "ChoseSpecialtyScreen: Navigating to ShowInfoDoctorPage with:\n"
            "  Branch ID: ${widget.selectedBranch.idBranch} (Name: ${widget.selectedBranch.branchName})\n"
            "  Department ID: ${widget.selectedDepartment.idDepartment} (Name: ${widget.selectedDepartment.departmentName})\n"
            // SỬA Ở ĐÂY: Sử dụng thuộc tính đúng của model Specialty của bạn
            // Model Specialty bạn cung cấp có thuộc tính là `id` (từ file specialty_model.dart)
            // hoặc `idSpecialty` (từ file service). Cần thống nhất.
            // Giả sử model của bạn là `Specialty` với thuộc tính `id`
            "  Specialty ID: ${specialty.idSpecialty} (Name: ${specialty.specialtyName})",
          );

          Future.delayed(const Duration(milliseconds: 200), () {
            if (mounted) {
              Navigator.push(
                context,
                MaterialPageRoute(
                  builder:
                      (context) => ShowInfoDoctorPage(
                        branchId: widget.selectedBranch.idBranch,
                        departmentId: widget.selectedDepartment.idDepartment,
                        // SỬA Ở ĐÂY TƯƠNG TỰ
                        specialtyId:
                            specialty.idSpecialty, // Giả sử model có thuộc tính `id`
                        selectedDisplayInfo: SelectedCriteriaDisplayInfo(
                          branchName: widget.selectedBranch.branchName,
                          departmentName:
                              widget.selectedDepartment.departmentName,
                          specialtyName: specialty.specialtyName,
                        ),
                      ),
                ),
              );
            }
          });
        },
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            CircleAvatar(
              radius: 30,
              backgroundColor:
                  isSelected
                      ? _primaryColor.withOpacity(0.15)
                      : Colors.grey[200],
              child:
                  (specialty.imageUrlSpecialty != null &&
                          // SỬA Ở ĐÂY: Sử dụng getter fullImageUrlSpecialty
                          specialty.fullImageUrlSpecialty.isNotEmpty)
                      ? ClipOval(
                        child: Image.network(
                          // SỬA Ở ĐÂY: Sử dụng getter fullImageUrlSpecialty
                          specialty.fullImageUrlSpecialty,
                          fit: BoxFit.cover,
                          width: 58,
                          height: 58,
                          errorBuilder:
                              (context, error, stackTrace) =>
                                  _buildDefaultSpecialtyIcon(isSelected),
                        ),
                      )
                      : _buildDefaultSpecialtyIcon(isSelected),
            ),
            const SizedBox(height: 12),
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 8),
              child: Text(
                specialty.specialtyName,
                textAlign: TextAlign.center,
                maxLines: 2,
                overflow: TextOverflow.ellipsis,
                style: TextStyle(
                  fontSize: 14,
                  fontWeight: FontWeight.w600,
                  color: isSelected ? _primaryColor : Colors.grey[800],
                ),
              ),
            ),
            if (isSelected)
              Padding(
                padding: const EdgeInsets.only(top: 6.0),
                child: Icon(Icons.check_circle, color: _primaryColor, size: 18),
              ),
          ],
        ),
      ),
    );
  }

  Widget _buildDefaultSpecialtyIcon(bool isSelected) {
    return Icon(
      Icons.medical_information_sharp,
      color: isSelected ? _primaryColor : _primaryColor.withOpacity(0.6),
      size: 28,
    );
  }

  Widget _buildErrorWidget(String itemType, String error) {
    String displayError = error.replaceFirst("Exception: ", "");
    if (displayError.toLowerCase().contains("socketexception") ||
        displayError.toLowerCase().contains("connection refused") ||
        displayError.toLowerCase().contains("timeout")) {
      displayError =
          "Không thể kết nối đến máy chủ. Vui lòng kiểm tra lại đường truyền mạng.";
    } else if (displayError.toLowerCase().contains("invalid data format") ||
        displayError.toLowerCase().contains("unexpected character")) {
      displayError =
          "Dữ liệu nhận được từ máy chủ không đúng định dạng. Vui lòng thử lại sau.";
    }
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(20.0),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.error_outline_rounded, color: Colors.red[600], size: 50),
            const SizedBox(height: 16),
            Text(
              // Sửa tiêu đề lỗi
              itemType, // Thay vì "Lỗi tải $itemType"
              style: TextStyle(
                fontSize: 18,
                fontWeight: FontWeight.bold,
                color: Colors.red[700],
              ),
            ),
            const SizedBox(height: 8),
            Text(
              displayError,
              textAlign: TextAlign.center,
              style: TextStyle(color: Colors.grey[700], fontSize: 14),
            ),
            const SizedBox(height: 20),
            ElevatedButton.icon(
              icon: const Icon(Icons.refresh, size: 18),
              label: const Text("Thử lại"),
              onPressed: _retryFetchSpecialties,
              style: ElevatedButton.styleFrom(
                backgroundColor: _primaryColor,
                foregroundColor: Colors.white,
                padding: const EdgeInsets.symmetric(
                  horizontal: 24,
                  vertical: 12,
                ),
                textStyle: const TextStyle(fontWeight: FontWeight.bold),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildEmptyWidget(String itemType) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(20.0),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.search_off_rounded, color: Colors.grey[400], size: 50),
            const SizedBox(height: 16),
            Text(
              "Không có $itemType",
              style: TextStyle(
                fontSize: 18,
                fontWeight: FontWeight.bold,
                color: Colors.grey,
              ),
            ),
            const SizedBox(height: 8),
            Text(
              "Hiện tại chưa có $itemType nào được tìm thấy cho khoa này.",
              textAlign: TextAlign.center,
              style: TextStyle(color: Colors.grey[600], fontSize: 14),
            ),
            const SizedBox(height: 20),
            ElevatedButton.icon(
              icon: const Icon(Icons.refresh, size: 18),
              label: const Text("Tải lại"),
              onPressed: _retryFetchSpecialties,
              style: ElevatedButton.styleFrom(
                backgroundColor: _primaryColor,
                foregroundColor: Colors.white,
                padding: const EdgeInsets.symmetric(
                  horizontal: 24,
                  vertical: 12,
                ),
                textStyle: const TextStyle(fontWeight: FontWeight.bold),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
