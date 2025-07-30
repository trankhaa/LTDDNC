// Program.cs

using backend.Data;
using backend.Helper;
using Resend;
using backend.Models.Entities;
using backend.Models.Entities.Doctor;
using backend.Services.Packages;
using backend.Services.Tiktok;
using backend.Services.Patient;
using backend.Services.UploadFile;
using backend.Services.User;
using backend.Services;
using backend.Services.Hosted;
using backend.Models.Entities.Booking;
using backend.Settings;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Net.payOS;
using Net.payOS.Types;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using System.IO;
using backend.Services.GoogleAuth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;


// using backend.Services.GoogleAuth;


using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// === 1. Đọc và Cấu hình Settings ===
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
var mongoDbSettings = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();


builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddOptions();
builder.Services.AddHttpClient<ResendClient>();
builder.Services.Configure<ResendClientOptions>(builder.Configuration.GetSection("Resend")); // Lấy config từ appsettings.json
builder.Services.AddTransient<IResend, ResendClient>();

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPackageService, PackageService>();

builder.Services.AddScoped<IMongoCollection<ConfirmAppointment>>(sp =>
{
    var database = sp.GetRequiredService<IMongoDatabase>();
    // Lấy tên collection từ file settings để dễ thay đổi
    var collectionName = builder.Configuration["MongoDbSettings:ConfirmAppointmentCollectionName"];
    if (string.IsNullOrEmpty(collectionName))
    {
        throw new InvalidOperationException("ConfirmAppointmentCollectionName is not configured in MongoDbSettings.");
    }
    return database.GetCollection<ConfirmAppointment>(collectionName);
});

if (mongoDbSettings == null || string.IsNullOrEmpty(mongoDbSettings.ConnectionString) || string.IsNullOrEmpty(mongoDbSettings.DatabaseName))
{
    throw new InvalidOperationException("MongoDbSettings configuration section is missing or invalid. Ensure ConnectionString and DatabaseName are set.");
}
// builder.Services.Configure<GoogleAuthSettings>(builder.Configuration.GetSection("GoogleAuthSettings"));

// === 2. Đăng ký Services với Dependency Injection ===
// Đăng ký MongoClient như một Singleton
builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(mongoDbSettings.ConnectionString));
// builder.Services.Configure<backend.Services.GoogleAuth.GoogleAuthSettings>(builder.Configuration.GetSection("GoogleAuth"));

// builder.Services.AddSingleton<backend.Services.GoogleAuth.IGoogleUserService, backend.Services.GoogleAuth.GoogleUserService>();
// builder.Services.AddSingleton<backend.Services.GoogleAuth.IGoogleAuthService, backend.Services.GoogleAuth.GoogleAuthService>();
// Đăng ký IMongoDatabase (Scoped)
builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(mongoDbSettings.DatabaseName);
});


// 3. Đăng ký các services với Dependency Injection
builder.Services.AddSingleton<TokenStorageService>();
builder.Services.AddHttpClient();

builder.Services.AddScoped<IMongoCollection<User>>(serviceProvider =>
{
    var database = serviceProvider.GetRequiredService<IMongoDatabase>();
    // Chỗ này có thể là vấn đề nếu mongoDbSettings.Collections là null
    // hoặc mongoDbSettings.Collections.Users là null hoặc rỗng
    return database.GetCollection<User>(mongoDbSettings.Collections.Users);
});

// Đăng ký MongoDbContext (nếu bạn sử dụng nó để quản lý các collections)
builder.Services.AddScoped<MongoDbContext>();

// Đăng ký các Application Services (ưu tiên đăng ký interface trỏ đến implementation)
builder.Services.AddScoped<IUploadFileService, UploadFileService>();
builder.Services.AddScoped<IBcryptHelper, BcryptHelper>();
// builder.Services.AddScoped<IGoogleUserService, GoogleUserService>();

// --- User related services ---

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();

builder.Services.AddHostedService<ExpiredAppointmentCleanerService>();

// --- Doctor related services ---
builder.Services.AddScoped<IDoctorService, DoctorService>(); // ✅ Controller sẽ inject IDoctorService
builder.Services.AddScoped<IDoctorDetailService, DoctorDetailService>(); // ✅ Controller sẽ inject IDoctorDetailService
builder.Services.AddScoped<DoctorService>();
// Đăng ký DoctorScheduleService trực tiếp nếu không có interface và được inject như class
// Hoặc nếu có interface IDoctorScheduleService: builder.Services.AddScoped<IDoctorScheduleService, DoctorScheduleService>();
builder.Services.AddScoped<DoctorScheduleService>();
builder.Services.AddScoped<Net.payOS.PayOS>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var clientId = config["PayOS:ClientId"];
    var apiKey = config["PayOS:ApiKey"];
    var checksumKey = config["PayOS:ChecksumKey"];

    return new Net.payOS.PayOS(clientId, apiKey, checksumKey);
});

builder.Services.AddScoped<DoctorDetailService>();

// --- Booking related services ---
// Giả sử ConfirmAppointmentService không có interface và được inject trực tiếp
// Hoặc nếu có interface IConfirmAppointmentService: builder.Services.AddScoped<IConfirmAppointmentService, ConfirmAppointmentService>();
builder.Services.AddScoped<ConfirmAppointmentService>();

// --- Other entity services ---
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<ISpecialtyService, SpecialtyService>();
// builder.Services.AddScoped<IPackageService, PackageService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IBranchService, BranchService>();
builder.Services.AddSingleton<HealthTipService>();
builder.Services.AddScoped<UserGGService>();

// IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
// var settings = configuration.GetSection("PayOS").Get<PayOSSettings>();
// PayOS payOS = new PayOS(
//     configuration["PayOS:ClientId"] ?? throw new Exception("Missing PayOS ClientId"),
//     configuration["PayOS:ApiKey"] ?? throw new Exception("Missing PayOS ApiKey"),
//     configuration["PayOS:ChecksumKey"] ?? throw new Exception("Missing PayOS ChecksumKey")
// );

// builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();

// === 3. Cấu hình Controllers và Views ===
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();

// Authentication: JWT + Cookies + Google OAuth
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "your_issuer",
        ValidAudience = "your_audience",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your_secret_key"))
    };
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{
    options.ClientId = builder.Configuration["Google:ClientId"];
    options.ClientSecret = builder.Configuration["Google:ClientSecret"];
    options.CallbackPath = "/signin-google";
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy =>
        policy.RequireRole("Admin"));
});

// === 5. Cấu hình CORS ===

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                    "http://localhost:5173", // React dev server (nếu có)
                    "http://localhost"       // Cho phép các port khác của Flutter khi debug
                 )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Nếu frontend gửi credentials (cookies)
    });
});



// === 6. Cấu hình Kestrel ===
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    // serverOptions.ListenLocalhost(2000);
    serverOptions.ListenAnyIP(2000);
});

// === 7. Cấu hình Swagger/OpenAPI ===
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "My Clinic API",
        Version = "v1",
        Description = "API for Clinic Management Application",
        Contact = new OpenApiContact { Name = "Your Name", Email = "your.email@example.com" }
    });
    c.CustomSchemaIds(type => type.FullName);
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});



// === Đảm bảo thư mục uploads tồn tại trước khi Build() ===
// Điều này giúp tránh lỗi DirectoryNotFoundException khi PhysicalFileProvider khởi tạo
string uploadsFolderPath = Path.Combine(builder.Environment.ContentRootPath, "uploads");
if (!Directory.Exists(uploadsFolderPath))
{
    try
    {
        Directory.CreateDirectory(uploadsFolderPath);
        Console.WriteLine($"[INFO] Created directory: {uploadsFolderPath}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] Failed to create directory {uploadsFolderPath}: {ex.Message}");
        // Bạn có thể quyết định throw exception ở đây nếu việc tạo thư mục là bắt buộc
        // throw;
    }
}
// =========================================================

var app = builder.Build();

// ==================================================================================
// === 8. Cấu hình HTTP Request Pipeline (Middleware) ===
// ==================================================================================

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My Clinic API V1");
        c.RoutePrefix = string.Empty;
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
// Thêm vào Program.cs để debug webhook
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api/Payment/payos-webhook-handler"))
    {
        Console.WriteLine($"🔔 WEBHOOK REQUEST RECEIVED:");
        Console.WriteLine($"   Method: {context.Request.Method}");
        Console.WriteLine($"   Path: {context.Request.Path}");
        Console.WriteLine($"   Headers: {string.Join(", ", context.Request.Headers.Select(h => $"{h.Key}={h.Value}"))}");

        // Đọc body để log
        context.Request.EnableBuffering();
        var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
        context.Request.Body.Position = 0;
        Console.WriteLine($"   Body: {body}");
    }

    await next();
});

// Đặt TRƯỚC app.MapControllers();
// HTTPS Redirection (nếu bạn có chứng chỉ SSL cho localhost hoặc production)
// app.UseHttpsRedirection(); // Bỏ comment nếu cần

// --- Cấu hình Static Files ---
// Phục vụ file từ wwwroot (nếu có)
app.UseStaticFiles();

// Phục vụ file từ thư mục "uploads"
// Kiểm tra lại sự tồn tại của thư mục trước khi cấu hình FileProvider
// Mặc dù đã cố gắng tạo ở trên, nhưng kiểm tra lại cũng không thừa
if (Directory.Exists(uploadsFolderPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(uploadsFolderPath),
        RequestPath = "/uploads"
    });
    Console.WriteLine($"[INFO] Serving static files from: {uploadsFolderPath} at /uploads");
}
else
{
    Console.WriteLine($"[WARNING] Directory not found, not serving static files from: {uploadsFolderPath}");
}


app.UseRouting();

// --- Áp dụng CORS Policy ---
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

// --- Map Endpoints ---
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public class PayOSSettings
{
    public string? ClientId { get; set; }
    public string? ApiKey { get; set; }
    public string? ChecksumKey { get; set; }
}