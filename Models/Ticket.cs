using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventSystemAPI.Models
{
    public class Ticket
    {
        [Key] 
        public int Id { get; set; }

        [Required] 
        [ForeignKey("Event")] 
        public int EventId { get; set; }
        public Event Event { get; set; } // Liên kết ngược về Event

        [Required] 
        [StringLength(50)] 
        public string TypeName { get; set; } 

        [Required] 
        [Range(0, double.MaxValue)] 
        [Column(TypeName = "decimal(18,2)")] 
        public decimal Price { get; set; }

        [Required] 
        [Range(1, int.MaxValue)] 
        public int Quantity { get; set; } 
    }
}