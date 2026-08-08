using System.ComponentModel.DataAnnotations;

namespace DoctorMobileApp.DTOs.PatientFeedbackDto
{
    // Request to generate feedback link
    public class GenerateFeedbackTokenRequestDto
    {
        public int PatientId { get; set; }

        public int RegistrationId { get; set; }

        // 0 = OPD
        // 1 = IPD
        public byte opdIpdFlag{ get; set; }

        public int UserIdF { get; set; }
    }

    // Response after token generation
    public class GenerateFeedbackTokenResponseDto
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public string Token { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;
    }

    // Database token DTO
    public class FeedbackTokenDto
    {
        public long FeedbackTokenID { get; set; }

        public string Token { get; set; } = string.Empty;

        public int PatientIDF { get; set; }

        public int RegistrationIDF { get; set; }

        // 0 = OPD
        // 1 = IPD
        public byte RegistrationType { get; set; }

        public int HospitalIDF { get; set; }

        public int UserIdF { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime ExpiryDate { get; set; }

        public bool IsUsed { get; set; }

        public bool IsActive { get; set; }
    }

    // Token resolve result
    public class ResolveFeedbackTokenDto
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public int PatientId { get; set; }

        public int RegistrationId { get; set; }

        public byte opdIpdFlag{ get; set; }

        public int UserIdF { get; set; }
    }
}