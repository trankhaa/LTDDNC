// lib/utils/url_helper.dart

import '../Services/branch_api_service.dart';

String resolveFullImageUrl(String? imagePath) {
  if (imagePath == null || imagePath.isEmpty) return '';
  if (imagePath.startsWith('http')) return imagePath;
  return BranchApiService.host + imagePath;
}
