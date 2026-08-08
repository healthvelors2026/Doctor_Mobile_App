using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoctorMobileApp.Models.PatientFeedback
{
    [Table("tbFeedbackToken")]
    public class tbFeedbackToken
    {
        [Key]
        public long FeedbackTokenID { get; set; }
        [MaxLength(20)]
        public string Token { get; set; } = "";
        public int PatientIDF { get; set; }
        public int RegistrationIDF { get; set; }
        public byte RegistrationType { get; set; }
        public int HospitalIDF { get; set; }
        public int GeneratedByUserIDF { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsUsed { get; set; }
        public bool IsActive { get; set; }
    }
    public class FeedbackTokenSettings
    {
        public int TokenExpiryHours { get; set; } = 48;
    }
}