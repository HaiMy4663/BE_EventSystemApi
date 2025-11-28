namespace EventSystemAPI.DTOs.User
{
    public class RegistrationHistoryDto
    {
        public int RegistrationId { get; set; }
        
        public int EventId { get; set; }
        public string EventName { get; set; }
        public DateTime StartDate { get; set; }
        public string Location { get; set; }
        
        public string TicketType { get; set; } 
        public decimal TicketPrice { get; set; } 

        public string Status { get; set; } 
        public DateTime RegisteredAt { get; set; } 
        
        // Biến này giúp FE biết có nên hiện nút "Hủy Vé" không
        public bool CanCancel { get; set; } 
    }
}