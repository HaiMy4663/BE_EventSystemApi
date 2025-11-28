using System.Net;
using System.Text.Json;

namespace EventSystemAPI.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger) 
        { 
            _next = next; 
            _logger = logger; 
        }

        public async Task Invoke(HttpContext context)
        {
            try 
            { 
                await _next(context); 
            }
            catch (Exception ex) 
            { 
                // Nếu có lỗi xảy ra ở bất cứ đâu...
                // 1. Ghi log lỗi thật vào console để Dev sửa
                _logger.LogError(ex, "Lỗi hệ thống nghiêm trọng");

                // 2. Trả về JSON thông báo lỗi thân thiện cho người dùng
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                
                var response = new { StatusCode = 500, Message = "Lỗi hệ thống nội bộ. Vui lòng liên hệ Admin." };
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }
    }
}