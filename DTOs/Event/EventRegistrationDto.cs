namespace EventSystemAPI.DTOs.Event 
{
    public class EventRegistrationDto 
    {
        public int RegistrationId { get; set; }
        public string UserFullName { get; set; } // Lấy tên user
        public string UserEmail { get; set; }    // Lấy email user
        public string TicketType { get; set; }   // Lấy tên vé
        public decimal PricePaid { get; set; }   // Lấy giá vé
        public DateTime RegisteredAt { get; set; }
        public string Status { get; set; }
    }
}