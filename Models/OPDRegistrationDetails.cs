namespace DoctorMobileApp.Models
{
    public class OPDRegistration
    {
        public OPDRegistrationDetails OPDRegistrationDetails { get; set; } = new OPDRegistrationDetails();
        public TodayInvestigations TodayInvestigations { get; set; } = new TodayInvestigations();
        public List<InvestigationTestReport> PathoTestList { get; set; } = new List<InvestigationTestReport>();
        public List<InvestigationTestReport> RadioTestList { get; set; } = new List<InvestigationTestReport>();
        public List<InvestigationTestReport> ProcedureTestList { get; set; } = new List<InvestigationTestReport>();
    }
    public class OPDRegistrationDetails
    {
        public int OPDRegistrationIDP { get; set; }
        public DateTime? RegistrationDateTime { get; set; }
        public int PatientIDF { get; set; }
        public int DoctorIDF { get; set; }
        public int ClassIDF { get; set; }
        public string? ClassName { get; set; }
        public int NonCashLess { get; set; }
        public bool ClassForReimbursement { get; set; }
        public int RateBasedOn { get; set; }
        public int ChargeType { get; set; }
        public string? CRNumber { get; set; }
        //CRNumber
        public string? PatientName { get; set; }
        public string? DoctorName { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? RefDocName { get; set; }
        public string? ServiceName { get; set; }
        public bool IsHealthCardReg { get; set; }
        public int HCPatientIssueDetailIDF { get; set; }
        public string? HealthCardNumber { get; set; }
        public int OPDEntryIDP { get; set; }
        public string PathoRemarks { get; set; } = string.Empty;
        public string RadioRemarks { get; set; } = string.Empty;
        public string ProcRemarks { get; set; } = string.Empty;
        public string PathoItems { get; set; } = string.Empty;
        public string RadioItems { get; set; } = string.Empty;
        public string ProcedureItems { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
        public string BrowserName { get; set; } = string.Empty;
    }
    public class OPDRegistrationDetailsRequest
    {
        public string RegistrationCode { get; set; } = string.Empty;
    }
    public class SaveOPDEntryWithTestResponse
    {
        public bool Success { get; set; }
        public int OPDRegistrationIDF { get; set; }
        public int VisitIDF { get; set; }
        public string RegistrationCode { get; set; }= string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
