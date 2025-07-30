// lib/View/PageHome/Booking/doctor_booking_screen.dart

import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:url_launcher/url_launcher.dart';

// Import các model và service cần thiết
import '../../../Model/user_model.dart';
import '../../../services/auth_service.dart';
import '../../../Model/doctor_full_info_model.dart';
import '../../../Model/booking_confirmation_model.dart';
import '../../../Model/appointment_model.dart';
import '../../../Services/doctor_api_service.dart';
import '../../../Services/booking_api_service.dart';
import './confirm_booking_dialog_content.dart';

// ✅ BƯỚC 1: IMPORT NOTIFICATION SERVICE (Bạn đã làm đúng)
import '../../../services/notification_service.dart'; 

const String _defaultDoctorAvatarAssetScreen = "assets/images/default_avatar.png";

class DoctorBookingScreen extends StatefulWidget {
  final String doctorId;
  const DoctorBookingScreen({Key? key, required this.doctorId}) : super(key: key);

  @override
  State<DoctorBookingScreen> createState() => _DoctorBookingScreenState();
}

class _DoctorBookingScreenState extends State<DoctorBookingScreen> {
  // ... (Toàn bộ các biến state của bạn, giữ nguyên không đổi)
  final AuthService _authService = AuthService();
  final DoctorApiService _doctorApiService = DoctorApiService();
  final BookingApiService _bookingApiService = BookingApiService();

  User? _currentUser;
  bool _isLoadingUser = true;
  DoctorFullInfoModel? _doctorFullInfo;
  bool _isLoadingDoctorInfo = true;
  String? _errorLoadingDoctorInfo;
  DateTime? _selectedDate;
  List<String> _availableTimeSlots = [];
  bool _isLoadingSlots = false;
  String? _selectedTimeSlot;
  final TextEditingController _symptomsController = TextEditingController();
  List<AppointmentModel> _allBookedAppointmentsForDoctor = [];
  bool _hasLoadedAllAppointments = false;


  @override
  void initState() {
    super.initState();
    _fetchInitialData();
  }

  @override
  void dispose() {
    _symptomsController.dispose();
    super.dispose();
  }

  // ... (Tất cả các hàm _fetch..., _load..., _update..., _generate... giữ nguyên không đổi)
  Future<void> _fetchInitialData() async {
    await _loadCurrentUser();
    if (!mounted || _currentUser == null) return;
    await _fetchDoctorDetails();
    if (_doctorFullInfo != null && !_hasLoadedAllAppointments && mounted) {
      await _fetchAllBookedAppointmentsForDoctor();
    }
  }

  Future<void> _loadCurrentUser() async {
    if (!mounted) return;
    setState(() { _isLoadingUser = true; });
    _currentUser = await _authService.getSavedUser();
    if (mounted) setState(() { _isLoadingUser = false; });
  }

  Future<void> _fetchDoctorDetails() async {
    if (!mounted) return;
    setState(() { _isLoadingDoctorInfo = true; _errorLoadingDoctorInfo = null; });
    try {
      final data = await _doctorApiService.getDoctorFullInfoById(widget.doctorId);
      if (mounted) setState(() { _doctorFullInfo = data; _isLoadingDoctorInfo = false; });
    } catch (e) {
      if (mounted) setState(() { _errorLoadingDoctorInfo = "Lỗi tải thông tin bác sĩ."; _isLoadingDoctorInfo = false; });
    }
  }

  Future<void> _fetchAllBookedAppointmentsForDoctor() async {
    if (!mounted || _doctorFullInfo == null) return;
    setState(() { _isLoadingSlots = true; });
    try {
      final appointments = await _bookingApiService.getAppointmentsByDoctorId(widget.doctorId);
      if (mounted) {
        setState(() {
          _allBookedAppointmentsForDoctor = appointments;
          _hasLoadedAllAppointments = true;
          _updateAvailableSlotsForSelectedDate();
        });
      }
    } catch (e) {
      if (mounted) setState(() { _isLoadingSlots = false; });
    }
  }

  void _updateAvailableSlotsForSelectedDate() {
    if (!mounted || _selectedDate == null || !_hasLoadedAllAppointments || _doctorFullInfo == null) {
      if (mounted) setState(() { _availableTimeSlots = []; _isLoadingSlots = false; });
      return;
    }
    final allPossible = _generatePossibleStartTimesForDay(_selectedDate!);
    final selectedDateUtc = DateTime.utc(_selectedDate!.year, _selectedDate!.month, _selectedDate!.day);
    final bookedSlots = _allBookedAppointmentsForDoctor
        .where((app) => DateTime.utc(app.date.year, app.date.month, app.date.day).isAtSameMomentAs(selectedDateUtc))
        .map((app) => app.startTimeOfSlot)
        .toSet();
    if (mounted) {
      setState(() {
        _availableTimeSlots = allPossible.where((slot) => !bookedSlots.contains(slot)).toList();
        _isLoadingSlots = false;
      });
    }
  }

  List<String> _generatePossibleStartTimesForDay(DateTime forDate) {
    if (_doctorFullInfo?.doctorSchedules == null || _doctorFullInfo!.doctorSchedules.isEmpty) return [];
    final schedule = _doctorFullInfo!.doctorSchedules[0];
    final examinationTime = schedule.examinationTime ?? 30;
    final startTimeString = schedule.startTime;
    final endTimeString = schedule.endTime;
    if (startTimeString == null || endTimeString == null) return [];
    final List<String> slots = [];
    try {
      DateTime start = DateFormat("HH:mm").parse(startTimeString);
      DateTime end = DateFormat("HH:mm").parse(endTimeString);
      while (start.isBefore(end)) {
        slots.add(DateFormat("HH:mm").format(start));
        start = start.add(Duration(minutes: examinationTime));
      }
    } catch (e) { print("Lỗi tạo slot: $e"); }
    return slots;
  }

  Future<void> _onDateSelectedFromPicker(DateTime date) async {
    if (!mounted) return;
    setState(() { _selectedDate = date; _selectedTimeSlot = null; _isLoadingSlots = true; });
    _updateAvailableSlotsForSelectedDate();
  }


  // ===================================================================================
  // ✅✅✅ HÀM NÀY ĐÃ ĐƯỢC CẬP NHẬT HOÀN CHỈNH ✅✅✅
  // ===================================================================================
  Future<void> _executeBookingAndPaymentFlow() async {
    if (_currentUser == null || _doctorFullInfo == null || _selectedDate == null || _selectedTimeSlot == null) return;
    
    // Chuẩn bị dữ liệu
    final int examinationTime = _doctorFullInfo!.examinationTimePerSlot ?? 30;
    final String startTime = _selectedTimeSlot!;
    final DateTime startDateTime = DateFormat('HH:mm').parse(startTime);
    final String slotForBooking = "$startTime-${DateFormat('HH:mm').format(startDateTime.add(Duration(minutes: examinationTime)))}";
    final DateTime dateUtc = DateTime.utc(_selectedDate!.year, _selectedDate!.month, _selectedDate!.day);
    
    final bookingData = BookingConfirmationModel(
      // ... (dữ liệu booking của bạn)
      nameDr: _doctorFullInfo!.doctor.name ?? 'Bác sĩ',
      doctorId: _doctorFullInfo!.doctor.id,
      slot: slotForBooking,
      patientId: _currentUser!.id,
      patientEmail: _currentUser!.email ?? 'no-email@example.com',
      patientName: _currentUser!.name ?? 'Bệnh nhân',
      date: dateUtc,
      symptoms: _symptomsController.text.trim(),
      consultationFee: _doctorFullInfo!.consultationFee ?? 0.0,
    );

    try {
      // Gọi API và chờ kết quả thành công
      final String paymentUrl = await _bookingApiService.createAppointmentAndGetPaymentLink(bookingData);
      if (!mounted) return;

      // ===========================================================================
      // ✅ BƯỚC 2: GỬI THÔNG BÁO SAU KHI ĐẶT LỊCH THÀNH CÔNG
      // ===========================================================================

      // ---- 2.1: THÔNG BÁO TỨC THÌ "ĐẶT LỊCH THÀNH CÔNG" ----
      final int immediateId = DateTime.now().millisecondsSinceEpoch.remainder(100000);
      await NotificationService.showImmediateNotification(
          id: immediateId,
          title: '✅ Đặt Lịch Thành Công!',
          body: 'Bạn đã đặt hẹn với ${_doctorFullInfo!.doctor.name} vào lúc $slotForBooking, ${DateFormat('dd/MM/yyyy').format(_selectedDate!)}.');
      
      // ---- 2.2: LÊN LỊCH NHẮC NHỞ TRONG TƯƠNG LAI (vẫn giữ nguyên) ----
      final int scheduledId = DateTime.now().millisecondsSinceEpoch.remainder(100000) + 1; // Đảm bảo ID khác nhau
      await NotificationService.scheduleAppointmentReminder(
        id: scheduledId,
        doctorName: _doctorFullInfo!.doctor.name ?? 'Không rõ tên',
        appointmentDate: _selectedDate!,
        appointmentSlot: slotForBooking,
      );
      
      // ===========================================================================

      // Mở link thanh toán
      await _launchUrl(paymentUrl);

      // Hiển thị thông báo trong ứng dụng cho người dùng biết
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(
        content: Text('Đặt lịch thành công! Một lời nhắc đã được tạo.'),
        backgroundColor: Colors.green,
      ));

      // Quay về màn hình chính
      Navigator.of(context).popUntil((route) => route.isFirst);

    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.toString().replaceFirst("Exception: ", "")), backgroundColor: Colors.red));
      }
      rethrow;
    }
  }
  // ===================================================================================

  Future<void> _launchUrl(String url) async {
    final uri = Uri.parse(url);
    if (!await launchUrl(uri, mode: LaunchMode.externalApplication)) {
      throw 'Không thể mở $url';
    }
  }

  Future<void> _showBookingConfirmationDialog() async {
    if (_currentUser == null || _doctorFullInfo == null || _selectedDate == null || _selectedTimeSlot == null) {
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Vui lòng chọn đủ thông tin')));
      return;
    }
    showDialog(
      context: context,
      builder: (ctx) => ConfirmBookingDialogContent(
        patientName: _currentUser!.name ?? 'Bệnh nhân',
        patientEmail: _currentUser!.email ?? 'Không có email',
        doctorName: _doctorFullInfo!.doctor.name ?? "Bác sĩ",
        selectedDate: _selectedDate!,
        selectedTimeSlotStartTime: _selectedTimeSlot!,
        examinationTimeMinutes: _doctorFullInfo!.examinationTimePerSlot ?? 30,
        consultationFee: _doctorFullInfo!.consultationFee,
        symptoms: _symptomsController.text.trim(),
        onConfirmBooking: () async {
          try {
            await _executeBookingAndPaymentFlow();
          } catch (e) {
            // Lỗi đã được hiển thị, chỉ cần đóng dialog
            if (mounted) Navigator.of(ctx).pop();
          }
        },
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    // ... (Hàm build và các widget con giữ nguyên không đổi)
    return Scaffold(
      appBar: AppBar(title: Text(_doctorFullInfo?.doctor.name ?? 'Đặt lịch khám')),
      body: _buildBody(),
    );
  }

  Widget _buildBody() {
    if (_isLoadingUser) return const Center(child: CircularProgressIndicator());
    if (_currentUser == null) {
      return Center(
        child: Column(mainAxisAlignment: MainAxisAlignment.center, children: [
          const Text('Vui lòng đăng nhập để đặt lịch.'),
          ElevatedButton(onPressed: () => Navigator.of(context).pushReplacementNamed('/login'), child: const Text('Đăng nhập'))
        ]),
      );
    }
    if (_isLoadingDoctorInfo) return const Center(child: CircularProgressIndicator());
    if (_errorLoadingDoctorInfo != null) return Center(child: Text(_errorLoadingDoctorInfo!));
    if (_doctorFullInfo == null) return const Center(child: Text("Không tìm thấy bác sĩ."));
    
    return SingleChildScrollView(
      padding: const EdgeInsets.all(16.0),
      child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
        _buildDoctorHeader(),
        const SizedBox(height: 24),
        _buildDatePickerSection(),
        const SizedBox(height: 16),
        _buildTimeSlotSection(),
        const SizedBox(height: 24),
        _buildSymptomsInput(),
        const SizedBox(height: 32),
        _buildBookingButton(),
      ]),
    );
  }

  Widget _buildDoctorHeader() {
    return Card(
      elevation: 2,
      child: Padding(padding: const EdgeInsets.all(12), child: Row(children: [
        CircleAvatar(
          radius: 40, 
          backgroundImage: (_doctorFullInfo?.doctorImageUrl != null && _doctorFullInfo!.doctorImageUrl!.isNotEmpty) 
              ? NetworkImage(_doctorFullInfo!.doctorImageUrl!) 
              : const AssetImage(_defaultDoctorAvatarAssetScreen) as ImageProvider,
          onBackgroundImageError: (exception, stackTrace) {
            print("Lỗi tải ảnh bác sĩ: $exception");
          },
        ),
        const SizedBox(width: 16),
        Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
          Text(_doctorFullInfo!.doctor.name ?? 'Bác sĩ không tên', style: Theme.of(context).textTheme.titleLarge),
          Text(_doctorFullInfo!.specialtyName ?? 'Chưa có chuyên khoa'),
        ])),
      ])),
    );
  }

  Widget _buildDatePickerSection() {
    return Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
      Text('Chọn ngày khám', style: Theme.of(context).textTheme.titleMedium),
      const SizedBox(height: 8),
      ElevatedButton.icon(
        icon: const Icon(Icons.calendar_today),
        label: Text(_selectedDate == null ? "Chọn ngày" : DateFormat('dd/MM/yyyy').format(_selectedDate!)),
        onPressed: () async {
          final picked = await showDatePicker(
            context: context,
            initialDate: DateTime.now().add(const Duration(days: 1)),
            firstDate: DateTime.now().add(const Duration(days: 1)),
            lastDate: DateTime.now().add(const Duration(days: 60)),
          );
          if (picked != null) _onDateSelectedFromPicker(picked);
        },
      ),
    ]);
  }

  Widget _buildTimeSlotSection() {
    if (_selectedDate == null) return const SizedBox.shrink();
    if (_isLoadingSlots) return const Center(child: CircularProgressIndicator());
    if (_availableTimeSlots.isEmpty && _hasLoadedAllAppointments) return const Center(child: Padding(padding: EdgeInsets.all(16), child: Text('Không có lịch trống cho ngày này.')));
    
    return Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
      Text('Chọn giờ khám', style: Theme.of(context).textTheme.titleMedium),
      const SizedBox(height: 8),
      Wrap(
        spacing: 8, runSpacing: 8,
        children: _availableTimeSlots.map((slot) {
          final isSelected = _selectedTimeSlot == slot;
          return ChoiceChip(
            label: Text(slot),
            selected: isSelected,
            onSelected: (selected) => setState(() => _selectedTimeSlot = selected ? slot : null),
            selectedColor: Theme.of(context).primaryColor,
            labelStyle: TextStyle(color: isSelected ? Colors.white : Colors.black),
          );
        }).toList(),
      ),
    ]);
  }

  Widget _buildSymptomsInput() {
    return TextField(
      controller: _symptomsController,
      decoration: const InputDecoration(labelText: 'Triệu chứng (không bắt buộc)', border: OutlineInputBorder()),
      maxLines: 3,
    );
  }

  Widget _buildBookingButton() {
    bool canBook = _selectedDate != null && _selectedTimeSlot != null;
    return SizedBox(
      width: double.infinity,
      child: ElevatedButton(
        onPressed: canBook ? _showBookingConfirmationDialog : null,
        child: const Text('Tiếp tục đến thanh toán', style: TextStyle(fontSize: 16)),
        style: ElevatedButton.styleFrom(padding: const EdgeInsets.symmetric(vertical: 16)),
      ),
    );
  }
}