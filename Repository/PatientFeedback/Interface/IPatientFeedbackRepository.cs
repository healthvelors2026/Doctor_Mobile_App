using DoctorMobileApp.DTOs.PatientFeedback;
using DoctorMobileApp.DTOs.PatientFeedbackDto;

namespace DoctorMobileApp.Repository.PatientFeedback.Interface
{
    public interface IPatientFeedbackRepository
    {
        Task<PatientFeedbackDto?> GetPatientFeedbackAsync(
            int patientId,
            int registrationId,
            byte registrationType,
            int userIdF);

        Task<int> SaveFeedbackAsync(
            SaveFeedbackRequestDto request);
    }
}