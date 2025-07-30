// lib/View/PageHome/Booking/confirm_booking_dialog_content.dart
// (Adjust path based on your project structure)

import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

class ConfirmBookingDialogContent extends StatefulWidget {
  final String patientName;
  final String patientEmail;
  final String doctorName;
  final DateTime selectedDate;
  final String selectedTimeSlotStartTime; // e.g., "09:00"
  final int examinationTimeMinutes;
  final double? consultationFee;
  final String symptoms;
  final Future<void> Function() onConfirmBooking;

  const ConfirmBookingDialogContent({
    Key? key,
    required this.patientName,
    required this.patientEmail,
    required this.doctorName,
    required this.selectedDate,
    required this.selectedTimeSlotStartTime,
    required this.examinationTimeMinutes,
    required this.consultationFee,
    required this.symptoms,
    required this.onConfirmBooking,
  }) : super(key: key);

  @override
  State<ConfirmBookingDialogContent> createState() =>
      _ConfirmBookingDialogContentState();
}

class _ConfirmBookingDialogContentState
    extends State<ConfirmBookingDialogContent> {
  bool _isProcessing = false;

  String get _formattedDate {
    // Format: Thứ Bảy, 28/06/2025
    return DateFormat('EEEE, dd/MM/yyyy', 'vi_VN').format(widget.selectedDate);
  }

  String get _formattedTimeRange {
    final timeFormat = DateFormat('HH:mm');
    try {
      DateTime parsedStartTime =
          timeFormat.parse(widget.selectedTimeSlotStartTime);
      DateTime endTime =
          parsedStartTime.add(Duration(minutes: widget.examinationTimeMinutes));
      return '${widget.selectedTimeSlotStartTime} - ${timeFormat.format(endTime)}';
    } catch (e) {
      print("[ConfirmBookingDialog] Error formatting time range: $e");
      return widget.selectedTimeSlotStartTime; // Fallback
    }
  }

  String get _formattedFee {
    if (widget.consultationFee == null) {
      return "Chưa có thông tin";
    }
    // Format: 150.000 VNĐ
    return NumberFormat.currency(locale: 'vi_VN', symbol: '')
            .format(widget.consultationFee)
            .replaceAll('.', ',') + // Ensure comma as thousands separator for display
        ' VNĐ';
  }

  Future<void> _handleConfirm() async {
    if (_isProcessing) return;
    setState(() {
      _isProcessing = true;
    });
    try {
      await widget.onConfirmBooking();
      // Dialog closing is handled by the caller of onConfirmBooking
    } catch (e) {
      // Error display is handled by onConfirmBooking (e.g., SnackBar)
      // Dialog closing is also handled by the caller in case of error
    } finally {
      // If the dialog is still mounted and an error occurred that didn't lead to popping,
      // reset the state. However, the current flow pops the dialog.
      if (mounted) {
        setState(() {
          _isProcessing = false;
        });
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16.0)),
      titlePadding: const EdgeInsets.fromLTRB(20, 24, 20, 0),
      contentPadding: const EdgeInsets.fromLTRB(20, 16, 20, 16),
      actionsPadding: const EdgeInsets.fromLTRB(20, 8, 20, 16), // Adjusted top padding for actions
      title: const Text(
        'Xác nhận thông tin đặt lịch',
        textAlign: TextAlign.center,
        style: TextStyle(fontWeight: FontWeight.bold, fontSize: 18),
      ),
      content: SingleChildScrollView(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: <Widget>[
            const Text(
              'Vui lòng kiểm tra kỹ thông tin trước khi xác nhận:',
              style: TextStyle(fontSize: 14, color: Colors.black54),
            ),
            const SizedBox(height: 20),
            _buildInfoRow(
                Icons.person_outline, 'Bệnh nhân:', widget.patientName),
            _buildInfoRow(Icons.email_outlined, 'Email:', widget.patientEmail),
            const Divider(height: 24, thickness: 0.5),
            _buildInfoRow(
                const IconData(0xe3f9, fontFamily: 'MaterialIcons'), // Placeholder for first-aid/doctor bag icon
                'Bác sĩ:',
                widget.doctorName),
            _buildInfoRow(
                Icons.calendar_today_outlined, 'Ngày khám:', _formattedDate),
            _buildInfoRow(
                Icons.access_time_outlined, 'Thời gian:', _formattedTimeRange),
            _buildInfoRow(
                Icons.monetization_on_outlined, 'Phí khám (dự kiến):', _formattedFee,
                valueColor: const Color(0xFF388E3C)), // Green color like image
            _buildInfoRow(Icons.notes_outlined, 'Triệu chứng:',
                widget.symptoms.isNotEmpty ? widget.symptoms : "Không có"),
          ],
        ),
      ),
      actions: <Widget>[
        TextButton(
          child: Text('HỦY', style: TextStyle(color: Colors.grey.shade700, fontWeight: FontWeight.bold)),
          onPressed: _isProcessing ? null : () => Navigator.of(context).pop(),
        ),
        ElevatedButton(
          style: ElevatedButton.styleFrom(
            backgroundColor: Theme.of(context).primaryColor, // Use your app's primary color
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(20.0), // More rounded like image
            ),
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10)
          ),
          onPressed: _isProcessing ? null : _handleConfirm,
          child: _isProcessing
              ? Row(
                  mainAxisSize: MainAxisSize.min,
                  children: const [
                    SizedBox(
                      width: 18,
                      height: 18,
                      child: CircularProgressIndicator(
                        strokeWidth: 2.5,
                        valueColor: AlwaysStoppedAnimation<Color>(Colors.white),
                      ),
                    ),
                    SizedBox(width: 10),
                    Text('ĐANG XỬ LÝ...', style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
                  ],
                )
              : const Text('XÁC NHẬN', style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
        ),
      ],
    );
  }

  Widget _buildInfoRow(IconData icon, String label, String value, {Color? valueColor}) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 7.0), // Adjusted vertical padding
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.center, // Align items center for better look with icons
        children: [
          Icon(icon, size: 20, color: Theme.of(context).primaryColorDark), // Slightly darker icon color
          const SizedBox(width: 12),
          Text(
            label,
            style: const TextStyle(fontSize: 14.5, fontWeight: FontWeight.normal, color: Colors.black87),
          ),
          const SizedBox(width: 8),
          Expanded(
            child: Text(
              value,
              textAlign: TextAlign.right,
              style: TextStyle(fontSize: 14.5, fontWeight: FontWeight.w500, color: valueColor ?? Colors.black),
            ),
          ),
        ],
      ),
    );
  }
}