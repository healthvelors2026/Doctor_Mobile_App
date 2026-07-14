using System.ComponentModel.DataAnnotations;

namespace DoctorMobileApp.PatientFeedback
{
    public class tbUserMaster
    {
        [Key]
        public int UserIDP { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public int UserCategoryIDF { get; set; }

        public bool Active { get; set; }

        public bool Admin { get; set; }

        public int UserType { get; set; }

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