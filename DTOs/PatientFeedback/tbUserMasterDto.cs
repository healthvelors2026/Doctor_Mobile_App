namespace DoctorMobileApp.DTOs.PatientFeedback
{
    public class TbUserMasterDto
    {
        public int UserIDP { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public int UserCategoryIDF { get; set; }

        public bool Active { get; set; }

        public bool Admin { get; set; }

        public bool AllowReports { get; set; }

        public bool DiscountAuthority { get; set; }

        public bool HRAdmin { get; set; }

        public bool PathoAdmin { get; set; }

        public bool ServerAdmin { get; set; }

        public int UserType { get; set; }

        public int? EmployeeIDF { get; set; }

        public bool LockUser { get; set; }

        public int HospitalIDF { get; set; }

        public bool CLReceiptDiscountRights { get; set; }

        public bool MedicalCollegeAdmin { get; set; }

        public bool POSUser { get; set; }

        public bool IsMedicalCollegeAdmission { get; set; }

        public int LoginFailAttemptCounts { get; set; }

        public DateTime? EntryDateTime { get; set; }

        public string? IPAddress { get; set; }

        public string? BrowserName { get; set; }
    }
}