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
        }
        public class SaveOPDTestReceiptRequestModel
        {
            public int HospitalIDF { get; set; }
            public int PatientIDF { get; set; }
            public int OPDRegistrationIDF { get; set; }
            public int UserIDF { get; set; }
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
        }

        public class DoctorRequestModel
        {
            public int SkillSetIDF { get; set; }
        }
        public class DoctorResponseModel
        {
            public int DoctorID { get; set; }
            public string? DoctorName { get; set; }
        }

        public class AdvanceDepositModel
        {
            public int PatientIDF { get; set; }
            public int HospitalIDF { get; set; }
            public decimal AdvanceAmount { get; set; }
            public string? TransactionId { get; set; }
            public int ModeOfPaymentIDF { get; set; }
            public int Kiosk_UserIDF { get; set; }
        }
    }
}
