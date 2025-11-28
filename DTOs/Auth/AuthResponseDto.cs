namespace EventSystemAPI.DTOs.Auth 
{
    public class AuthResponseDto 
    { 
        public string Token { get; set; } 
        public string FullName { get; set; } 
        public string Role { get; set; } 
    }
}