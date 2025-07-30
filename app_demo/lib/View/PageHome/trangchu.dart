// lib/PageHome/trangchu.dart
import 'package:flutter/material.dart';
import '../../services/auth_service.dart'; // Đảm bảo đường dẫn này đúng
import '../../Model/user_model.dart'; // Đảm bảo đường dẫn này đúng

class TrangChuPage extends StatefulWidget {
  const TrangChuPage({super.key});

  @override
  State<TrangChuPage> createState() => _TrangChuPageState();
}

class _TrangChuPageState extends State<TrangChuPage> {
  // Để quản lý slider quảng cáo
  final PageController _pageController = PageController();
  int _currentPage = 0;

  // === THÊM CÁC BIẾN VÀ HÀM ĐỂ LẤY DỮ LIỆU NGƯỜI DÙNG ===
  final AuthService _authService = AuthService();
  User? _currentUser;
  bool _isLoadingUser = true;

  @override
  void initState() {
    super.initState();
    _loadUserData(); // Lấy dữ liệu người dùng khi màn hình được khởi tạo

    // Lắng nghe sự thay đổi trang của PageView
    _pageController.addListener(() {
      if (_pageController.hasClients && _pageController.page != null) {
        setState(() {
          _currentPage = _pageController.page!.round();
        });
      }
    });
  }

  /// Hàm mới để lấy thông tin người dùng từ SharedPreferences
  Future<void> _loadUserData() async {
    final user = await _authService.getSavedUser();
    if (mounted) {
      setState(() {
        _currentUser = user;
        _isLoadingUser = false;
      });
    }
  }
  // =========================================================

  @override
  void dispose() {
    _pageController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      // Màu nền tổng thể cho trang
      backgroundColor: const Color(0xFFF0F4F7),
      body: SingleChildScrollView(
        child: Column(
          children: [
            _buildHeader(), // Phần Header màu xanh
            const SizedBox(height: 24),
            _buildPromoCarousel(), // Phần Slider quảng cáo
            const SizedBox(height: 24),
            _buildQuickActions(), // Các nút Hồ sơ, Kết quả, Bác sĩ AI
            const SizedBox(height: 24),
            _buildSectionHeader(title: "Bác sĩ nổi bật", onSeeMore: () {}),
            const SizedBox(height: 16),
            _buildFeaturedDoctorsList(), // Danh sách bác sĩ
            const SizedBox(height: 24),
            _buildMedicalServices(), // Dịch vụ y tế
            const SizedBox(height: 24),
            _buildSpecialtySection(), // Chuyên khoa
            const SizedBox(height: 24),
            _buildHealthTipsSection(), // Mẹo sức khỏe
            const SizedBox(height: 24),
            _buildBookingSection(), // Đặt lịch khám
            const SizedBox(height: 24),
            _buildEmergencySection(), // Cấp cứu
            const SizedBox(height: 24),
            _buildSectionHeader(title: "Tin tức - Sự kiện", onSeeMore: () {}),
            const SizedBox(height: 16),
            _buildNewsList(), // Danh sách tin tức
            const SizedBox(height: 24),
            _buildMedicalFacilities(), // Cơ sở y tế
            const SizedBox(height: 24),
            _buildHealthCheckPackages(), // Gói khám sức khỏe
            const SizedBox(height: 24),
            _buildExpertConsultation(), // Tư vấn chuyên gia
            const SizedBox(height: 24),
            _buildPharmacySection(), // Nhà thuốc
            const SizedBox(height: 24),
            _buildHealthStats(), // Thống kê sức khỏe
            const SizedBox(height: 24),
            _buildFooterInfo(), // Thông tin liên hệ
            const SizedBox(height: 40),
          ],
        ),
      ),
    );
  }

  // === CÁC WIDGET CON ĐƯỢC TÁCH RA ===

  /// 1. Widget Header màu xanh phía trên (ĐÃ CẬP NHẬT)
  Widget _buildHeader() {
    return Container(
      padding: const EdgeInsets.only(top: 50, left: 20, right: 20, bottom: 20),
      decoration: const BoxDecoration(
        color: Color(0xFF2D6A8E),
        borderRadius: BorderRadius.only(
          bottomLeft: Radius.circular(30),
          bottomRight: Radius.circular(30),
        ),
      ),
      child: Column(
        children: [
          // Phần Chào bạn
          Row(
            children: [
              const CircleAvatar(
                radius: 25,
                backgroundColor: Colors.white,
                child: Icon(Icons.person, size: 30, color: Color(0xFF2D6A8E)),
              ),
              const SizedBox(width: 15),
              Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Text(
                    "Chào bạn",
                    style: TextStyle(color: Colors.white70, fontSize: 14),
                  ),
                  // === HIỂN THỊ TÊN NGƯỜI DÙNG THẬT ===
                  Text(
                    _isLoadingUser
                        ? "Đang tải..."
                        : (_currentUser?.name ??
                            "Khách"), // Hiển thị tên hoặc "Khách"
                    style: const TextStyle(
                      color: Colors.white,
                      fontSize: 18,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  // ==================================
                ],
              ),
            ],
          ),
          const SizedBox(height: 20),
          // Phần tìm kiếm
          TextField(
            decoration: InputDecoration(
              hintText: "Tìm bác sĩ,...",
              hintStyle: TextStyle(color: Colors.grey[600]),
              prefixIcon: Icon(Icons.search, color: Colors.grey[600]),
              filled: true,
              fillColor: Colors.white,
              contentPadding: const EdgeInsets.symmetric(vertical: 0),
              border: OutlineInputBorder(
                borderRadius: BorderRadius.circular(30),
                borderSide: BorderSide.none,
              ),
            ),
          ),
        ],
      ),
    );
  }

  /// 2. Widget Slider/Carousel quảng cáo
  Widget _buildPromoCarousel() {
    final List<String> banners = [
      'assets/banner1.png',
      'assets/banner1.png',
      'assets/banner1.png',
    ];

    return Column(
      children: [
        SizedBox(
          height: 160,
          child: PageView.builder(
            controller: _pageController,
            itemCount: banners.length,
            itemBuilder: (context, index) {
              return Container(
                margin: const EdgeInsets.symmetric(horizontal: 20),
                child: ClipRRect(
                  borderRadius: BorderRadius.circular(20),
                  child: Image.asset(
                    banners[index],
                    fit: BoxFit.cover,
                    errorBuilder: (context, error, stackTrace) {
                      return Container(
                        color: Colors.grey.shade300,
                        child: const Center(
                          child: Icon(Icons.image_not_supported,
                              color: Colors.grey),
                        ),
                      );
                    },
                  ),
                ),
              );
            },
          ),
        ),
        const SizedBox(height: 12),
        Row(
          mainAxisAlignment: MainAxisAlignment.center,
          children: List.generate(banners.length, (index) {
            return Container(
              width: 8,
              height: 8,
              margin: const EdgeInsets.symmetric(horizontal: 4),
              decoration: BoxDecoration(
                shape: BoxShape.circle,
                color: _currentPage == index
                    ? const Color(0xFF2D6A8E)
                    : Colors.grey.shade300,
              ),
            );
          }),
        ),
      ],
    );
  }

  /// 3. Widget 3 nút chức năng nhanh
  Widget _buildQuickActions() {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 20),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceAround,
        children: [
          _buildActionButton(icon: Icons.folder_copy_outlined, label: "Hồ sơ"),
          _buildActionButton(icon: Icons.bar_chart_outlined, label: "Kết quả"),
          _buildActionButton(icon: Icons.auto_awesome, label: "Bác sĩ AI"),
        ],
      ),
    );
  }

  Widget _buildActionButton({required IconData icon, required String label}) {
    return Column(
      children: [
        Container(
          width: 80,
          height: 80,
          decoration: BoxDecoration(
            color: const Color(0xFFE0E8EF),
            borderRadius: BorderRadius.circular(15),
          ),
          child: Icon(icon, size: 40, color: const Color(0xFF2D6A8E)),
        ),
        const SizedBox(height: 8),
        Text(
          label,
          style: const TextStyle(
            fontWeight: FontWeight.bold,
            color: Colors.black87,
          ),
        ),
      ],
    );
  }

  /// 4. Widget chung cho tiêu đề các mục (Bác sĩ, Tin tức)
  Widget _buildSectionHeader({
    required String title,
    required VoidCallback onSeeMore,
  }) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 20),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(
            title,
            style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
          ),
          InkWell(
            onTap: onSeeMore,
            child: const Row(
              children: [
                Text("Xem thêm", style: TextStyle(color: Color(0xFF2D6A8E))),
                Icon(
                  Icons.arrow_forward_ios,
                  size: 14,
                  color: Color(0xFF2D6A8E),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  /// 5. Widget danh sách bác sĩ nổi bật (dạng cuộn ngang)
  Widget _buildFeaturedDoctorsList() {
    return SizedBox(
      height: 180,
      child: ListView(
        scrollDirection: Axis.horizontal,
        padding: const EdgeInsets.only(left: 20),
        children: [
          _buildDoctorCard(
            name: "BS. Nguyễn Văn A",
            specialty: "Chuyên khoa Nội Khoa",
            imagePath: "assets/doctor1.png",
          ),
          _buildDoctorCard(
            name: "BS. Trần Thị B",
            specialty: "Chuyên khoa Da Liễu",
            imagePath: "assets/doctor2.png",
          ),
          _buildDoctorCard(
            name: "BS. Trần Hữu C",
            specialty: "Chuyên khoa Nhi Khoa",
            imagePath: "assets/doctor3.png",
          ),
        ],
      ),
    );
  }

  Widget _buildDoctorCard({
    required String name,
    required String specialty,
    required String imagePath,
  }) {
    return Container(
      width: 140,
      margin: const EdgeInsets.only(right: 16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(15),
        boxShadow: [
          BoxShadow(
            color: Colors.grey.withOpacity(0.1),
            spreadRadius: 2,
            blurRadius: 5,
            offset: const Offset(0, 3),
          ),
        ],
      ),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          CircleAvatar(
            radius: 40,
            backgroundColor: Colors.grey.shade200,
            backgroundImage: AssetImage(imagePath),
            onBackgroundImageError: (exception, stackTrace) {
              // This is called when the image fails to load
            },
            child: ClipOval(
              child: Image.asset(
                imagePath,
                fit: BoxFit.cover,
                errorBuilder: (context, error, stackTrace) {
                  return const Icon(Icons.person, size: 50, color: Colors.grey);
                },
              ),
            ),
          ),
          const SizedBox(height: 10),
          Text(name, style: const TextStyle(fontWeight: FontWeight.bold)),
          const SizedBox(height: 4),
          Text(
            specialty,
            style: const TextStyle(color: Colors.grey, fontSize: 12),
            textAlign: TextAlign.center,
          ),
        ],
      ),
    );
  }

  /// 6. Widget Dịch vụ y tế
  Widget _buildMedicalServices() {
    return Column(
      children: [
        _buildSectionHeader(title: "Dịch vụ y tế", onSeeMore: () {}),
        const SizedBox(height: 16),
        Padding(
          padding: const EdgeInsets.symmetric(horizontal: 20),
          child: GridView.count(
            crossAxisCount: 2,
            shrinkWrap: true,
            physics: const NeverScrollableScrollPhysics(),
            crossAxisSpacing: 16,
            mainAxisSpacing: 16,
            childAspectRatio: 1.3,
            children: [
              _buildServiceCard(
                icon: Icons.medical_services,
                title: "Khám tổng quát",
                description: "Khám sức khỏe định kỳ",
                color: const Color(0xFF4CAF50),
              ),
              _buildServiceCard(
                icon: Icons.medication,
                title: "Cấp thuốc",
                description: "Thuốc theo đơn",
                color: const Color(0xFF2196F3),
              ),
              _buildServiceCard(
                icon: Icons.science,
                title: "Xét nghiệm",
                description: "Xét nghiệm máu, nước tiểu",
                color: const Color(0xFFFF9800),
              ),
              _buildServiceCard(
                icon: Icons.healing,
                title: "Vật lý trị liệu",
                description: "Phục hồi chức năng",
                color: const Color(0xFF9C27B0),
              ),
            ],
          ),
        ),
      ],
    );
  }

  Widget _buildServiceCard({
    required IconData icon,
    required String title,
    required String description,
    required Color color,
  }) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(15),
        boxShadow: [
          BoxShadow(
            color: Colors.grey.withOpacity(0.1),
            spreadRadius: 2,
            blurRadius: 5,
          ),
        ],
      ),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(icon, size: 40, color: color),
          const SizedBox(height: 8),
          Text(
            title,
            style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 14),
          ),
          const SizedBox(height: 4),
          Text(
            description,
            style: const TextStyle(color: Colors.grey, fontSize: 12),
            textAlign: TextAlign.center,
          ),
        ],
      ),
    );
  }

  /// 7. Widget Chuyên khoa
  Widget _buildSpecialtySection() {
    return Column(
      children: [
        _buildSectionHeader(title: "Chuyên khoa", onSeeMore: () {}),
        const SizedBox(height: 16),
        SizedBox(
          height: 120,
          child: ListView(
            scrollDirection: Axis.horizontal,
            padding: const EdgeInsets.only(left: 20),
            children: [
              _buildSpecialtyCard(
                  "Nội khoa", Icons.favorite, const Color(0xFFF44336)),
              _buildSpecialtyCard("Ngoại khoa", Icons.medical_services,
                  const Color(0xFF2196F3)),
              _buildSpecialtyCard(
                  "Nhi khoa", Icons.child_care, const Color(0xFF4CAF50)),
              _buildSpecialtyCard(
                  "Sản khoa", Icons.pregnant_woman, const Color(0xFFE91E63)),
              _buildSpecialtyCard(
                  "Mắt", Icons.visibility, const Color(0xFF9C27B0)),
              _buildSpecialtyCard(
                  "Răng hàm mặt", Icons.face, const Color(0xFFFF9800)),
            ],
          ),
        ),
      ],
    );
  }

  Widget _buildSpecialtyCard(String title, IconData icon, Color color) {
    return Container(
      width: 100,
      margin: const EdgeInsets.only(right: 16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(15),
        boxShadow: [
          BoxShadow(
            color: Colors.grey.withOpacity(0.1),
            spreadRadius: 2,
            blurRadius: 5,
          ),
        ],
      ),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(icon, size: 30, color: color),
          const SizedBox(height: 8),
          Text(
            title,
            style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 12),
            textAlign: TextAlign.center,
          ),
        ],
      ),
    );
  }

  /// 8. Widget Mẹo sức khỏe
  Widget _buildHealthTipsSection() {
    return Column(
      children: [
        _buildSectionHeader(title: "Mẹo sức khỏe", onSeeMore: () {}),
        const SizedBox(height: 16),
        Padding(
          padding: const EdgeInsets.symmetric(horizontal: 20),
          child: Column(
            children: [
              _buildHealthTipCard(
                title: "Uống đủ nước mỗi ngày",
                description: "2-3 lít nước/ngày giúp cơ thể khỏe mạnh",
                icon: Icons.water_drop,
                color: const Color(0xFF03A9F4),
              ),
              const SizedBox(height: 12),
              _buildHealthTipCard(
                title: "Tập thể dục đều đặn",
                description: "30 phút/ngày giúp tăng cường sức khỏe",
                icon: Icons.fitness_center,
                color: const Color(0xFF4CAF50),
              ),
            ],
          ),
        ),
      ],
    );
  }

  Widget _buildHealthTipCard({
    required String title,
    required String description,
    required IconData icon,
    required Color color,
  }) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(15),
        boxShadow: [
          BoxShadow(
            color: Colors.grey.withOpacity(0.1),
            spreadRadius: 2,
            blurRadius: 5,
          ),
        ],
      ),
      child: Row(
        children: [
          Container(
            padding: const EdgeInsets.all(12),
            decoration: BoxDecoration(
              color: color.withOpacity(0.1),
              borderRadius: BorderRadius.circular(10),
            ),
            child: Icon(icon, color: color, size: 24),
          ),
          const SizedBox(width: 16),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  title,
                  style: const TextStyle(fontWeight: FontWeight.bold),
                ),
                const SizedBox(height: 4),
                Text(
                  description,
                  style: const TextStyle(color: Colors.grey, fontSize: 12),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  /// 9. Widget Đặt lịch khám
  Widget _buildBookingSection() {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 20),
      child: Container(
        padding: const EdgeInsets.all(20),
        decoration: BoxDecoration(
          gradient: const LinearGradient(
            colors: [Color(0xFF2D6A8E), Color(0xFF4A90C2)],
            begin: Alignment.topLeft,
            end: Alignment.bottomRight,
          ),
          borderRadius: BorderRadius.circular(20),
        ),
        child: Column(
          children: [
            const Icon(Icons.calendar_today, color: Colors.white, size: 40),
            const SizedBox(height: 12),
            const Text(
              "Đặt lịch khám nhanh",
              style: TextStyle(
                color: Colors.white,
                fontSize: 18,
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 8),
            const Text(
              "Đặt lịch khám với bác sĩ chuyên khoa",
              style: TextStyle(color: Colors.white70, fontSize: 14),
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 16),
            ElevatedButton(
              onPressed: () {
                // TODO: Implement booking action
              },
              style: ElevatedButton.styleFrom(
                backgroundColor: Colors.white,
                foregroundColor: const Color(0xFF2D6A8E),
                padding:
                    const EdgeInsets.symmetric(horizontal: 32, vertical: 12),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(25),
                ),
              ),
              child: const Text("Đặt lịch ngay"),
            ),
          ],
        ),
      ),
    );
  }

  /// 10. Widget Cấp cứu
  Widget _buildEmergencySection() {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 20),
      child: Container(
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(
          color: const Color(0xFFFFEBEE),
          borderRadius: BorderRadius.circular(15),
          border: Border.all(color: const Color(0xFFE57373)),
        ),
        child: Row(
          children: [
            Container(
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(
                color: const Color(0xFFF44336),
                borderRadius: BorderRadius.circular(10),
              ),
              child: const Icon(Icons.emergency, color: Colors.white, size: 24),
            ),
            const SizedBox(width: 16),
            const Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    "Cấp cứu 24/7",
                    style: TextStyle(
                      fontWeight: FontWeight.bold,
                      color: Color(0xFFF44336),
                    ),
                  ),
                  Text(
                    "Hotline: 115 - Luôn sẵn sàng hỗ trợ",
                    style: TextStyle(fontSize: 12, color: Colors.black87),
                  ),
                ],
              ),
            ),
            ElevatedButton(
              onPressed: () {
                // TODO: Implement call action (e.g., using url_launcher)
              },
              style: ElevatedButton.styleFrom(
                backgroundColor: const Color(0xFFF44336),
                foregroundColor: Colors.white,
                padding:
                    const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(20),
                ),
              ),
              child: const Text("Gọi ngay"),
            ),
          ],
        ),
      ),
    );
  }

  /// 11. Widget danh sách tin tức
  Widget _buildNewsList() {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 20),
      child: Column(
        children: [
          _buildNewsCard(),
          const SizedBox(height: 12),
          _buildNewsCard(),
        ],
      ),
    );
  }

  Widget _buildNewsCard() {
    return Container(
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(15),
        boxShadow: [
          BoxShadow(
            color: Colors.grey.withOpacity(0.1),
            spreadRadius: 2,
            blurRadius: 5,
          ),
        ],
      ),
      child: Row(
        children: [
          ClipRRect(
            borderRadius: BorderRadius.circular(10),
            child: Image.asset(
              "assets/banner1.png",
              width: 100,
              height: 80,
              fit: BoxFit.cover,
              errorBuilder: (context, error, stackTrace) {
                return Container(
                  width: 100,
                  height: 80,
                  color: Colors.grey.shade300,
                  child:
                      const Icon(Icons.image_not_supported, color: Colors.grey),
                );
              },
            ),
          ),
          const SizedBox(width: 12),
          const Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  "Tiêu đề tin tức sức khỏe ở đây",
                  style: TextStyle(fontWeight: FontWeight.bold),
                  maxLines: 2,
                  overflow: TextOverflow.ellipsis,
                ),
                SizedBox(height: 8),
                Text(
                  "1 giờ trước",
                  style: TextStyle(color: Colors.grey, fontSize: 12),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  /// 12. Widget Cơ sở y tế
  Widget _buildMedicalFacilities() {
    return Column(
      children: [
        _buildSectionHeader(title: "Cơ sở y tế", onSeeMore: () {}),
        const SizedBox(height: 16),
        SizedBox(
          height: 200,
          child: ListView(
            scrollDirection: Axis.horizontal,
            padding: const EdgeInsets.only(left: 20),
            children: [
              _buildFacilityCard(
                name: "Bệnh viện Chợ Rẫy",
                address: "201B Nguyễn Chí Thanh, Q.5",
                rating: 4.5,
                distance: "2.5 km",
              ),
              _buildFacilityCard(
                name: "Bệnh viện Bình Dân",
                address: "371 Điện Biên Phủ, Q.3",
                rating: 4.3,
                distance: "3.2 km",
              ),
              _buildFacilityCard(
                name: "Bệnh viện 115",
                address: "527 Sư Vạn Hạnh, Q.10",
                rating: 4.7,
                distance: "1.8 km",
              ),
            ],
          ),
        ),
      ],
    );
  }

  Widget _buildFacilityCard({
    required String name,
    required String address,
    required double rating,
    required String distance,
  }) {
    return Container(
      width: 200,
      margin: const EdgeInsets.only(right: 16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(15),
        boxShadow: [
          BoxShadow(
            color: Colors.grey.withOpacity(0.1),
            spreadRadius: 2,
            blurRadius: 5,
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Container(
            height: 100,
            decoration: BoxDecoration(
              color: Colors.grey[300],
              borderRadius:
                  const BorderRadius.vertical(top: Radius.circular(15)),
            ),
            child: const Center(
              child: Icon(Icons.local_hospital, size: 40, color: Colors.grey),
            ),
          ),
          Padding(
            padding: const EdgeInsets.all(12),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  name,
                  style: const TextStyle(
                      fontWeight: FontWeight.bold, fontSize: 14),
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                ),
                const SizedBox(height: 4),
                Text(
                  address,
                  style: const TextStyle(color: Colors.grey, fontSize: 12),
                  maxLines: 2,
                  overflow: TextOverflow.ellipsis,
                ),
                const SizedBox(height: 8),
                Row(
                  children: [
                    const Icon(Icons.star, color: Colors.amber, size: 16),
                    const SizedBox(width: 4),
                    Text(
                      rating.toString(),
                      style: const TextStyle(
                          fontSize: 12, fontWeight: FontWeight.bold),
                    ),
                    const Spacer(),
                    Text(
                      distance,
                      style: const TextStyle(fontSize: 12, color: Colors.grey),
                    ),
                  ],
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  /// 13. Widget Gói khám sức khỏe
  Widget _buildHealthCheckPackages() {
    return Column(
      children: [
        _buildSectionHeader(title: "Gói khám sức khỏe", onSeeMore: () {}),
        const SizedBox(height: 16),
        Padding(
          padding: const EdgeInsets.symmetric(horizontal: 20),
          child: Column(
            children: [
              _buildPackageCard(
                title: "Gói khám cơ bản",
                price: "500.000 đ",
                services: ["Khám tổng quát", "Xét nghiệm máu", "Đo huyết áp"],
                color: const Color(0xFF4CAF50),
              ),
              const SizedBox(height: 12),
              _buildPackageCard(
                title: "Gói khám nâng cao",
                price: "1.500.000 đ",
                services: [
                  "Khám tổng quát",
                  "Xét nghiệm đầy đủ",
                  "Siêu âm",
                  "X-quang"
                ],
                color: const Color(0xFF2196F3),
              ),
            ],
          ),
        ),
      ],
    );
  }

  Widget _buildPackageCard({
    required String title,
    required String price,
    required List<String> services,
    required Color color,
  }) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(15),
        boxShadow: [
          BoxShadow(
            color: Colors.grey.withOpacity(0.1),
            spreadRadius: 2,
            blurRadius: 5,
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                title,
                style: TextStyle(
                  fontWeight: FontWeight.bold,
                  fontSize: 16,
                  color: color,
                ),
              ),
              Text(
                price,
                style: TextStyle(
                  fontWeight: FontWeight.bold,
                  fontSize: 16,
                  color: color,
                ),
              ),
            ],
          ),
          const SizedBox(height: 12),
          ...services.map((service) => Padding(
                padding: const EdgeInsets.only(bottom: 4),
                child: Row(
                  children: [
                    Icon(Icons.check_circle, size: 16, color: color),
                    const SizedBox(width: 8),
                    Text(
                      service,
                      style: const TextStyle(fontSize: 14),
                    ),
                  ],
                ),
              )),
          const SizedBox(height: 12),
          SizedBox(
            width: double.infinity,
            child: ElevatedButton(
              onPressed: () {},
              style: ElevatedButton.styleFrom(
                backgroundColor: color,
                foregroundColor: Colors.white,
                padding: const EdgeInsets.symmetric(vertical: 12),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(10),
                ),
              ),
              child: const Text("Đặt gói khám"),
            ),
          ),
        ],
      ),
    );
  }

  /// 14. Widget Tư vấn chuyên gia
  Widget _buildExpertConsultation() {
    return Column(
      children: [
        _buildSectionHeader(title: "Tư vấn chuyên gia", onSeeMore: () {}),
        const SizedBox(height: 16),
        Padding(
          padding: const EdgeInsets.symmetric(horizontal: 20),
          child: Container(
            padding: const EdgeInsets.all(20),
            decoration: BoxDecoration(
              gradient: const LinearGradient(
                colors: [Color(0xFF6A4C93), Color(0xFF9C6ADE)],
                begin: Alignment.topLeft,
                end: Alignment.bottomRight,
              ),
              borderRadius: BorderRadius.circular(20),
            ),
            child: Column(
              children: [
                Row(
                  children: [
                    Container(
                      padding: const EdgeInsets.all(12),
                      decoration: BoxDecoration(
                        color: Colors.white.withOpacity(0.2),
                        borderRadius: BorderRadius.circular(15),
                      ),
                      child: const Icon(
                        Icons.video_call,
                        color: Colors.white,
                        size: 30,
                      ),
                    ),
                    const SizedBox(width: 16),
                    const Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            "Tư vấn trực tuyến",
                            style: TextStyle(
                              color: Colors.white,
                              fontSize: 18,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                          SizedBox(height: 4),
                          Text(
                            "Gặp gỡ bác sĩ qua video call",
                            style: TextStyle(
                              color: Colors.white70,
                              fontSize: 14,
                            ),
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 16),
                Row(
                  children: [
                    Expanded(
                      child: ElevatedButton(
                        onPressed: () {},
                        style: ElevatedButton.styleFrom(
                          backgroundColor: Colors.white,
                          foregroundColor: const Color(0xFF6A4C93),
                          padding: const EdgeInsets.symmetric(vertical: 12),
                          shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(25),
                          ),
                        ),
                        child: const Text("Tư vấn ngay"),
                      ),
                    ),
                    const SizedBox(width: 12),
                    Container(
                      padding: const EdgeInsets.all(12),
                      decoration: BoxDecoration(
                        color: Colors.white.withOpacity(0.2),
                        borderRadius: BorderRadius.circular(25),
                      ),
                      child: const Text(
                        "Từ 100.000đ",
                        style: TextStyle(
                          color: Colors.white,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
        ),
      ],
    );
  }

  /// 15. Widget Nhà thuốc
  Widget _buildPharmacySection() {
    return Column(
      children: [
        _buildSectionHeader(title: "Nhà thuốc", onSeeMore: () {}),
        const SizedBox(height: 16),
        SizedBox(
          height: 140,
          child: ListView(
            scrollDirection: Axis.horizontal,
            padding: const EdgeInsets.only(left: 20),
            children: [
              _buildPharmacyCard(
                name: "Thuốc cảm cúm",
                price: "25.000đ",
                originalPrice: "30.000đ",
                discount: "17%",
              ),
              _buildPharmacyCard(
                name: "Vitamin C",
                price: "45.000đ",
                originalPrice: "50.000đ",
                discount: "10%",
              ),
              _buildPharmacyCard(
                name: "Thuốc đau đầu",
                price: "35.000đ",
                originalPrice: "40.000đ",
                discount: "12%",
              ),
            ],
          ),
        ),
      ],
    );
  }

  Widget _buildPharmacyCard({
    required String name,
    required String price,
    required String originalPrice,
    required String discount,
  }) {
    return Container(
      width: 120,
      margin: const EdgeInsets.only(right: 16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(15),
        boxShadow: [
          BoxShadow(
            color: Colors.grey.withOpacity(0.1),
            spreadRadius: 2,
            blurRadius: 5,
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Stack(
            children: [
              Container(
                height: 60,
                decoration: BoxDecoration(
                  color: Colors.grey[200],
                  borderRadius:
                      const BorderRadius.vertical(top: Radius.circular(15)),
                ),
                child: const Center(
                  child: Icon(Icons.medication, size: 30, color: Colors.grey),
                ),
              ),
              Positioned(
                top: 4,
                right: 4,
                child: Container(
                  padding:
                      const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
                  decoration: BoxDecoration(
                    color: Colors.red,
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: Text(
                    "-$discount",
                    style: const TextStyle(
                      color: Colors.white,
                      fontSize: 10,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ),
              ),
            ],
          ),
          Padding(
            padding: const EdgeInsets.all(8),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  name,
                  style: const TextStyle(
                    fontWeight: FontWeight.bold,
                    fontSize: 12,
                  ),
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                ),
                const SizedBox(height: 4),
                Text(
                  price,
                  style: const TextStyle(
                    color: Colors.red,
                    fontWeight: FontWeight.bold,
                    fontSize: 14,
                  ),
                ),
                Text(
                  originalPrice,
                  style: const TextStyle(
                    color: Colors.grey,
                    fontSize: 10,
                    decoration: TextDecoration.lineThrough,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  /// 16. Widget Thống kê sức khỏe
  Widget _buildHealthStats() {
    return Column(
      children: [
        _buildSectionHeader(title: "Thống kê sức khỏe", onSeeMore: () {}),
        const SizedBox(height: 16),
        Padding(
          padding: const EdgeInsets.symmetric(horizontal: 20),
          child: Container(
            padding: const EdgeInsets.all(20),
            decoration: BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.circular(15),
              boxShadow: [
                BoxShadow(
                  color: Colors.grey.withOpacity(0.1),
                  spreadRadius: 2,
                  blurRadius: 5,
                ),
              ],
            ),
            child: Column(
              children: [
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceAround,
                  children: [
                    _buildStatItem(
                      icon: Icons.favorite,
                      value: "72",
                      unit: "BPM",
                      label: "Nhịp tim",
                      color: Colors.red,
                    ),
                    _buildStatItem(
                      icon: Icons.monitor_weight,
                      value: "65",
                      unit: "kg",
                      label: "Cân nặng",
                      color: Colors.blue,
                    ),
                    _buildStatItem(
                      icon: Icons.height,
                      value: "170",
                      unit: "cm",
                      label: "Chiều cao",
                      color: Colors.green,
                    ),
                  ],
                ),
                const SizedBox(height: 16),
                ElevatedButton(
                  onPressed: () {},
                  style: ElevatedButton.styleFrom(
                    backgroundColor: const Color(0xFF2D6A8E),
                    foregroundColor: Colors.white,
                    minimumSize: const Size(double.infinity, 45),
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(12),
                    ),
                  ),
                  child: const Text("Cập nhật chỉ số"),
                ),
              ],
            ),
          ),
        ),
      ],
    );
  }

  Widget _buildStatItem({
    required IconData icon,
    required String value,
    required String unit,
    required String label,
    required Color color,
  }) {
    return Column(
      children: [
        Container(
          padding: const EdgeInsets.all(12),
          decoration: BoxDecoration(
            color: color.withOpacity(0.1),
            borderRadius: BorderRadius.circular(12),
          ),
          child: Icon(icon, color: color, size: 24),
        ),
        const SizedBox(height: 8),
        RichText(
          text: TextSpan(
            style: DefaultTextStyle.of(context).style,
            children: [
              TextSpan(
                text: value,
                style: const TextStyle(
                  color: Colors.black,
                  fontSize: 16,
                  fontWeight: FontWeight.bold,
                ),
              ),
              TextSpan(
                text: " $unit",
                style: const TextStyle(
                  color: Colors.grey,
                  fontSize: 12,
                ),
              ),
            ],
          ),
        ),
        const SizedBox(height: 4),
        Text(
          label,
          style: const TextStyle(
            color: Colors.grey,
            fontSize: 12,
          ),
        ),
      ],
    );
  }

  /// 17. Widget Thông tin liên hệ (Footer)
  Widget _buildFooterInfo() {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 20),
      child: Container(
        padding: const EdgeInsets.all(20),
        decoration: BoxDecoration(
          color: const Color(0xFF2D6A8E),
          borderRadius: BorderRadius.circular(15),
        ),
        child: Column(
          children: [
            const Text(
              "Liên hệ với chúng tôi",
              style: TextStyle(
                color: Colors.white,
                fontSize: 18,
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 16),
            _buildContactItem(
              icon: Icons.phone,
              title: "Hotline",
              content: "1900 1234",
            ),
            const SizedBox(height: 12),
            _buildContactItem(
              icon: Icons.email,
              title: "Email",
              content: "support@medapp.vn",
            ),
            const SizedBox(height: 12),
            _buildContactItem(
              icon: Icons.location_on,
              title: "Địa chỉ",
              content: "123 Nguyễn Văn Linh, Q.7, TP.HCM",
            ),
            const SizedBox(height: 16),
            Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                _buildSocialButton(Icons.facebook),
                const SizedBox(width: 12),
                _buildSocialButton(Icons.alternate_email),
                const SizedBox(width: 12),
                _buildSocialButton(Icons.language),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildContactItem({
    required IconData icon,
    required String title,
    required String content,
  }) {
    return Row(
      children: [
        Icon(icon, color: Colors.white70, size: 20),
        const SizedBox(width: 12),
        Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              title,
              style: const TextStyle(
                color: Colors.white70,
                fontSize: 12,
              ),
            ),
            Text(
              content,
              style: const TextStyle(
                color: Colors.white,
                fontSize: 14,
                fontWeight: FontWeight.bold,
              ),
            ),
          ],
        ),
      ],
    );
  }

  Widget _buildSocialButton(IconData icon) {
    return InkWell(
      onTap: () {},
      child: Container(
        padding: const EdgeInsets.all(8),
        decoration: BoxDecoration(
          color: Colors.white.withOpacity(0.2),
          borderRadius: BorderRadius.circular(8),
        ),
        child: Icon(icon, color: Colors.white, size: 20),
      ),
    );
  }
}
