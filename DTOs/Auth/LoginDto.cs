using System.ComponentModel.DataAnnotations;

namespace EventSystemAPI.DTOs.Auth 
{
    /// <summary>
    /// </summary>
    public class LoginDto 
    {
        [Required(ErrorMessage = "Vui lòng nhập Email.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")] 
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Mật khẩu.")] 
        public string Password { get; set; }
    }
}