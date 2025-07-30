// lib/PageHome/Booking/ChoseDepartmentScreen.dart
import 'package:flutter/material.dart';
import 'package:app_demo/Model/branch_model.dart';
import '../../../Model/DepartmentModel.dart';
import '../../../Services/DepartmentApiService.dart';
import './ChoseSpecialtyScreen.dart'; // Màn hình chọn chuyên khoa

class ChoseDepartmentScreen extends StatefulWidget {
  final Branch selectedBranch;

  const ChoseDepartmentScreen({super.key, required this.selectedBranch});

  @override
  State<ChoseDepartmentScreen> createState() => _ChoseDepartmentScreenState();
}

class _ChoseDepartmentScreenState extends State<ChoseDepartmentScreen>
    with TickerProviderStateMixin {
  late Future<List<Department>> _departmentsFuture;
  final DepartmentApiService _apiService = DepartmentApiService();

  AnimationController?
  _gridItemAnimationController; // Controller cho animation của từng item
  int? _selectedIndex;
  final Color _primaryColor = const Color(0xFF1976D2);

  @override
  void initState() {
    super.initState();
    _departmentsFuture = _apiService.fetchAllDepartments();
    // API của bạn lấy tất cả department, không theo branch.
    // Nếu DepartmentModel có branchId, bạn có thể lọc ở client:
    // _departmentsFuture = _apiService.fetchAllDepartments().then((allDepts) {
    //   return allDepts.where((dept) => dept.branchId == widget.selectedBranch.idBranch).toList();
    // });

    _gridItemAnimationController = AnimationController(
      duration: const Duration(
        milliseconds: 500,
      ), // Duration cho animation của item
      vsync: this,
    );
    // Không cần forward ở đây, sẽ trigger khi item được build
  }

  @override
  void dispose() {
    _gridItemAnimationController?.dispose();
    super.dispose();
  }

  void _retryFetchDepartments() {
    setState(() {
      _departmentsFuture = _apiService.fetchAllDepartments();
      _selectedIndex = null; // Reset selection
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.grey[100],
      appBar: AppBar(
        backgroundColor: _primaryColor,
        elevation: 1,
        title: const Text(
          "Chọn Khoa Khám",
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
            _buildSelectedBranchInfoCard(),
            Padding(
              padding: const EdgeInsets.fromLTRB(20, 16, 20, 12),
              child: Row(
                children: [
                  Icon(
                    Icons.local_hospital_outlined,
                    color: _primaryColor,
                    size: 22,
                  ),
                  const SizedBox(width: 10),
                  Text(
                    "Chọn khoa khám phù hợp",
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
              child: FutureBuilder<List<Department>>(
                future: _departmentsFuture,
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
                    return _buildErrorWidget("khoa", snapshot.error.toString());
                  }
                  if (!snapshot.hasData || snapshot.data!.isEmpty) {
                    return _buildEmptyWidget("khoa");
                  }
                  final departments = snapshot.data!;
                  // Chạy animation khi GridView được build
                  WidgetsBinding.instance.addPostFrameCallback((_) {
                    if (mounted &&
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
                        ), // Điều chỉnh tỷ lệ
                    itemCount: departments.length,
                    itemBuilder: (context, index) {
                      return AnimatedBuilder(
                        animation: _gridItemAnimationController!,
                        builder: (context, child) {
                          final animationValue =
                              _gridItemAnimationController?.value ?? 0.0;
                          // Staggered animation
                          double itemDelayFactor = (index /
                                  (departments.length * 2.0))
                              .clamp(0.0, 0.5); // Tăng độ trễ
                          double currentItemAnimationValue = (animationValue -
                                  itemDelayFactor)
                              .clamp(0.0, 1.0);
                          return Transform.translate(
                            offset: Offset(
                              0,
                              (1 - currentItemAnimationValue) * 30,
                            ), // Khoảng cách translate
                            child: Opacity(
                              opacity: currentItemAnimationValue,
                              child: _buildDepartmentCard(
                                departments[index],
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

  Widget _buildSelectedBranchInfoCard() {
    return Card(
      margin: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      elevation: 1,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Row(
          children: [
            ClipRRect(
              borderRadius: BorderRadius.circular(8),
              child: SizedBox(
                width: 50,
                height: 50,
                child:
                    (widget.selectedBranch.imageUrl != null &&
                            widget.selectedBranch.imageUrl!.isNotEmpty)
                        ? Image.network(
                          widget.selectedBranch.imageUrl!,
                          fit: BoxFit.cover,
                          errorBuilder:
                              (c, e, s) => Icon(
                                Icons.business,
                                size: 28,
                                color: Colors.grey[400],
                              ),
                        )
                        : Icon(
                          Icons.business,
                          size: 28,
                          color: Colors.grey[400],
                        ),
              ),
            ),
            const SizedBox(width: 16),
            Expanded(
              child: Text(
                "Chi nhánh: ${widget.selectedBranch.branchName}",
                style: const TextStyle(
                  fontSize: 15,
                  fontWeight: FontWeight.w500,
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildDepartmentCard(Department department, int index) {
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
          Future.delayed(const Duration(milliseconds: 200), () {
            if (mounted) {
              Navigator.push(
                context,
                MaterialPageRoute(
                  builder:
                      (context) => ChoseSpecialtyScreen(
                        selectedBranch: widget.selectedBranch,
                        selectedDepartment: department,
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
                  (department.imageUrl != null &&
                          department.imageUrl!.isNotEmpty)
                      ? ClipOval(
                        child: Image.network(
                          department.imageUrl!,
                          fit: BoxFit.cover,
                          width: 58,
                          height: 58,
                          errorBuilder:
                              (c, e, s) =>
                                  _buildDefaultDepartmentIcon(isSelected),
                        ),
                      )
                      : _buildDefaultDepartmentIcon(isSelected),
            ),
            const SizedBox(height: 12),
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 8),
              child: Text(
                department.departmentName,
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

  Widget _buildDefaultDepartmentIcon(bool isSelected) {
    return Icon(
      Icons.local_hospital,
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
              "Lỗi tải $itemType",
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
              style: TextStyle(color: Colors.grey[700]),
            ),
            const SizedBox(height: 20),
            ElevatedButton.icon(
              icon: const Icon(Icons.refresh, size: 18),
              label: const Text("Thử lại"),
              onPressed: _retryFetchDepartments,
              style: ElevatedButton.styleFrom(
                backgroundColor: _primaryColor,
                foregroundColor: Colors.white,
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
              "Hiện tại chưa có $itemType nào được tìm thấy.",
              textAlign: TextAlign.center,
              style: TextStyle(color: Colors.grey[600]),
            ),
            const SizedBox(height: 20),
            ElevatedButton.icon(
              icon: const Icon(Icons.refresh, size: 18),
              label: const Text("Tải lại"),
              onPressed: _retryFetchDepartments,
              style: ElevatedButton.styleFrom(
                backgroundColor: _primaryColor,
                foregroundColor: Colors.white,
              ),
            ),
          ],
        ),
      ),
    );
  }
}
