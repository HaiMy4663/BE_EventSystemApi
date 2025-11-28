namespace EventSystemAPI.DTOs.Event 
{
    public class EventRegistrationDto 
    {
        public int RegistrationId { get; set; }
        public string UserFullName { get; set; } 
        public string UserEmail { get; set; }    
        public string TicketType { get; set; }   
        public decimal PricePaid { get; set; }   
        public DateTime RegisteredAt { get; set; }
        public string Status { get; set; }
    }
}