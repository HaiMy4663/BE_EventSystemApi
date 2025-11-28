using System.ComponentModel.DataAnnotations;

namespace EventSystemAPI.DTOs.Auth 
{
    /// <summary>
    /// </summary>
    public class RegisterDto 
    {
        [Required(ErrorMessage = "Vui lòng nhập Họ và Tên.")] 
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Email.")] 
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")] 
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Mật khẩu.")] 
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")] 
        public string Password { get; set; }
    }
}