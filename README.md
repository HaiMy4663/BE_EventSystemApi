Event Management System
- Hệ thống quản lý và bán vé sự kiện trực tuyến được xây dựng theo mô hình Client-Server, tách biệt hoàn toàn giữa Backend API và Frontend.
- Công nghệ sử dụng
Backend:
- ASP.NET Core Web API (.NET 8)
- Entity Framework Core (SQL Server)
- JWT Authentication
- RESTful API Standard
Frontend:
- HTML5, CSS3, JavaScript
- Bootstrap (Admin Template - SB Admin 2)
- Bootstrap 5 (User Template - Agency)
- SweetAlert2, Chart.js
Yêu cầu cài đặt
- .NET SDK 8.0 trở lên
- SQL Server (Express hoặc Developer)
- Visual Studio Code hoặc Visual Studio 2022
Git
Hướng dẫn triển khai
1. Cấu hình Backend
- Mở thư mục dự án Backend.
- Mở file appsettings.json.
- Cập nhật chuỗi kết nối ConnectionStrings:DefaultConnection với Server Name của máy tính hiện tại.
- Mở Terminal tại thư mục chứa file .csproj.
- Chạy lệnh sau để khởi tạo cơ sở dữ liệu và tài khoản Admin mặc định.
Bash
dotnet ef database update
Khởi chạy Server:
Bash
dotnet run
Lưu ý: Ghi nhớ số Port hiển thị trên Terminal
2. Cấu hình Frontend
- Truy cập thư mục ClientApp.
- Mở file config.js.
- Cập nhật giá trị API_BASE_URL trùng khớp với Port của Backend đang chạy.
JavaScript
const API_BASE_URL = "http://localhost:5245/api";
- Sử dụng Extension Live Server (trên VS Code) để chạy file index.html (User) hoặc admin/login.html (Admin).
- Tài khoản quản trị mặc định
- Hệ thống tự động tạo tài khoản Admin khi khởi tạo Database lần đầu:
Email: admin@system.com
Mật khẩu: M77830311
Tài liệu API
- Sau khi khởi chạy Backend, truy cập đường dẫn Swagger để xem tài liệu API chi tiết:
URL: http://localhost:<PORT>/swagger
Chức năng chính
- Người dùng (User):
Đăng ký, Đăng nhập.
Xem danh sách sự kiện, Chi tiết sự kiện.
Đặt vé, Xem lịch sử đặt vé, Hủy vé.
- Quản trị viên (Admin):
Đăng nhập bảo mật.
Dashboard thống kê (Doanh thu, Sự kiện, Top người dùng).
Quản lý sự kiện (Thêm, Sửa, Xóa).
Xem báo cáo danh sách người mua vé theo sự kiện.
