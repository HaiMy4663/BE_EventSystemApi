namespace EventSystemAPI.DTOs.Event
{
    public class TicketResponseDto
    {
        public int Id { get; set; }
        public string TypeName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int AvailableQuantity { get; set; } 
    }
}