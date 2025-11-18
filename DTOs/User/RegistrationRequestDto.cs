using System.ComponentModel.DataAnnotations;

namespace EventSystemAPI.DTOs.User
{
    public class RegistrationRequestDto
    {
        // [QUAN TRỌNG] User chỉ cần gửi TicketId muốn mua.
        // Server sẽ tự truy ra EventId từ TicketId này.
        [Required(ErrorMessage = "Vui lòng chọn loại vé muốn mua")]
        public int TicketId { get; set; } 
    }
}