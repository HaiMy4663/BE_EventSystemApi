using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventSystemAPI.Models
{
    public class Registration
    {
        [Key] 
        public int Id { get; set; }

        [ForeignKey("User")] 
        public int UserId { get; set; }
        public User User { get; set; }

        [ForeignKey("Event")] 
        public int EventId { get; set; }
        public Event Event { get; set; }

        // Lưu vé ID để biết user mua loại vé nào
        [ForeignKey("Ticket")] 
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }

        public DateTime RegisteredAt { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Active";
    }
}