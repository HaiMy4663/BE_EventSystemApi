using System.ComponentModel.DataAnnotations;

namespace EventSystemAPI.Models
{
    public class Event
    {
        [Key] 
        public int Id { get; set; }

        [Required] 
        [StringLength(200)] 
        public string Name { get; set; }

        public string Description { get; set; }

        [Required] 
        public DateTime StartDate { get; set; }

        [Required] 
        public DateTime EndDate { get; set; } 

        [Required] 
        public string Location { get; set; }

        [Range(1, int.MaxValue)] 
        public int TotalSlots { get; set; }

        public string Status { get; set; } = "Published";

        // Mối quan hệ: 1 Event có nhiều vé và nhiều người đăng ký
        public ICollection<Ticket> Tickets { get; set; }
        public ICollection<Registration> Registrations { get; set; }
    }
}