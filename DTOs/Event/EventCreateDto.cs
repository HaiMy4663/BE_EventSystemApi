using System.ComponentModel.DataAnnotations;

namespace EventSystemAPI.DTOs.Event
{
    public class EventCreateDto
    {
        [Required] public string Name { get; set; }
        public string Description { get; set; }
        [Required] public DateTime StartDate { get; set; }
        [Required] public DateTime EndDate { get; set; }
        [Required] public string Location { get; set; }
        [Required] [Range(1, int.MaxValue)] public int TotalSlots { get; set; }

        // Admin nhập luôn danh sách các loại vé khi tạo sự kiện
        public List<TicketCreateDto> Tickets { get; set; }
    }

    public class TicketCreateDto
    {
        [Required] public string TypeName { get; set; }
        [Required] [Range(0, double.MaxValue)] public decimal Price { get; set; }
        [Required] [Range(1, int.MaxValue)] public int Quantity { get; set; }
    }
}