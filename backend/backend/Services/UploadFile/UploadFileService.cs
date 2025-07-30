namespace backend.Services.UploadFile
{
    public class UploadFileService : IUploadFileService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private const string UploadsDirectory = "uploads";

        // Sử dụng dependency injection để tiêm IWebHostEnvironment
        // Nó giúp chúng ta lấy được đường dẫn vật lý đến thư mục wwwroot
        public UploadFileService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string subfolder)
        {
            if (file == null || file.Length == 0)
            {
                return string.Empty; // Trả về chuỗi rỗng nếu không có file
            }

            // Lấy đường dẫn đến thư mục gốc của web (wwwroot)
            var webRootPath = _webHostEnvironment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRootPath))
            {
                // Fallback nếu wwwroot không được cấu hình đúng
                webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            // Tạo đường dẫn đầy đủ tới thư mục lưu trữ: wwwroot/uploads/{tên thư mục con}
            var targetFolderPath = Path.Combine(webRootPath, UploadsDirectory, subfolder);

            // Kiểm tra và tạo thư mục nếu chưa tồn tại
            if (!Directory.Exists(targetFolderPath))
            {
                Directory.CreateDirectory(targetFolderPath);
            }

            // Tạo một tên file độc nhất để tránh bị ghi đè file trùng tên
            var fileExtension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var fullFilePath = Path.Combine(targetFolderPath, uniqueFileName);

            // Lưu file vào đường dẫn đã tạo
            using (var fileStream = new FileStream(fullFilePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Trả về đường dẫn tương đối, có thể dùng trực tiếp trong thẻ <img> của HTML
            return $"/{UploadsDirectory}/{subfolder}/{uniqueFileName}";
        }

        public void DeleteFile(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                return;
            }

            var webRootPath = _webHostEnvironment.WebRootPath;

            // Chuyển đổi đường dẫn tương đối (ví dụ: /uploads/avatars/...) thành đường dẫn vật lý
            var fullPath = Path.Combine(webRootPath, relativePath.TrimStart('/'));

            // Kiểm tra xem file có tồn tại không trước khi xóa
            if (File.Exists(fullPath))
            {
                try
                {
                    File.Delete(fullPath);
                }
                catch (IOException ex)
                {
                    // Có thể ghi log lỗi ở đây để theo dõi nếu cần
                    Console.WriteLine($"Error deleting file {fullPath}: {ex.Message}");
                }
            }
        }

        public Task DeleteFileAsync(string avatar)
        {
            throw new NotImplementedException();
        }
    }
}
