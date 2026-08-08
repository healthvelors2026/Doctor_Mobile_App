using DoctorMobileApp.DTOs.PatientFeedbackDto;
using DoctorMobileApp.Models.PatientFeedback;
using DoctorMobileApp.Repository.PatientFeedback.Interface;
using DoctorMobileApp.WebService.PatientFeedback.Interface;
using Microsoft.Extensions.Options;

namespace DoctorMobileApp.WebService.PatientFeedback.Implementation
{
    public class FeedbackTokenService : IFeedbackTokenService
    {
        private readonly IFeedbackTokenRepository _repository;
        private readonly FeedbackTokenSettings _settings;
        private readonly IShortTokenService _shortTokenService;

        public FeedbackTokenService(
      IFeedbackTokenRepository repository,
      IShortTokenService shortTokenService,
      IOptions<FeedbackTokenSettings> options)
        {
            _repository = repository;
            _shortTokenService = shortTokenService;
            _settings = options.Value;
        }

        public async Task<FeedbackTokenDto> GenerateTokenAsync(
            int patientId,
            int registrationId,
            byte registrationType,
            int hospitalId,
            int userIdF)
        {
            // Check if active token already exists
            var existing =
                await _repository.GetActiveTokenAsync(
                    patientId,
                    registrationId,
                    registrationType,
                    hospitalId);

            if (existing != null)
            {
                return Map(existing);
            }

            string token;

            // Generate unique token
            do
            {
                token = _shortTokenService.Generate();
            }
            while (await _repository.GetByTokenAsync(token) != null);

            var entity = new tbFeedbackToken
            {
                Token = token,
                PatientIDF = patientId,
                RegistrationIDF = registrationId,
                RegistrationType = registrationType,
                HospitalIDF = hospitalId,
                GeneratedByUserIDF = userIdF,
                CreatedDate = DateTime.Now,
                ExpiryDate = DateTime.Now.AddHours(_settings.TokenExpiryHours),
                IsUsed = false,
                IsActive = true
            };
            var saved = await _repository.SaveAsync(entity);
            return Map(saved);
        }

        public async Task<FeedbackTokenDto?> ResolveTokenAsync(string token)
        {
            var entity = await _repository.GetByTokenAsync(token);
            if (entity == null)
                return null;
            return Map(entity);
        }

        public async Task<bool> MarkTokenUsedAsync(string token)
        {
            return await _repository.MarkAsUsedAsync(token);
        }

        private static FeedbackTokenDto Map(tbFeedbackToken entity)
        {
            return new FeedbackTokenDto
            {
                FeedbackTokenID = entity.FeedbackTokenID,
                Token = entity.Token,
                PatientIDF = entity.PatientIDF,
                RegistrationIDF = entity.RegistrationIDF,
                RegistrationType = entity.RegistrationType,
                HospitalIDF = entity.HospitalIDF,
                UserIdF = entity.GeneratedByUserIDF,
                CreatedDate = entity.CreatedDate,
                ExpiryDate = entity.ExpiryDate,
                IsUsed = entity.IsUsed,
                IsActive = entity.IsActive
            };
        }
    }
}