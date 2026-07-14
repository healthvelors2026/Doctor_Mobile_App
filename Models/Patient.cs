using System.ComponentModel.DataAnnotations;

namespace MobieAppPatientFeedback.Models
{
    public class Patient
    {
        [Key]
        public int PatientIDP { get; set; }
        public string? CRNumber { get; set; }
        public string? FName { get; set; }
        public string? LName { get; set; }
    }
}
