using DoctorMobileApp.DTOs.PatientFeedbackDto;

namespace DoctorMobileApp.WebService.PatientFeedback.Interface
{
    public interface IFeedbackTokenService
    {
        Task<FeedbackTokenDto> GenerateTokenAsync(
            int patientId,
            int registrationId,
            byte registrationType,
            int hospitalId);

        Task<FeedbackTokenDto?> ResolveTokenAsync(string token);
        Task<bool> MarkTokenUsedAsync(string token);
    }
}
