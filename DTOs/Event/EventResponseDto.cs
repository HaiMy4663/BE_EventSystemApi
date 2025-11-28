namespace EventSystemAPI.DTOs.Event
{
    public class EventResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; }
        public int TotalSlots { get; set; }
        public string Status { get; set; }
        
        // Danh sách vé đi kèm
        public List<TicketResponseDto> Tickets { get; set; }
    }
}