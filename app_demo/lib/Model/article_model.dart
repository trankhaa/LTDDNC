// lib/models/article_model.dart

class Article {
  final String title;
  final String? description;
  final String url;
  final String? imageUrl;
  final DateTime publishedAt;
  final String sourceName;

  Article({
    required this.title,
    this.description,
    required this.url,
    this.imageUrl,
    required this.publishedAt,
    required this.sourceName,
  });

  // Factory constructor để tạo một Article từ JSON
  factory Article.fromJson(Map<String, dynamic> json) {
    return Article(
      title: json['title'] ?? 'No Title',
      description: json['description'],
      url: json['url'],
      imageUrl: json['image'],
      publishedAt: DateTime.parse(json['publishedAt']),
      sourceName: (json['source'] as Map<String, dynamic>)['name'] ?? 'Unknown',
    );
  }
}
