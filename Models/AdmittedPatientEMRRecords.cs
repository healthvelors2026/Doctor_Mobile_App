namespace DoctorMobileApp.Models
{
    public class AdmittedPatientEMRRecords
    {
        public List<VitalList> lstVital { get; set; } = new List<VitalList>();
    }
    public class VitalList
    {
        public int VitalRecordIDP { get; set; }
        public string VitalRecordName { get; set; } = string.Empty;
        public int SRNo { get; set; }
        public string Value { get; set; } = string.Empty;
        public int VisitID { get; set; }
        public string VisitDateTime { get; set; } = string.Empty;
    }
    public class AdmittedPatientPathoRadioProcedureRecords
    {
        public List<PathoRadioProcedureList> lstPathoRadioProcedure { get; set; } = new();
    }
    public class PathoRadioProcedureList
    { // Common Visit Details
        public int? DocVisitIDP { get; set; }
        public string VisitCode { get; set; }
        public DateTime? VisitDateTime { get; set; }

        // Patient & Doctor Details
        public int? PatientIDF { get; set; }
        public int? DoctorIDF { get; set; }
        public int? EmployeeIDP { get; set; }
        public string DoctorName { get; set; }

        // Test / Procedure Details
        public string Test { get; set; }
        public DateTime? TestDate { get; set; }

        // Pathology Fields
        public int? Collected { get; set; }
        public int? IsSampleAcknowledged { get; set; }
        public string HospitalCode { get; set; }
        public int? PathoTestReportIDP { get; set; }

        // Status Details
        public int? Status { get; set; }
        public string ReportStatus { get; set; }

        // Payment
        public int? Paid { get; set; }

        // Category / Portable
        public string CategoryIDF { get; set; }
        public string RadioCategoryName { get; set; }
        public string ProcedureCategoryName { get; set; }
        public string IsPortable { get; set; }

        // Registration Details
        public int? AdmissionRegIDP { get; set; }
        public int? RegistrationIDP { get; set; }

        // Report Paths
        public string ReportPath { get; set; }
        public string ExternalReportPath { get; set; }

        // Refund
        public string RefundRemarks { get; set; }

        // Procedure Specific
        public int? ProcCnt { get; set; }
        public int? MedicalProcRegDetailIDP { get; set; }
        public int? RegIDP { get; set; }

        public string EmpFName { get; set; }
        public string EmpMName { get; set; }
        public string EmpLName { get; set; }

        // Flag
        public string Flag { get; set; }
    }
    public class ValueFeedPathoTestReportRecords
    {
        public List<FeedPathoTestReportList> lstFeedPathoTestReport { get; set; } = new();
    }
    public class FeedPathoTestReportList
    {
        public string? PathoTestReportName  { get; set; }
        public string? PathoTestCategoryName { get; set; }
        public int? PathoTestMasterIDP { get; set; }
        public string? PathoTestName { get; set; }
        public int? PathoTestUnitIDP { get; set; }
        public string? PathoTestUnitName { get; set; }
        public float? NumericTestValue { get; set; }
        public string? AlphaNumericTestValue { get; set; }
        public bool? IsMultiColumn { get; set; }
        public string? MultiColumnTestValue { get; set; }
        public int? FieldType {get; set; }
        public int? SrNo { get; set; }
        public int? PathoTestReportIDP { get; set; }
        public int? CategoryDetailSrNo { get; set; }
        public int? PathoTestCategoryIDP { get; set; }
        public int? PathoFacultyIDF { get; set; }
        public string? NormalRange  { get; set; }
        public bool IsAbnormalResult { get; set; }
    }
}
