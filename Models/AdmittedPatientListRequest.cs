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

    public class AddmisionCheckRequest
    {
        public int CheckListType { get; set; }
        public bool NonActive { get; set; } = false;
        public int AdmissionIDF { get; set; }
        public int RegistrationIDF { get; set; }
        public int RegistrationType { get; set; }
    }

    public class AddmisionCheckResponse
    {
        public int CheckListQuestionIDP { get; set; }
        public string? Question { get; set; }
        public int SrNo { get; set; }
        public int Type { get; set; }
        public int CheckListQuestionDetailIDP { get; set; }
        public bool Value { get; set; }
        public string? Remarks { get; set; }
        public int CategoryIDF { get; set; }
        public string? CategoryName { get; set; }

    }
    public class BedTransferRequest
    {
        public int PatientID { get; set; }
        //public string? Filter { get; set; } = string.Empty;
        //public int PageNumber { get; set; } = 1;
        //public int PageSize { get; set; } = 20;
    }
    public class BedTransferResponse
    {
        public int AdmissionID { get; set; }
        public string PatientName { get; set; }
        public string AdmissionDate { get; set; }
        public string IPDRegistrationCode { get; set; }
        public string Age { get; set; }
        public string Gender { get; set; }
        public string ClassName { get; set; }
        public string ConsultDoctor { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public List<BedTransferHistoryModel> BedTransferHistory { get; set; } = new List<BedTransferHistoryModel>();
    }
    public class BedTransferHistoryModel
    {
        public int TransferType { get; set; }

        public string? BedName { get; set; }

        public string? WardName { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public string? UserName { get; set; }

        public string? ReBedname { get; set; }

        public bool? CurrentBed { get; set; }

        public string Mode { get; set; } = string.Empty;

        public string TransferBy { get; set; } = string.Empty;
    }
    public class BedTransferPatientDBModel
    {
        public int IPDAdmissionDischargeIDP { get; set; }

        public int PatientIDP { get; set; }

        public string? IPDRegistrationCode { get; set; }

        public DateTime AdmissionDateTime { get; set; }

        public DateTime DateOfBirth { get; set; }

        public int Gender { get; set; }

        public string? FName { get; set; }

        public string? MName { get; set; }

        public string? LName { get; set; }

        public bool NonCashLess { get; set; }

        public int EmployeeIDP { get; set; }

        public string? EmpFName { get; set; }

        public string? EmpMName { get; set; }

        public string? EmpLName { get; set; }

        public int ClassIDP { get; set; }

        public string? ClassName { get; set; }

        public int BedIDP { get; set; }

        public string? BedName { get; set; }

        public int WardIDP { get; set; }

        public string? WardName { get; set; }

        public int WardTypeIDF { get; set; }

        public int IPDBedAmenityTrackingIDP { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public int UserIDP { get; set; }

        public string? UserName { get; set; }

        public int OtherWardIDF { get; set; }

        public bool IsDayCare { get; set; }

        public int PrimaryDocIDF { get; set; }
    }
    public class BedTransferEditRequest
    {
        public int PatientIDF { get; set; }
    }
    public class BedTransferEditResponse
    {
        public int AdmissionID { get; set; }

        public int CurrentTrackingID { get; set; }

        public int CurrentBedID { get; set; }

        public string CurrentBed { get; set; } = string.Empty;

        public int CurrentWardID { get; set; }

        public string CurrentWard { get; set; } = string.Empty;

        public int CurrentWardTypeID { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime TransferDate { get; set; }

        public bool IsDayCare { get; set; }

        public List<BedDropdownModel> AvailableBeds { get; set; } = new();

        public List<CheckListQuestionModel> AdmissionChecklist { get; set; } = new();

        public List<SwapPatientModel> SwapPatients { get; set; } = new();
    }
    public class BedDropdownModel
    {
        public int BedID { get; set; }

        public string BedName { get; set; }

        public int WardID { get; set; }

        public string WardName { get; set; }
    }
    public class CheckListQuestionModel
    {
        public int CheckListQuestionIDP { get; set; }

        public string Question { get; set; }

        public bool Value { get; set; }

        public string Remarks { get; set; }

        public string CategoryName { get; set; }
    }
    public class SwapPatientModel
    {
        public int AdmissionID { get; set; }

        public int PatientIDF { get; set; }

        public string PatientName { get; set; } = string.Empty;

        public int TrackingID { get; set; }

        public int BedID { get; set; }

        public string BedName { get; set; } = string.Empty;

        public int WardID { get; set; }

        public string WardName { get; set; } = string.Empty;

        public bool IsICUWard { get; set; }

        public DateTime? FromDate { get; set; }
    }
    public class AvailableBedDBModel
    {
        public int BedIDP { get; set; }

        public string BedName { get; set; }

        public string BedCode { get; set; }

        public int WardIDP { get; set; }

        public string WardName { get; set; }

        public int WardTypeIDF { get; set; }

        public bool Status { get; set; }

        public bool IsICUWard { get; set; }

        public bool IsOTWard { get; set; }
    }

    // For Swap 
    public class SwapPatientRequest
    {
        public int PatientID { get; set; }
    }
    public class SwapPatientResponse
    {
        public int PatientID { get; set; }

        public int AdmissionID { get; set; }

        public string PatientName { get; set; } = string.Empty;

        public string IPDRegistrationCode { get; set; } = string.Empty;

        public int CurrentTrackingID { get; set; }

        public int CurrentBedID { get; set; }

        public string CurrentBed { get; set; } = string.Empty;

        public int CurrentWardID { get; set; }

        public string CurrentWard { get; set; } = string.Empty;

        public DateTime CurrentFromDate { get; set; }

        public bool IsDayCare { get; set; }

        public List<SwapPatientModel> SwapPatients { get; set; } = new();
    }
    public class SaveBedTransferRequest
    {
        public int AdmissionID { get; set; }

        public int CurrentBedID { get; set; }

        public int ToBedID { get; set; }

        public DateTime TransferDate { get; set; }

        public string? Remarks { get; set; }

        public bool IsIcuBed { get; set; }

        public int? ICUChargeType { get; set; }
    }

    public class SaveBedTransferResponse
    {
        public bool Success { get; set; }

        public int ResultCode { get; set; }

        public string Message { get; set; } = string.Empty;

        public int TrackingID { get; set; }
    }
    public class RequestBedTransferResponse
    {
        public bool Success { get; set; }

        public int ResultCode { get; set; }

        public string Message { get; set; } = string.Empty;

        public int BedStatusTrackingID { get; set; }

        public int AdmissionID { get; set; }

        public int FromBedID { get; set; }

        public int ToBedID { get; set; }
    }
    public class RequestBedTransferDBModel
    {
        public bool Success { get; set; }

        public int ResultCode { get; set; }

        public string Message { get; set; } = string.Empty;

        public int BedStatusTrackingID { get; set; }

        public int AdmissionID { get; set; }

        public int FromBedID { get; set; }

        public int ToBedID { get; set; }
    }
    public class SaveBedTransferDBModel
    {
        public bool Success { get; set; }

        public int ResultCode { get; set; }

        public string Message { get; set; } = string.Empty;

        public int TrackingID { get; set; }
    }
}

//public class RequestBedTransferRequest
//{
//    public int AdmissionID { get; set; }

//    public int CurrentTrackingID { get; set; }

//    public int CurrentBedID { get; set; }

//    public int ToBedID { get; set; }

//    public DateTime TransferDate { get; set; }

//    public string? Remarks { get; set; }

//    public List<int> AmenityIDs { get; set; } = new();
//}
