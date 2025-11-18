using EventSystemAPI.Data;
using EventSystemAPI.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ==================================================================
// 1. KHU VỰC ĐĂNG KÝ DỊCH VỤ (SERVICES - Dependency Injection)
// ==================================================================

// [DATABASE] Kết nối SQL Server
// Lấy chuỗi kết nối từ file appsettings.json
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// [CORS] Cho phép Frontend (chạy port khác) gọi API
// Nếu thiếu dòng này, trình duyệt sẽ chặn kết nối vì lý do bảo mật
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => 
        policy.AllowAnyOrigin()  // Chấp nhận mọi nguồn (Domain)
              .AllowAnyMethod()  // Chấp nhận mọi hành động (GET, POST...)
              .AllowAnyHeader()); // Chấp nhận mọi Header
});

// [AUTHENTICATION] Cấu hình xác thực bảo mật bằng JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, // Kiểm tra server nào phát hành
            ValidateAudience = true, // Kiểm tra token dành cho ai
            ValidateLifetime = true, // Kiểm tra token còn hạn không
            ValidateIssuerSigningKey = true, // Kiểm tra chữ ký bí mật
            
            // Lấy thông tin cấu hình từ appsettings.json
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// [RATE LIMITING] Giới hạn tốc độ request (Chống Spam/DDoS)
// Yêu cầu đề bài: 50 request/phút/IP
builder.Services.AddRateLimiter(options =>
{
    // Nếu vượt quá giới hạn thì trả về lỗi 429 (Too Many Requests)
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        // Lấy IP người dùng để phân biệt
        var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        // Cấu hình cửa sổ trượt: 50 lượt trong 1 phút
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: remoteIp, 
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true, // Tự động hồi phục lượt
                PermitLimit = 50,
                Window = TimeSpan.FromMinutes(1)
            });
    });
});

builder.Services.AddControllers();

// [SWAGGER] Cấu hình giao diện test API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EventSystem API", Version = "v1" });
    
    // Thêm nút "Ổ khóa" (Authorize) vào giao diện Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        In = ParameterLocation.Header,
        Description = "Nhập vào: Bearer {token_cua_ban}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, new string[] {} }
    });
});

var app = builder.Build();

// ==================================================================
// 2. KHU VỰC PIPELINE (MIDDLEWARE - Luồng xử lý Request)
// Thứ tự dòng code ở đây RẤT QUAN TRỌNG. Request chảy từ trên xuống.
// ==================================================================

// 1. [Global Exception] Bắt lỗi toàn cục
// Đặt đầu tiên để nó bao bọc mọi lỗi xảy ra ở bên dưới
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 2. [Logging] Ghi lại nhật ký request (Ai gọi, gọi cái gì, bao lâu)
app.UseMiddleware<RequestLoggingMiddleware>();

// 3. [Rate Limiter] Kiểm tra xem có spam không
app.UseRateLimiter();

// 4. [CORS] Mở cửa cho Frontend vào (Phải đặt trước Auth)
app.UseCors("AllowAll");

// 5. [Auth] Kiểm tra danh tính (Bạn là ai? Có Token không?)
app.UseAuthentication();

// 6. [Auth] Kiểm tra quyền hạn (Bạn có phải Admin không?)
app.UseAuthorization();

// 7. Chuyển request đến đúng Controller xử lý
app.MapControllers();

app.Run();