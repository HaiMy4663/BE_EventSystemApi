using EventSystemAPI.Data;
using EventSystemAPI.DTOs.Auth;
using EventSystemAPI.Helpers;
using EventSystemAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EventSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // [POST] /api/Auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return BadRequest(new { Message = "Email này đã tồn tại." });
            }
            // Hash mật khẩu (Không lưu plain text)
            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = AppUtils.HashPassword(dto.Password),
                Role = "User" // Mặc định là User thường
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Đăng ký thành công!" });
        }

        // [POST] /api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || !AppUtils.VerifyPassword(dto.Password, user.PasswordHash))
            {
                return Unauthorized(new { Message = "Email hoặc mật khẩu sai." });
            }

            // Sinh JWT Token
            var token = CreateToken(user);

            return Ok(new AuthResponseDto
            {
                Token = token,
                FullName = user.FullName,
                Role = user.Role
            });
        }

        // Hàm phụ: Tạo chuỗi Token
        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Lưu ID để các API sau dùng
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}