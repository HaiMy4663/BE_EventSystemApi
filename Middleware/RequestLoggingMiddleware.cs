using System.Diagnostics;

namespace EventSystemAPI.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger) 
        { 
            _next = next; 
            _logger = logger; 
        }

        public async Task Invoke(HttpContext context)
        {
            var sw = Stopwatch.StartNew();
            await _next(context);
            sw.Stop();
            
            // Ghi log: Method, Đường dẫn, Mã trạng thái (200/404...), Thời gian xử lý
            _logger.LogInformation($"[API LOG] {context.Request.Method} {context.Request.Path} | Status: {context.Response.StatusCode} | Time: {sw.ElapsedMilliseconds}ms");
        }
    }
}