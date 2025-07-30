// lib/screens/health_news_screen.dart

import 'package:easy_localization/easy_localization.dart';
import 'package:flutter/material.dart';
import '../../generated/locale_keys.g.dart';
import '../../Model/article_model.dart';
import '../../Services/news_service.dart';
import '../../widgets/article_card.dart';

class HealthNewsScreen extends StatefulWidget {
  const HealthNewsScreen({super.key});

  @override
  State<HealthNewsScreen> createState() => _HealthNewsScreenState();
}

class _HealthNewsScreenState extends State<HealthNewsScreen> {
  late Future<List<Article>> _articlesFuture;
  final NewsService _newsService = NewsService();

  @override
  void initState() {
    super.initState();
    _articlesFuture = _newsService.fetchHealthNews();
  }

  void _changeLanguage(BuildContext context, Locale locale) {
    context.setLocale(locale);
    // Không cần gọi lại API vì API luôn là tiếng Anh
    // Giao diện sẽ tự cập nhật nhờ easy_localization
  }

  @override
  Widget build(BuildContext context) {
    final currentLocale = context.locale;

    return Scaffold(
      body: Container(
        decoration: BoxDecoration(
          gradient: LinearGradient(
            colors: [Colors.blue.shade50, Colors.white, Colors.green.shade50],
            begin: Alignment.topLeft,
            end: Alignment.bottomRight,
          ),
        ),
        child: SafeArea(
          child: Column(
            children: [
              // Header và Language Switcher
              Padding(
                padding: const EdgeInsets.all(16.0),
                child: Column(
                  children: [
                    // Language switcher
                    Row(
                      mainAxisAlignment: MainAxisAlignment.end,
                      children: [
                        TextButton(
                          onPressed:
                              () => _changeLanguage(
                                context,
                                const Locale('vi', 'VN'),
                              ),
                          style: TextButton.styleFrom(
                            backgroundColor:
                                currentLocale.languageCode == 'vi'
                                    ? Colors.blue[500]
                                    : Colors.blue[100],
                            foregroundColor:
                                currentLocale.languageCode == 'vi'
                                    ? Colors.white
                                    : Colors.black,
                          ),
                          child: const Text("🇻🇳 Tiếng Việt"),
                        ),
                        const SizedBox(width: 8),
                        TextButton(
                          onPressed:
                              () => _changeLanguage(
                                context,
                                const Locale('en', 'US'),
                              ),
                          style: TextButton.styleFrom(
                            backgroundColor:
                                currentLocale.languageCode == 'en'
                                    ? Colors.green[500]
                                    : Colors.green[100],
                            foregroundColor:
                                currentLocale.languageCode == 'en'
                                    ? Colors.white
                                    : Colors.black,
                          ),
                          child: const Text("🇺🇸 English"),
                        ),
                      ],
                    ),
                    const SizedBox(height: 16),
                    // Header
                    const Icon(
                      Icons.monitor_heart_outlined,
                      size: 48,
                      color: Colors.blue,
                    ),
                    const SizedBox(height: 8),
                    Text(
                      LocaleKeys.health_news_title.tr(),
                      style: const TextStyle(
                        fontSize: 28,
                        fontWeight: FontWeight.bold,
                        color: Colors.black87,
                      ),
                      textAlign: TextAlign.center,
                    ),
                    const SizedBox(height: 4),
                    Text(
                      LocaleKeys.health_news_description.tr(),
                      style: TextStyle(fontSize: 16, color: Colors.grey[600]),
                      textAlign: TextAlign.center,
                    ),
                  ],
                ),
              ),
              // Vùng nội dung
              Expanded(
                child: FutureBuilder<List<Article>>(
                  future: _articlesFuture,
                  builder: (context, snapshot) {
                    // Đang tải
                    if (snapshot.connectionState == ConnectionState.waiting) {
                      return Center(
                        child: Column(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            const CircularProgressIndicator(),
                            const SizedBox(height: 16),
                            Text(LocaleKeys.loading.tr()),
                          ],
                        ),
                      );
                    }
                    // Có lỗi
                    if (snapshot.hasError) {
                      return Center(
                        child: Padding(
                          padding: const EdgeInsets.all(16.0),
                          child: Text(
                            '${LocaleKeys.api_error.tr()}: ${snapshot.error}',
                            textAlign: TextAlign.center,
                            style: const TextStyle(color: Colors.red),
                          ),
                        ),
                      );
                    }
                    // Thành công nhưng không có dữ liệu
                    final articles = snapshot.data;
                    if (articles == null || articles.isEmpty) {
                      return Center(child: Text(LocaleKeys.no_articles.tr()));
                    }
                    // Thành công và có dữ liệu
                    return GridView.builder(
                      padding: const EdgeInsets.all(8),
                      gridDelegate:
                          const SliverGridDelegateWithMaxCrossAxisExtent(
                            maxCrossAxisExtent:
                                450, // Chiều rộng tối đa của mỗi item
                            childAspectRatio: 0.8, // Tỷ lệ chiều rộng/cao
                            crossAxisSpacing: 8,
                            mainAxisSpacing: 8,
                          ),
                      itemCount: articles.length,
                      itemBuilder: (context, index) {
                        return ArticleCard(article: articles[index]);
                      },
                    );
                  },
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
