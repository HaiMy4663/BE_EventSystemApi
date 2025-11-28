using System.ComponentModel.DataAnnotations;

namespace EventSystemAPI.DTOs.Event
{
    public class EventUpdateDto
    {
        [Required] public string Name { get; set; }
        public string Description { get; set; }
        [Required] public DateTime StartDate { get; set; }
        [Required] public DateTime EndDate { get; set; }
        [Required] public string Location { get; set; }
        [Required] public int TotalSlots { get; set; }
        public string Status { get; set; }

        // Cho phép gửi kèm danh sách vé để cập nhật
        public List<TicketCreateDto> Tickets { get; set; }
    }
}