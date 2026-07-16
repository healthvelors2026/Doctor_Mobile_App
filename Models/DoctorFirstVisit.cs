namespace DoctorMobileApp.Models
{
    public class DoctorFirstVisit
    {
        public VisitDetails VisitDetails { get; set; } = new VisitDetails();
        public List<TabDetail> TabDetaillist { get; set; } = new List<TabDetail>();
        public List<DiagnosisModel> ProvisionalDiagnosisList { get; set; } = new List<DiagnosisModel>();
        public List<DiagnosisModel> FinalDiagnosisList { get; set; } = new List<DiagnosisModel>();
        public List<Doctor> DoctorList { get; set; } = new List<Doctor>();
        public List<IDNamePair> VisitTypeList { get; set; } = new List<IDNamePair>();
        public List<VitalDetail> VitalDetails { get; set; } = new List<VitalDetail>();
        public List<IDNamePair> DietCategoryList { get; set; } = new List<IDNamePair>();
        public List<IDNamePair> RegTypeList { get; set; } = new List<IDNamePair>();
        public VisitChargeDetail VisitChargeDetail { get; set; } = new VisitChargeDetail();
        public List<InvestigationTestReport> PathoTestList { get; set; } = new List<InvestigationTestReport>();
        public List<InvestigationTestReport> RadioTestList { get; set; } = new List<InvestigationTestReport>();
        public List<InvestigationTestReport> ProcedureTestList { get; set; } = new List<InvestigationTestReport>();
    }
    public class VisitDetails
    {
        public int DocVisitIDP { get; set; }
        public string VisitCode { get; set; } = string.Empty;
        public byte VisitType { get; set; }
        public string VisitDateTime { get; set; } = string.Empty;
        public int? AdmissionIDF { get; set; }
        public int? DoctorIDF { get; set; }
        public int? PatientIDF { get; set; }
        public int? BedIDF { get; set; }
        public int? WardIDF { get; set; }
        public int? WardTypeIDF { get; set; }
        public int? OtherWardIDF { get; set; }
        public int? OtherWardTypeIDF { get; set; }
        public string Complaints { get; set; } = string.Empty;
        public string FindingAndSuggestions { get; set; } = string.Empty;
        public string DietOrOtherInstruction { get; set; } = string.Empty;
        public byte ChargeType { get; set; }
        public decimal VisitCharge { get; set; }
        public int? DietCategoryIDF { get; set; }
        public int? DietFoodIDF { get; set; }
        public string Advice { get; set; } = string.Empty;
        public byte IncludedInPackage { get; set; }
        public bool Urgent { get; set; }
        public int? CheckedByIDF { get; set; }
        public int UserIDF { get; set; }
        public int HospitalIDF { get; set; }
        public int HospitalGroupIDF { get; set; }
        public DateTime EntryDateTime { get; set; }
        public string Planofcare { get; set; } = string.Empty;
        public int? ReferDocIDF { get; set; }
        public string ReferDocVisitDate { get; set; } = string.Empty;
        public string ReferDocArrivalDatetime { get; set; } = string.Empty;
        public string ReferDocRemarks { get; set; } = string.Empty;
        public int? BedTrackingIDF { get; set; }
        public string IPAddress { get; set; } = string.Empty;
        public string BrowserName { get; set; } = string.Empty;
        public int? EditUserIDF { get; set; }
        public string EditEntryDateTime { get; set; } = string.Empty;
        public string EditIPAddress { get; set; } = string.Empty;
        public string EditBrowserName { get; set; } = string.Empty;
        public bool IsBackDatedIPDDrVisit { get; set; }
        public int? PainMeasurementScaleLevel { get; set; }

        // Admission Detail 
        public DateTime AdmissionDateTime { get; set; }
        public string IPDRegistrationCode { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string ConDoc { get; set; } = string.Empty;
        public string DocName { get; set; } = string.Empty;
        public string CRNumber { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public int DiscLedgerIDF { get; set; }
        public int ClassIDF { get; set; }
        public int NonCashLess { get; set; }
        public bool ClassForReimbursement { get; set; }
        
        public int IPDForNew { get; set; }
        public string BedName { get; set; } = string.Empty;
        public string WardName { get; set; } = string.Empty;
        public string OtherWardName { get; set; } = string.Empty;
        public string FloorName { get; set; } = string.Empty;
        public string BlockName { get; set; } = string.Empty;
        public int? SkillSetIDF { get; set; }
        public string PathoRemarks { get; set; } = string.Empty;
        public string RadioRemarks { get; set; } = string.Empty;
        public string ProcRemarks { get; set; } = string.Empty;
    }
    public class TabDetail
    {
        public int EMRCategoryIDP { get; set; }
        public string EMRCategoryName { get; set; } = string.Empty;
        public int? SubCategoryIDP { get; set; } // PascalCase
        public int? FirstVisitDetailsIDP { get; set; } // PascalCase
        public int? DocVisitIDF { get; set; }
        public string Answer { get; set; } = string.Empty;
        public int? ReferenceType { get; set; } // Fixed spacing
        public string Label { get; set; } = string.Empty;
        public List<string> ItemValue { get; set; } = new List<string>();
    }
    public class DiagnosisModel
    {
        public int DiagnosisType { get; set; }
        public string DiagnosisName { get; set; } = string.Empty;
    }
    public class Doctor
    {
        public int DoctorId { get; set; }
        public string? DoctorName { get; set; }
        public int? SkillsetId { get; set; }
        public string? SkillsetName { get; set; }
    }
    public class IDNamePair
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class VitalDetail
    {
        public int VitalRecordIDP { get; set; }
        public string VitalRecordName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;

        public bool IsNumeric { get; set; }
        public bool IsWeight { get; set; }
        public bool IsHeight { get; set; }
        public bool IsBMI { get; set; }
        public bool IsIBW { get; set; }
        public bool IsEBW { get; set; }
    }
    public class VisitChargeDetail
    {
        public int ServiceIDF { get; set; } = new int();
        public decimal OriginalAmt { get; set; } = new decimal();
        public decimal CostAddition { get; set; } = new decimal();
        public decimal DicountPer { get; set; } = new decimal();
        public decimal DiscountAmount { get; set; } = new decimal();
        public decimal VisitAmount { get; set; } = new decimal();
    }
    public class InvestigationTestReport
    {
        public int TestRegistrationIDP { get; set; }
        public int TestReportIDP { get; set; }
        public string TestReportName { get; set; } = string.Empty;
        public int ServiceIDF { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public int LabFlag { get; set; }
        public int? LabIDF { get; set; }
        public int PathoGroupDetailIDP { get; set; }
        public int CategoryIDF { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public int NotApplicable { get; set; }
        public bool IsPortable { get; set; }
        public bool CashFlag { get; set; }
      

        public string Container { get; set; } = string.Empty;
        public string TestInstruction { get; set; } = string.Empty;
        public string ReportTests { get; set; } = string.Empty;
        public int ReportInstruction { get; set; }

        public int Qty { get; set; }
        public decimal OriginalAmt { get; set; }
        public decimal CostAddRate { get; set; }
        public decimal DiscountPer { get; set; }
        public decimal DiscountAmt { get; set; }
        public decimal RoundingAmt { get; set; }
        public decimal Amount { get; set; }

        public decimal PortableOriginalAmt { get; set; }
        public decimal PortableDiscountAmt { get; set; }
        public decimal PortableRoundingAmt { get; set; }
        public decimal PortableAmount { get; set; }

        public int IsSelected { get; set; }
        public int Paid { get; set; }
        public int Status { get; set; }
        public int IsSampleCollection { get; set; }
        public bool IsReject { get; set; }
    }
   
    //Use Only Request Parameter
    public class VisitDetailsRequest
    {
        public int AdmissionIDF { get; set; }
        public int DocVisitIDF { get; set; }
        public int DoctorIDF { get; set; }
    }
    public class VisitChargeRequest
    {
        public int VisitTypeIDF { get; set; }
        public int ClassIDF { get; set; }
        public int NonCashLess { get; set; }
        public string Visitdate { get; set; }  = string.Empty;
    }
    public class TestPriceRequest
    {
        public int AdmissionIDF { get; set; }
        public int ChargeType { get; set; }
        public string Visitdate { get; set; } = string.Empty;
        public int CategoryIDF { get; set; }
        public int ClassIDF { get; set; }
        public int SkillSetIDF { get; set; }
        public int WardTypeIDF { get; set; }
        public int BedTrackingIDF { get; set; }
        public string SearchTest { get; set; } = string.Empty;
        public int VistType { get; set; }
        public int NonCashLess { get; set; }
        public int IPDForNew { get; set; }
    }
    public class VisitTestRequest
    {
        public int AdmissionIDF { get; set; }
        public int VisitIDF { get; set; }
        public int VisitFlag { get; set; }
    }
}
