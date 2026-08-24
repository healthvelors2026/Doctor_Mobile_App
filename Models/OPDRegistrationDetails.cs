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
        public bool? NonCashLess { get; set; }
        public bool ClassForReimbursement { get; set; }
        public int RateBasedOn { get; set; }
        public int ChargeType { get; set; }
        public string? PatientName { get; set; }
        public string? DoctorName { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? RefDocName { get; set; }
        public string? ServiceName { get; set; }
        public int OPDEntryIDP { get; set; }
        public string PathoRemarks { get; set; } = string.Empty;
        public string RadioRemarks { get; set; } = string.Empty;
        public string ProcRemarks { get; set; } = string.Empty;
        public string PathoItems { get; set; } = string.Empty;
        public string RadioItems { get; set; } = string.Empty;
        public string ProcedureItems { get; set; } = string.Empty;
    }
    public class OPDRegistrationDetailsRequest
    {
        public string RegistrationCode { get; set; } = string.Empty;
    }

    public class DoctorOPDEntryTokenListRequest
    {
        public int DoctorIDF { get; set; }
    }

    // Property names must match the SP's output columns exactly (case-insensitive) for QueryAsync<T>'s mapping - Token_Status keeps the underscore for that reason.
    public class DoctorOPDEntryTokenList
    {
        public int TokenIssueIDP { get; set; }
        public DateTime? RegistrationDateTime { get; set; }
        public string? RegistrationCode { get; set; }
        public string? PatientName { get; set; }
        public string? CRNumber { get; set; }
        public int TokenNumber { get; set; }
        public DateTime TokenIssueDateTime { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public string? PatientType { get; set; }
        public int Token_Status { get; set; }
        public string? TokenStatus { get; set; }
    }

    public class ConsultingRoom
    {
        public int RoomID { get; set; }
        public string? RoomName { get; set; }
        public int RoomNo { get; set; }
        public int? RoomtAllocatedDoctorID { get; set; }
    }

    public class InsertTokenDisplayRequest
    {
        public int RoomIDF { get; set; }
        public int TokenIDF { get; set; }
        public int DoctorIDF { get; set; }
    }

    public class InsertTokenDisplayResult
    {
        public int IsInserted { get; set; }
        public string? Message { get; set; }
    }
}
