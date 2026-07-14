using System.ComponentModel.DataAnnotations;

namespace DoctorMobileApp.Models
{
    public class ClassMaster
    {
        [Key]
        public int ClassIDP { get; set; }

        public string? ClassName { get; set; }
    }
}
