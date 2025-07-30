namespace backend.Services.UploadFile
{
    // Interface được khai báo bằng từ khóa "interface", không phải "class"
    public interface IUploadFileService
    {
        /// <summary>
        /// Lưu một file vào thư mục con trong wwwroot/uploads và trả về đường dẫn.
        /// </summary>
        /// <param name="file">Đối tượng IFormFile từ request.</param>
        /// <param name="subfolder">Thư mục con để lưu file (ví dụ: "avatars").</param>
        /// <returns>Đường dẫn tương đối của file đã lưu (ví dụ: /uploads/avatars/ten-file.jpg).</returns>
        Task<string> UploadFileAsync(IFormFile file, string subfolder);

        /// <summary>
        /// Xóa một file dựa trên đường dẫn tương đối của nó.
        /// </summary>
        /// <param name="relativePath">Đường dẫn tương đối của file trong wwwroot.</param>
        void DeleteFile(string? relativePath);
        Task DeleteFileAsync(string avatar);
    }
}
