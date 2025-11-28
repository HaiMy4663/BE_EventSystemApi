using System.ComponentModel.DataAnnotations;

namespace EventSystemAPI.DTOs.User
{
    public class RegistrationRequestDto
    {
        [Required(ErrorMessage = "Vui lòng chọn loại vé muốn mua")]
        public int TicketId { get; set; } 
    }
}