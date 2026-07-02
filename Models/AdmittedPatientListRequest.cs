namespace DoctorMobileApp.Models
{
    public class AdmittedPatientListRequest
    {
        public int WardIDF { get; set; }
        public string Filter { get; set; } = string.Empty;
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public int HospitalIDF { get; set; }
        public List<WardList> WardList { get; set; } = new List<WardList>();
        public List<DoctorList> DoctorList { get; set; } = new List<DoctorList>();
        public List<AdmittedPatientList> AdmittedPatientList { get; set; } = new List<AdmittedPatientList>();
    }
    public class AdmittedPatientList
    {
        public int IPDAdmissionDischargeIDP { get; set; }
        public DateTime? AdmissionDateTime { get; set; }
        public string IPDRegistrationCode { get; set; } = string.Empty;
        public int PatientIDF { get; set; }
        public bool? NonCashLess { get; set; }
        public int? ClassIDF { get; set; }
        public int? PrimaryDocIDF { get; set; }

        public string Gender { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string CRNumber { get; set; } = string.Empty;

        public string PatientName { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;

        public string ClassName { get; set; } = string.Empty;

        public string BedName { get; set; } = string.Empty;
        public string BedCode { get; set; } = string.Empty;
        public int? BedIDP { get; set; }

        public int? WardIDP { get; set; }
        public string WardName { get; set; } = string.Empty;
        public int? WardTypeIDF { get; set; }

        public int OtherWardIDF { get; set; }
        public string OtherWardName { get; set; } = string.Empty;
        public int OtherWardTypeIDF { get; set; }

        public string FloorName { get; set; } = string.Empty;
        public string BlockName { get; set; } = string.Empty;
    }

    public class WardList
    {
        public int WardIDP { get; set; }
        public string WardName { get; set; } = string.Empty;
    }
    public class DoctorList
    {
        public int EmployeeIDP { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string SkillSetName { get; set; } = string.Empty;
    }
    public class AdmittedPatienttRequest
    {
        public int WardIDF { get; set; }
        public string Filter { get; set; } = string.Empty;
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
