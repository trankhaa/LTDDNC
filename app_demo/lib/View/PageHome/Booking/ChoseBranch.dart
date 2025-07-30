// lib/PageHome/Booking/ChoseBranchScreen.dart
import 'package:flutter/material.dart';
import 'package:app_demo/Model/branch_model.dart';
import 'package:app_demo/services/branch_api_service.dart';
import './ChoseDepartmentScreen.dart'; // Màn hình chọn Khoa

class ChoseBranchScreen extends StatefulWidget {
  const ChoseBranchScreen({super.key});

  @override
  State<ChoseBranchScreen> createState() => _ChoseBranchScreenState();
}

class _ChoseBranchScreenState extends State<ChoseBranchScreen> {
  late Future<List<Branch>> _branchesFuture;
  final BranchApiService _apiService = BranchApiService();
  final Color _primaryColor = const Color(0xFF1976D2);

  @override
  void initState() {
    super.initState();
    _branchesFuture = _apiService.fetchAllBranches();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.grey[100],
      appBar: AppBar(
        title: const Text('Chọn Chi Nhánh', style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
        backgroundColor: _primaryColor,
        elevation: 1,
        leading: IconButton( // Nút back nếu màn hình này không phải là màn hình đầu tiên của luồng
          icon: const Icon(Icons.arrow_back_ios_new, color: Colors.white, size: 20),
          onPressed: () => Navigator.of(context).pop(),
        ),
      ),
      body: FutureBuilder<List<Branch>>(
        future: _branchesFuture,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return Center(child: CircularProgressIndicator(valueColor: AlwaysStoppedAnimation<Color>(_primaryColor)));
          }
          if (snapshot.hasError) {
            return _buildErrorWidget("chi nhánh", snapshot.error.toString());
          }
          if (!snapshot.hasData || snapshot.data!.isEmpty) {
            return _buildEmptyWidget("chi nhánh");
          }
          final branches = snapshot.data!;
          return ListView.builder(
            padding: const EdgeInsets.all(12),
            itemCount: branches.length,
            itemBuilder: (context, index) {
              return _buildBranchCard(branches[index]);
            },
          );
        },
      ),
    );
  }

  Widget _buildBranchCard(Branch branch) {
    return Card(
      margin: const EdgeInsets.symmetric(vertical: 8.0),
      elevation: 2,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      child: InkWell(
        borderRadius: BorderRadius.circular(12),
        onTap: () {
          Navigator.push(
            context,
            MaterialPageRoute(
              builder: (context) => ChoseDepartmentScreen(selectedBranch: branch),
            ),
          );
        },
        child: Padding(
          padding: const EdgeInsets.all(16.0),
          child: Row(
            children: [
              ClipRRect(
                borderRadius: BorderRadius.circular(8),
                child: SizedBox(
                  width: 60,
                  height: 60,
                  child: (branch.imageUrl != null && branch.imageUrl!.isNotEmpty)
                      ? Image.network(branch.imageUrl!, fit: BoxFit.cover,
                          errorBuilder: (c,e,s) => Icon(Icons.business, size: 30, color: Colors.grey[400]))
                      : Icon(Icons.business, size: 30, color: Colors.grey[400]),
                ),
              ),
              const SizedBox(width: 16),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(branch.branchName, style: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
                    if (branch.branchAddress != null && branch.branchAddress!.isNotEmpty) ...[
                      const SizedBox(height: 4),
                      Text(branch.branchAddress!, style: TextStyle(fontSize: 13, color: Colors.grey[600]), maxLines: 2, overflow: TextOverflow.ellipsis),
                    ]
                  ],
                ),
              ),
              Icon(Icons.arrow_forward_ios_rounded, color: Colors.grey[400], size: 18),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildErrorWidget(String itemType, String error) {
     String displayError = error.replaceFirst("Exception: ", "");
     if (displayError.toLowerCase().contains("socketexception") ||
        displayError.toLowerCase().contains("connection refused") ||
        displayError.toLowerCase().contains("timeout")) {
      displayError = "Không thể kết nối đến máy chủ. Vui lòng kiểm tra lại đường truyền mạng.";
    }
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(20.0),
        child: Column(mainAxisAlignment: MainAxisAlignment.center, children: [
          Icon(Icons.error_outline_rounded, color: Colors.red[600], size: 50),
          const SizedBox(height: 16),
          Text("Lỗi tải $itemType", style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Colors.red[700])),
          const SizedBox(height: 8),
          Text(displayError, textAlign: TextAlign.center, style: TextStyle(color: Colors.grey[700])),
           const SizedBox(height: 20),
            ElevatedButton.icon(
              icon: const Icon(Icons.refresh, size: 18),
              label: Text("Thử lại"),
              onPressed: () => setState(() { _branchesFuture = _apiService.fetchAllBranches(); }),
              style: ElevatedButton.styleFrom(backgroundColor: _primaryColor, foregroundColor: Colors.white),
            ),
        ]),
      ),
    );
  }

  Widget _buildEmptyWidget(String itemType) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(20.0),
        child: Column(mainAxisAlignment: MainAxisAlignment.center, children: [
          Icon(Icons.search_off_rounded, color: Colors.grey[400], size: 50),
          const SizedBox(height: 16),
          Text("Không có $itemType", style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: Colors.grey)),
          const SizedBox(height: 8),
          Text("Hiện tại chưa có $itemType nào được tìm thấy.", textAlign: TextAlign.center, style: TextStyle(color: Colors.grey[600])),
           const SizedBox(height: 20),
            ElevatedButton.icon(
              icon: const Icon(Icons.refresh, size: 18),
              label: Text("Tải lại"),
              onPressed: () => setState(() { _branchesFuture = _apiService.fetchAllBranches(); }),
              style: ElevatedButton.styleFrom(backgroundColor: _primaryColor, foregroundColor: Colors.white),
            ),
        ]),
      ),
    );
  }
}