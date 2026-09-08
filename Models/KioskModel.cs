using System.ComponentModel.DataAnnotations;

namespace DoctorMobileApp.Models
{
    public class KioskModel
    {
        public class PatientSearchModel
        {
            public string? MobileNo { get; set; }
            public string? ABHANo { get; set; }
            public string? CRNo { get; set; }
        }
        public class PatientDetail
        {
            public int PatientID { get; set; }
            public string? PatientName { get; set; }
            public string? ClassName { get; set; }
            public string? MobileNo { get; set; }
            public string? ABHANo { get; set; }
            public string? CRNo { get; set; }
            public int Age { get; set; }
            public string? Gender { get; set; }
            public int HospitalId { get; set; }
            public bool IsAdmitted { get; set; }
            public int AdmissionIDF { get; set; }
            public string? IPDRegistrationCode { get; set; }
            public string? Doctor {  get; set; }
            public string? BedWard { get; set; }

        }
        public class SkillSetResponseModel
        {
            public int SkillSetIDF { get; set; }
            public string? SkillSetName { get; set; }
            public string? SkillSetLocalLanguage { get; set; }
            public int? MobileDoctorSkillIDP { get; set; }
            public string? IconPath { get; set; }
            public int? HospitalGroupIDF { get; set; }
        }
        public class GeneratePatientOTPRequestModel
        {
            public int PatientIDF { get; set; }
            public string? CRNumber { get; set; }
            public string? MobileNo { get; set; }
        }
        public class GeneratePatientOTPResponseModel
        {
            public long OTPIDP { get; set; }
            public string? CRNumber { get; set; }
            public string? MobileNo { get; set; }
            public string? Message { get; set; }
            public string? OTP { get; set; }
        }
        public class VerifyPatientOTPRequestModel
        {
            public long KioskPatientOTPIDP { get; set; }
            public string? CRNumber { get; set; }
            public string? KioskOTP { get; set; }
        }
        public class VerifyPatientOTPResponseModel
        {
            public bool Status { get; set; }
            public string? Message { get; set; }
        }
        // get healthcard active patient list
        public class HealthCardActivePatientRequestModel
        {
            public int PatientID { get; set; }
        }
        public class HealthCardActivePatientResponseModel
        {
            public int HealthCardPlanIDP { get; set; }
            public string? HealthCardPlanName { get; set; }
            public int HealthCardPatientIssueIDP { get; set; }
            public string? HealthCardNumber { get; set; }
            public int HealthCardPatientIssueDetailIDP { get; set; }
        }
        public class PathoReportRequestModel
        {
            public int PatientIDF { get; set; }
        }
        public class PathoReportResponseModel
        {
            public int PathoRegistrationIDP { get; set; }
            public string? ReportName { get; set; }
            public string? RegistrationCode { get; set; }
            public DateTime RegistrationDateTime { get; set; }
            public string? ReportStatusDisplay { get; set; }
            public bool ReportStatus { get; set; }
            public string? Doctor { get; set; }
        }
        public class OPDTestReceiptRequestModel
        {
            public int PatientIDF { get; set; }
        }
        public class OPDTestReceiptResponseModel
        {
            public int OPDRegistrationIDP { get; set; }
            public int InvestigationRegistrationIDP { get; set; }
            public int ReportMasterID { get; set; }
            public string? ReportName { get; set; }
            public int ServiceIDF { get; set; }
            public string? ServiceName { get; set; }
            public bool Paid { get; set; }
            public int PriceListIDP { get; set; }
            public decimal Rate { get; set; }
            /*
                1 = Pathology
                2 = Radiology
                3 = Medical Procedure
            */
            public int InvestigationType { get; set; }
            public string? Doctor { get; set; }
            public int NotApplicable { get; set; }
            public string? Instruction { get; set; }
            public int TATTime { get; set; }
            public string? TATType { get; set; }
            public string? RegistrationCode { get; set; }
        }
        public class SaveOPDTestReceiptRequestModel
        {
            public int PatientIDF { get; set; }
            public int OPDRegistrationIDF { get; set; }
            public string? UPITransactionNo { get; set; }
            public List<OPDTestReceiptDetailModel> OPDTestReceiptList { get; set; } = new List<OPDTestReceiptDetailModel>();
        }
        public class OPDTestReceiptDetailModel
        {
            public int InvestigationRegistrationIDP { get; set; }
            public int InvestigationType { get; set; }
            public decimal Rate { get; set; }
        }
        public class SaveOPDTestReceiptResponseModel
        {
            public int VoucherIDP { get; set; }
            public int VoucherIDP_NA { get; set; }
            public string? VoucherNumber { get; set; }
            public string? VoucherNumber_NA { get; set; }
            public SaveOPDRegistrationReceiptResponseModel? ReceiptDetail { get; set; }
            public SaveOPDRegistrationReceiptResponseModel? NotApplicableReceiptDetail { get; set; }
        }
        public class LastVisitDrRequestmodel
        {
            public int PatientIDF { get; set; }
        }
        public class LastVisitDrResponseModel
        {
            public int OPDRegistrationIDP { get; set; }    
            public int PatientIDF { get; set; }
            public int DoctorIDF {  get; set; }
            public string? DoctorName { get; set; }
            public string? SkillSetName { get; set; }
            public string? ServiceName { get; set; }
            public double TotalAmount { get; set; }
            public string? LastConsultation {  get; set; }
        }
        public class PatientLatestAppointmentRequestModel
        {
            public int PatientID { get; set; }
        }
        public class PatientLatestAppointmentResponseModel
        {
            public int EmployeeIDP { get; set; }
            public string? DoctorName { get; set; }
            public string? DepartmentName { get; set; }
            public string? ServiceName { get; set; }
            public DateTime TodayDate { get; set; }
            public string? Slot { get; set; }
            public string? Status { get; set; }
        }
        public class DoctorRequestModel
        {
            public int? SkillSetID { get; set; }
            public int? PatientID { get; set; }
        }
        public class DoctorResponseModel
        {
            public int EmployeeIDP { get; set; }
            public string? EmployeeName { get; set; }
            public string? SkillSetName { get; set; }
            public string? SkillSetLocalLanguage { get; set; }
            public string? TimeSlot { get; set; }
            public decimal Amount { get; set; }
            public string? Photo { get; set; }
        }

        public class AdvanceDepositModel
        {
            public int PatientIDF { get; set; }
            public decimal AdvanceAmount { get; set; }
            public string? TransactionId { get; set; }
            public string? BrowserName { get; set; }
            public string? IPAdress { get; set; }
        }
        public class SaveOPDRegistrationModel
        {
            public int PatientIDF { get; set; }
            public int DoctorIDF { get; set; }
            public string? UPITransactionNo { get; set; }
            public string? BrowserName { get; set; }
            public string? IPAdress { get; set; }
            public int HealthCardPatientIssueDetailIDP { get; set; }
        }
        public class SaveOPDRegistrationReceiptResponseModel
        {
            public int VoucherIDP { get; set; }
            public string? VoucherNumber { get; set; }
            public string? TransactionType { get; set; } 
            public string? AdvanceDepositSaveDateTime { get; set; }
            public string? OPDRegistrationSaveDateTime { get; set; }
            public string? OPDTestReceiptSaveDateTime { get; set; }
            public int OPDRegistrationIDP { get; set; }
            public string? RegistrationCode { get; set; }   
            public int TokenNumber { get; set; }
            public string? RoomNumber { get; set; }
        }
        // Add For Kiosk Banner 
        public class KioskBannerResponseModel
        {
            public int KioskBannerIDP { get; set; }
            public string? KioskBannerPath { get; set; }
            public string? BannerImageUrl { get; set; }
            public string? OriginalFileName { get; set; }
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
            public int DisplayOrder { get; set; }
            public int DisplaySeconds { get; set; }
            public string? Status { get; set; }
        }
        // Get from HIMS 
        public class KioskBannerImageUploadModel
        {
            public string? FileName { get; set; }
            public string? FileBase64 { get; set; }
            public string? ContentType { get; set; }
            public string? FolderName { get; set; }
            public string? ImageIDP { get; set; }
            public string? HospitalCode { get; set; }
        }
        public class SmsConfigurationModel
        {
            public string? URL { get; set; }
            public string? SMSText { get; set; }
        }
    }
}
