using EventSystemAPI.Data;
using EventSystemAPI.DTOs.Event;
using EventSystemAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventSystemAPI.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = "Admin")] // BẢO MẬT: Chỉ Admin mới được vào
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // [GET] /api/admin/events: Xem danh sách (Có phân trang)
        [HttpGet("events")]
        public async Task<IActionResult> GetEvents([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var query = _context.Events.AsQueryable();
            var totalCount = await query.CountAsync();

            var events = await query
                .OrderByDescending(e => e.StartDate)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new EventResponseDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    Location = e.Location,
                    Status = e.Status,
                    TotalSlots = e.TotalSlots,
                    Tickets = e.Tickets.Select(t => new TicketResponseDto 
                    { 
                        AvailableQuantity = t.Quantity 
                    }).ToList()
                })
                .ToListAsync();

            return Ok(new { TotalCount = totalCount, Data = events });
        }

        // [GET] /api/admin/events/{id}: Xem chi tiết 1 sự kiện 
        [HttpGet("events/{id}")]
        [ActionName(nameof(GetEventById))] // Đặt tên hành động để CreatedAtAction tìm thấy
        public async Task<IActionResult> GetEventById(int id)
        {
            var evt = await _context.Events
                .Include(e => e.Tickets)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (evt == null) return NotFound();

            return Ok(new EventResponseDto
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
                    AvailableQuantity = t.Quantity
                }).ToList()
            });
        }

        // [POST] /api/admin/events: Tạo mới
        [HttpPost("events")]
        public async Task<IActionResult> CreateEvent([FromBody] EventCreateDto dto)
        {
            var newEvent = new Event
            {
                Name = dto.Name,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Location = dto.Location,
                TotalSlots = dto.TotalSlots,
                Status = "Published"
            };

            if (dto.Tickets != null && dto.Tickets.Count > 0)
            {
                newEvent.Tickets = dto.Tickets.Select(t => new Ticket
                {
                    TypeName = t.TypeName,
                    Price = t.Price,
                    Quantity = t.Quantity
                }).ToList();
            }

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            // 4. Trả về 201 Created với Header Location trỏ đúng hàm GetEventById
            return CreatedAtAction(nameof(GetEventById), new { id = newEvent.Id }, newEvent);
        }

        // [PUT] /api/admin/events/{id}
        [HttpPut("events/{id}")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] EventUpdateDto dto)
        {
            var evt = await _context.Events
                .Include(e => e.Tickets) // Load vé cũ lên để so sánh
                .FirstOrDefaultAsync(e => e.Id == id);

            if (evt == null) return NotFound(new { Message = "Không tìm thấy sự kiện" });

            // 1.Tổng sức chứa vs Vé đã bán
            var activeRegistrationsCount = await _context.Registrations
                .CountAsync(r => r.EventId == id && r.Status == "Active");

            if (dto.TotalSlots < activeRegistrationsCount)
            {
                return BadRequest(new { Message = $"Không thể giảm Sức Chứa xuống {dto.TotalSlots} vì đã có {activeRegistrationsCount} người mua vé." });
            }

            // 2.Tổng vé vs Sức chứa
            if (dto.Tickets != null)
            {
                var totalTicketQty = dto.Tickets.Sum(t => t.Quantity);
                if (totalTicketQty > dto.TotalSlots)
                {
                    return BadRequest(new { Message = $"Tổng số lượng vé ({totalTicketQty}) lớn hơn Sức Chứa ({dto.TotalSlots})." });
                }
            }

            // 3. Cập nhật thông tin Event
            evt.Name = dto.Name;
            evt.Description = dto.Description;
            evt.StartDate = dto.StartDate;
            evt.EndDate = dto.EndDate;
            evt.Location = dto.Location;
            evt.TotalSlots = dto.TotalSlots; 
            evt.Status = dto.Status ?? evt.Status;

            // 4. CẬP NHẬT VÉ 
            if (dto.Tickets != null)
            {
                foreach (var tDto in dto.Tickets)
                {
                    // Tìm xem vé loại này đã có chưa (dựa vào Tên)
                    var existingTicket = evt.Tickets.FirstOrDefault(t => t.TypeName == tDto.TypeName);
                    if (existingTicket != null)
                    {
                        // Đã có -> Cập nhật số lượng và giá
                        existingTicket.Price = tDto.Price;
                        existingTicket.Quantity = tDto.Quantity;
                    }
                    else
                    {
                        // Chưa có -> Thêm mới
                        evt.Tickets.Add(new Ticket 
                        { 
                            TypeName = tDto.TypeName, 
                            Price = tDto.Price, 
                            Quantity = tDto.Quantity 
                        });
                    }
                }

                // Xóa những vé không còn trong danh sách gửi lên
                // (Lấy danh sách tên vé mới)
                var newTicketNames = dto.Tickets.Select(t => t.TypeName).ToList();
                // (Tìm những vé cũ không nằm trong danh sách mới)
                var ticketsToDelete = evt.Tickets.Where(t => !newTicketNames.Contains(t.TypeName)).ToList();

                if (ticketsToDelete.Any())
                {
                    // Chỉ xóa nếu chưa ai mua (Check DB)
                    foreach (var tDel in ticketsToDelete)
                    {
                        var isSold = await _context.Registrations.AnyAsync(r => r.TicketId == tDel.Id);
                        if (!isSold)
                        {
                            _context.Tickets.Remove(tDel);
                        }
                        // Nếu đã bán rồi thì không xóa được, giữ lại để bảo toàn dữ liệu)
                    }
                }
            }

            try 
            {
                await _context.SaveChangesAsync();
                return Ok(new { Message = "Cập nhật thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi hệ thống: " + ex.Message });
            }
        }
        // [DELETE] /api/admin/events/{id}: Xóa
        [HttpDelete("events/{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var evt = await _context.Events.FindAsync(id);
            if (evt == null) return NotFound();

            // Nhờ cấu hình OnDelete Cascade, nó sẽ tự xóa Ticket và Registration liên quan.
            _context.Events.Remove(evt);
            await _context.SaveChangesAsync();

            return Ok("Đã xóa sự kiện và dữ liệu liên quan.");
        }

        // [GET] /api/admin/events/{id}/users
        [HttpGet("events/{id}/users")]
        public async Task<IActionResult> GetEventRegistrations(int id)
        {
            var list = await _context.Registrations
                .Where(r => r.EventId == id)
                .Include(r => r.User)
                .Include(r => r.Ticket)
                .Select(r => new EventRegistrationDto
                {
                    RegistrationId = r.Id,
                    UserFullName = r.User.FullName,
                    UserEmail = r.User.Email,
                    TicketType = r.Ticket.TypeName,
                    PricePaid = r.Ticket.Price,
                    RegisteredAt = r.RegisteredAt,
                    Status = r.Status
                })
                .ToListAsync();

            return Ok(list);
        }
 
        // [GET] /api/admin/dashboard/stats
        [HttpGet("dashboard/stats")]
        public async Task<IActionResult> GetDashboardStats()
        {

            var totalEvents = await _context.Events.CountAsync();
            var totalUsers = await _context.Users.CountAsync();
            var totalRevenue = await _context.Registrations
                                .Include(r => r.Ticket)
                                .Where(r => r.Status == "Active") 
                                .SumAsync(r => (decimal?)r.Ticket.Price ?? 0); 

            var topEvents = await _context.Registrations
                .Include(r => r.Event) 
                .Where(r => r.Status == "Active")
                .GroupBy(r => r.Event.Name)
                .Select(g => new { EventName = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            var topUsers = await _context.Registrations
                .Include(r => r.User)
                .Where(r => r.Status == "Active")
                .GroupBy(r => r.User.FullName) 
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            return Ok(new 
            { 
                TotalEvents = totalEvents, 
                TotalUsers = totalUsers, 
                TotalRevenue = totalRevenue,
                TopEvents = topEvents,
                TopUsers = topUsers 
            });
        }
    }
}