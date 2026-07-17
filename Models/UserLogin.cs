using System.ComponentModel.DataAnnotations;

namespace DoctorMobileApp.Models
{
    public class UserLogin
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int HospitalIDF { get; set; }
        public int HospitalGroupIDF { get; set; }
        public string HospitalName { get; set; } = string.Empty;
        public string HospitalCode { get; set; } = string.Empty;
        public int EmployeeIDF { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime ExpireDate { get; set; }
        public int FASModeOFPaymentIDF {  get; set; }

    }
    public class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int ExpiresInMinutes { get; set; }
    }
    public class UserLoginRequest
    {
        public string username { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public int IsKioskUser { get; set; }
    }
}
