// Thay số 5245 bằng port máy bạn (xem trên Swagger)
const API_BASE_URL = "http://localhost:5245/api"; 

// Hàm tiện ích để định dạng tiền Việt Nam (Ví dụ: 500,000 VNĐ)
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
}

// Hàm tiện ích để định dạng ngày tháng (Ví dụ: 31/12/2025)
function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('vi-VN');
}
// Hàm dùng chung cho tất cả các file HTML (Tạo Toast Notification)
function showToast(icon, message, timer = 2500) {
    Swal.fire({
        toast: true,
        position: 'top-end',
        icon: icon,
        title: message,
        showConfirmButton: false,
        timer: timer,
        timerProgressBar: true
    });
}

// Hàm Logout dùng chung
function logout() {
    localStorage.clear();
    window.location.href = "index.html";
}