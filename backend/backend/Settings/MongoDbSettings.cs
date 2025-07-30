namespace backend.Settings
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;

        // *******************************************************************
        // >>> THÊM DÒNG NÀY ĐỂ KHAI BÁO THUỘC TÍNH 'Collections' <<<
        // *******************************************************************
        public Collections Collections { get; set; } = new Collections(); // Khởi tạo để tránh null

        // Các thuộc tính Collection names riêng lẻ này vẫn được giữ lại
        // vì các service khác của bạn (ví dụ DoctorDetailService, DoctorService)
        // đang sử dụng chúng khi inject IOptions<MongoDbSettings>.
        public string ProductCollectionName { get; set; } = string.Empty;
        public string UserCollectionName { get; set; } = "User"; // Có thể đổi tên thành UsersCollectionName cho nhất quán
        public string BranchCollectionName { get; set; } = string.Empty;
        public string DoctorCollectionName { get; set; } = string.Empty;
        public string TeacherCollectionName { get; set; } = string.Empty;
        public string SpecialtyCollectionName { get; set; } = string.Empty;
        public string HealthTipCollectionName { get; set; } = string.Empty;
        public string DepartmentCollectionName { get; set; } = string.Empty;
        public string DoctorDetailCollectionName { get; set; } = string.Empty;
        public string DoctorScheduleCollectionName { get; set; } = string.Empty;
        public string ConfirmAppointmentCollectionName { get; set; } = string.Empty;
    }

    // Class Collections này định nghĩa cấu trúc cho thuộc tính 'Collections' ở trên
    // Các giá trị mặc định ở đây sẽ được ghi đè bởi appsettings.json nếu có
    public class Collections
    {
        public string Users { get; set; } = "Users";
        public string Doctors { get; set; } = "Doctors";
        public string DoctorDetails { get; set; } = "DoctorDetails";
        // public string DoctorReviews { get; set; } = "DoctorReviews";
        public string DoctorSchedules { get; set; } = "DoctorSchedules";
        public string AdminAnswers { get; set; } = "AdminAnswers";
        public string ALChatLogs { get; set; } = "ALChatLogs"; // Cân nhắc đổi tên thành AiChatLogs nếu logic của bạn là AI
        public string Bookings { get; set; } = "Bookings";
        public string ChatMessages { get; set; } = "ChatMessages"; // Thêm thuộc tính này để lưu trữ tên collection chat messages
        public string Branches { get; set; } = "Branches";
        public string Departments { get; set; } = "Departments";
        public string MedicalRecords { get; set; } = "MedicalRecords";
        public string Packages { get; set; } = "Packages";
        public string Patients { get; set; } = "Patients";
        public string Payments { get; set; } = "Payments";
        public string ConfirmAppointment { get; set; } = "ConfirmAppointment";
        public string Specialties { get; set; } = "Specialties";
        public string UserQuestions { get; set; } = "UserQuestions";

        public string UsersGoogleCollectionName { get; set; } = "GoogleUsers";
    }


    namespace backend.Settings
    {
        public class JwtSettings
        {
            public string Key { get; set; } = string.Empty;
            public string Issuer { get; set; } = string.Empty;
            public string Audience { get; set; } = string.Empty;
            public int DurationInMinutes { get; set; }
        }
    }
}