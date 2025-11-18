namespace EventSystemAPI.Helpers
{
    public static class AppUtils
    {
        // Hàm mã hóa mật khẩu (Dùng khi Đăng ký)
        public static string HashPassword(string password) 
        {
            // BCrypt sẽ tự sinh ra "muối" (salt) ngẫu nhiên, rất an toàn
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Hàm kiểm tra mật khẩu (Dùng khi Đăng nhập)
        public static bool VerifyPassword(string password, string hashedPassword) 
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}