using EventSystemAPI.Data;
using EventSystemAPI.DTOs.Event;
using EventSystemAPI.DTOs.User;
using EventSystemAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EventSystemAPI.Controllers
{
    [Route("api")] // URL gốc là /api
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. [GET] /api/events: Xem danh sách sự kiện
        [HttpGet("events")]
        public async Task<IActionResult> GetEvents()
        {
            // Chỉ lấy sự kiện đang Published
            var events = await _context.Events
                .Where(e => e.Status == "Published")
                .Select(e => new EventResponseDto 
                {
                    Id = e.Id,
                    Name = e.Name,
                    Description = e.Description,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    Location = e.Location,
                    TotalSlots = e.TotalSlots,
                    Status = e.Status
                })
                .ToListAsync();

            return Ok(events);
        }

        // 2. [GET] /api/events/{id}: Xem chi tiết sự kiện + Danh sách Vé
        [HttpGet("events/{id}")]
        public async Task<IActionResult> GetEventDetail(int id)
        {
            var evt = await _context.Events
                .Include(e => e.Tickets) 
                .FirstOrDefaultAsync(e => e.Id == id);

            if (evt == null) return NotFound(new { Message = "Không tìm thấy sự kiện" });

            var response = new EventResponseDto
            {
                Id = evt.Id,
                Name = evt.Name,
                Description = evt.Description,
                StartDate = evt.StartDate,
                EndDate = evt.EndDate,
                Location = evt.Location,
                TotalSlots = evt.TotalSlots,
                Status = evt.Status,
                Tickets = evt.Tickets.Select(t => new TicketResponseDto
                {
                    Id = t.Id,
                    TypeName = t.TypeName,
                    Price = t.Price,
                    Quantity = t.Quantity,
                    // Tính số lượng còn lại = Tổng - Đã bán
                    AvailableQuantity = t.Quantity - _context.Registrations.Count(r => r.TicketId == t.Id)
                }).ToList()
            };

            return Ok(response);
        }

        //  (Chỉ User mới được dùng)
        // 3. [POST] /api/events/register: MUA VÉ (API KHÓ NHẤT)
        [HttpPost("events/register")]
        [Authorize(Roles = "User")] // Bắt buộc phải là User
        public async Task<IActionResult> RegisterEvent([FromBody] RegistrationRequestDto dto)
        {
            // Lấy ID user đang đăng nhập từ Token
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            // Bước 1: Lấy thông tin vé user muốn mua
            var ticket = await _context.Tickets
                .Include(t => t.Event) // Load kèm sự kiện cha
                .FirstOrDefaultAsync(t => t.Id == dto.TicketId);

            if (ticket == null) return NotFound(new { Message = "Vé không tồn tại" });

            // Sự kiện có tồn tại (và đang mở) không?
            if (ticket.Event.Status != "Published" || ticket.Event.EndDate < DateTime.Now)
            {
                return BadRequest(new { Message = "Sự kiện này đã kết thúc hoặc tạm đóng." });
            }

            // Check trùng lặp 
            var daMua = await _context.Registrations
                .AnyAsync(r => r.UserId == userId && r.EventId == ticket.EventId);
            
            if (daMua) return BadRequest(new { Message = "Bạn đã đăng ký sự kiện này rồi." });

            // Check số lượng slot 
            var soLuongDaBan = await _context.Registrations.CountAsync(r => r.TicketId == dto.TicketId);
            
            if (soLuongDaBan >= ticket.Quantity)
            {
                return BadRequest(new { Message = "Rất tiếc, loại vé này đã hết hàng." });
            }

            // -> Tạo đăng ký mới
            var registration = new Registration
            {
                UserId = userId,
                EventId = ticket.EventId,
                TicketId = ticket.Id,
                RegisteredAt = DateTime.Now,
                Status = "Active"
            };

            _context.Registrations.Add(registration);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Đăng ký vé thành công!", RegistrationId = registration.Id });
        }

        // 4. [GET] /api/users/history: Xem lịch sử vé đã mua
        [HttpGet("users/history")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetMyHistory()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var list = await _context.Registrations
                .Where(r => r.UserId == userId)
                .Include(r => r.Event)
                .Include(r => r.Ticket)
                .OrderByDescending(r => r.RegisteredAt)
                .Select(r => new RegistrationHistoryDto
                {
                    RegistrationId = r.Id,
                    EventId = r.EventId,
                    EventName = r.Event.Name,
                    StartDate = r.Event.StartDate,
                    Location = r.Event.Location,
                    TicketType = r.Ticket.TypeName,
                    TicketPrice = r.Ticket.Price,
                    Status = r.Status,
                    RegisteredAt = r.RegisteredAt,
                    // Chỉ cho hủy nếu sự kiện chưa diễn ra
                    CanCancel = r.Event.StartDate > DateTime.Now
                })
                .ToListAsync();

            return Ok(list);
        }

       // 5. [DELETE] /api/users/registrations/{id}: HỦY VÉ
        [HttpDelete("users/registrations/{id}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CancelRegistration(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var registration = await _context.Registrations
                .Include(r => r.Event)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (registration == null) return NotFound(new { Message = "Không tìm thấy vé." });
            if (registration.UserId != userId) return Forbid();
            // Hủy đăng ký (trước thời hạn)
            if (DateTime.Now >= registration.Event.StartDate.AddDays(-1))
            {
                return BadRequest(new { Message = "Đã quá hạn hủy vé (Quy định: Phải hủy trước sự kiện 24h)." });
            }

            // Xóa khỏi DB
            _context.Registrations.Remove(registration);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Đã hủy đăng ký thành công." });
        }

        // 6. [GET] /api/users/me: Xem thông tin cá nhân
        [HttpGet("users/me")]
        [Authorize] 
        public async Task<IActionResult> GetMyProfile()
        {
            // Lấy ID từ Token
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            
            var userId = int.Parse(userIdStr);

            // Tìm User trong DB
            var user = await _context.Users
                .Select(u => new 
                {
                    u.Id,
                    u.FullName,
                    u.Email,
                    u.Role
                    // Không trả về PasswordHash!
                })
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();

            return Ok(user);
        }
    }
}