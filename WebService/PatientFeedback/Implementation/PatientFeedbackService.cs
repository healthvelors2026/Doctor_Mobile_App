using DoctorMobileApp.DTOs.PatientFeedback;
using DoctorMobileApp.DTOs.PatientFeedbackDto;
using DoctorMobileApp.Repository.PatientFeedback.Interface;
using DoctorMobileApp.WebService.patientFeedback.Interface;

namespace MobieAppPatientFeedback.Services
{
    public class PatientFeedbackService : IPatientFeedbackService
    {
        private readonly IPatientFeedbackRepository _repository;

        public PatientFeedbackService(
            IPatientFeedbackRepository repository)
        {
            _repository = repository;
        }

        public async Task<PatientFeedbackDto?> GetFeedbackAsync(
            int patientId,
            int registrationId,
            byte opdIpdFlag,
            int userIdF)
        {
            return await _repository.GetPatientFeedbackAsync(
                patientId,
                registrationId,
                opdIpdFlag,
                userIdF);
        }

        public async Task<int> SaveFeedbackAsync(
            SaveFeedbackRequestDto request)
        {
            return await _repository.SaveFeedbackAsync(request);
        }
    }
}