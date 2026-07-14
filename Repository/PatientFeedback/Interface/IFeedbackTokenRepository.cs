
using DoctorMobileApp.Models.PatientFeedback;
namespace DoctorMobileApp.Repository.PatientFeedback.Interface
{
  
    public interface IFeedbackTokenRepository
    {
        Task<tbFeedbackToken> SaveAsync(tbFeedbackToken token);

        Task<tbFeedbackToken?> GetByTokenAsync(string token);

        Task<tbFeedbackToken?> GetActiveTokenAsync(
            int patientId,
            int registrationId,
            byte registrationType,
            int hospitalId);

        Task<bool> MarkAsUsedAsync(string token);
    }
}
