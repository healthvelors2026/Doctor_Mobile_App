using DoctorMobileApp.DTOs.PatientFeedback;
using DoctorMobileApp.DTOs.PatientFeedbackDto;

namespace DoctorMobileApp.WebService.patientFeedback.Interface
{
    public interface IPatientFeedbackService
    {
        Task<PatientFeedbackDto?> GetFeedbackAsync(
            int patientId,
            int registrationId,
            byte opdIpdFlag,
            int userIdF);

        Task<int> SaveFeedbackAsync(
            SaveFeedbackRequestDto request);
    }
}