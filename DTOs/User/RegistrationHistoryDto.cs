namespace EventSystemAPI.DTOs.User
{
    public class RegistrationHistoryDto
    {
        public int RegistrationId { get; set; }
        
        // Thông tin sự kiện
        public int EventId { get; set; }
        public string EventName { get; set; }
        public DateTime StartDate { get; set; }
        public string Location { get; set; }
        
        // Thông tin vé đã mua
        public string TicketType { get; set; } // VD: Vé VIP
        public decimal TicketPrice { get; set; } // Giá lúc mua
        
        // Trạng thái
        public string Status { get; set; } // Active (Đang có hiệu lực) / Canceled (Đã hủy)
        public DateTime RegisteredAt { get; set; } // Ngày mua vé
        
        // [LOGIC FRONTEND] Biến này giúp FE biết có nên hiện nút "Hủy Vé" không
        // (Ví dụ: Sự kiện sắp diễn ra thì không cho hủy nữa)
        public bool CanCancel { get; set; } 
    }
}