using DoctorMobileApp.DTOs.PatientFeedbackDto;
using System.Text.Json.Serialization;

namespace DoctorMobileApp.DTOs.PatientFeedback
{
    public class PatientFeedbackDto
    {
        [JsonPropertyName("crNumber")]
        public string? CRNumber { get; set; }

        // OPD Registration Code
        [JsonPropertyName("registrationCode")]
        public string? RegistrationCode { get; set; }


        // IPD Registration Code
        [JsonPropertyName("ipdRegistrationCode")]
        public string? IPDRegistrationCode { get; set; }


        // OPD Registration Date
        [JsonPropertyName("registrationDate")]
        public DateTime? RegistrationDate { get; set; }


        // IPD Admission Date
        [JsonPropertyName("admissionDateTime")]
        public DateTime? AdmissionDateTime { get; set; }


        // IPD Discharge Date
        [JsonPropertyName("dischargeDate")]
        public DateTime? DischargeDate { get; set; }


        [JsonPropertyName("doctorName")]
        public string? DoctorName { get; set; }


        [JsonPropertyName("doctorIDF")]
        public int? DoctorIDF { get; set; }


        [JsonPropertyName("className")]
        public string? ClassName { get; set; }


        [JsonPropertyName("remarks")]
        public string? Remarks { get; set; }


        // 0 = OPD
        // 1 = IPD
        [JsonPropertyName("registrationType")]
        public byte RegistrationType { get; set; }


        [JsonPropertyName("feedbackForm")]
        public FeedbackFormDto FeedbackForm { get; set; } = new();


        [JsonPropertyName("feedbackDetails")]
        public List<FeedBackPatientDetailDto> FeedbackDetails { get; set; } = new();
    }
}