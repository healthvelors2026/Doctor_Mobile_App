using System.ComponentModel.DataAnnotations;

namespace DoctorMobileApp.Models.PatientFeedback
{
    public class Patient
    {
        [Key]
        public int PatientIDP { get; set; }

        public string? CRNumber { get; set; }

        public string? FName { get; set; }

        public string? LName { get; set; }
    }

    public class Employee
    {
        [Key]
        public int EmployeeIDP { get; set; }

        public string? FName { get; set; }

        public string? MName { get; set; }

        public string? LName { get; set; }
    }

    public class OpdRegistration
    {
        [Key]
        public int OPDRegistrationIDP { get; set; }

        public string? RegistrationCode { get; set; }

        public DateTime RegistrationDateTime { get; set; }

        public int PatientIDF { get; set; }

        public int? DoctorIDF { get; set; }
    }

    public class IpdAdmissionDischarge
    {
        [Key]
        public int IPDAdmissionDischargeIDP { get; set; }

        public DateTime AdmissionDateTime { get; set; }

        public string IPDRegistrationCode { get; set; } = "";

        public int PatientIDF { get; set; }

        public int ClassIDF { get; set; }

        public int PrimaryDocIDF { get; set; }

        public DateTime? DischargeDate { get; set; }
    }

    public class ClassMaster
    {
        [Key]
        public int ClassIDP { get; set; }

        public string? ClassName { get; set; }
    }
}