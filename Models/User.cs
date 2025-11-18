using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EventSystemAPI.Models
{
    public class User
    {
        [Key] 
        public int Id { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")] 
        [StringLength(100)] // Giới hạn độ dài để tối ưu DB
        public string FullName { get; set; }

        [Required] 
        [EmailAddress(ErrorMessage = "Email sai định dạng")] 
        public string Email { get; set; }

        [Required] 
        [JsonIgnore] // [BẢO MẬT] Không bao giờ gửi mật khẩu mã hóa về cho Frontend
        public string PasswordHash { get; set; }

        // Chỉ chấp nhận 2 giá trị này
        [RegularExpression("^(Admin|User)$", ErrorMessage = "Role không hợp lệ")] 
        public string Role { get; set; } = "User";

        // Mối quan hệ: 1 User đăng ký nhiều Event
        public ICollection<Registration> Registrations { get; set; }
    }
}