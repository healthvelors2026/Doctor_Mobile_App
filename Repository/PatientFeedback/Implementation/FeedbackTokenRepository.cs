using Microsoft.EntityFrameworkCore;
using DoctorMobileApp.Models.PatientFeedback;
using DoctorMobileApp.Models;
using DoctorMobileApp.Repository.PatientFeedback.Interface;


namespace DoctorMobileApp.Repository.PatientFeedback.Implementation
{
    // Repository class responsible for all database operations
    // related to Feedback Token table (tbFeedbackToken)
    public class FeedbackTokenRepository : IFeedbackTokenRepository
    {
        // Database context used to communicate with SQL Server
        private readonly AppDbContext _context;

        // Constructor injection of AppDbContext
        // Dependency Injection will provide database context instance
        public FeedbackTokenRepository(AppDbContext context)
        {
            _context = context;
        }


        #region Save Token

        // Save newly generated feedback token into database
        public async Task<tbFeedbackToken> SaveAsync(tbFeedbackToken token)
        {
            // Add token entity into EF Core tracking
            _context.tbFeedbackTokens.Add(token);
            // Execute INSERT query into tbFeedbackToken table
            await _context.SaveChangesAsync();
            // Return saved entity with generated ID
            return token;
        }

        #endregion



        #region Get Token By Token Value

        // Used when patient opens feedback URL:
        //
        // Example:
        // https://hospital.com/Home/OpdFeedback?token=9PYALQ8S9F
        //
        // This method validates whether token is still usable

        public async Task<tbFeedbackToken?> GetByTokenAsync(string token)
        {
            return await _context.tbFeedbackTokens

                // Do not track entity because we only read data
                // Improves performance
                .AsNoTracking()

                // Find matching valid token
                .FirstOrDefaultAsync(x =>

                    // Token value should match
                    x.Token == token &&
                    // Token should be active
                    x.IsActive &&
                    // Feedback should not already be submitted
                    !x.IsUsed &&
                    // Token should not be expired
                    x.ExpiryDate >= DateTime.Now
                );
        }

        #endregion

        #region Check Existing Active Token

        // Checks whether patient already has a valid feedback token
        // Prevents creating duplicate tokens for the same visit

        public async Task<tbFeedbackToken?> GetActiveTokenAsync(
            int patientId,
            int registrationId,
            byte registrationType,
            int hospitalId)
        {
            try
            {
                return await _context.tbFeedbackTokens

                    // Only reading data
                    .AsNoTracking()

                    .FirstOrDefaultAsync(x =>

                        // Same patient
                        x.PatientIDF == patientId &&

                        // Same OPD/IPD registration
                        x.RegistrationIDF == registrationId &&

                        // Same visit type
                        // 0 = OPD
                        // 1 = IPD
                        x.RegistrationType == registrationType &&


                        // Same hospital
                        x.HospitalIDF == hospitalId &&

                        // Token must be active
                        x.IsActive &&

                        // Feedback not submitted yet
                        !x.IsUsed &&

                        // Token still valid
                        x.ExpiryDate >= DateTime.Now
                    );
            }
            catch (Exception ex)
            {
                // Log exception details
                Console.WriteLine(ex.ToString());

                // Send exception back to service layer
                throw;
            }
        }

        #endregion




        #region Mark Token Used

        // Called after successful feedback submission
      

        public async Task<bool> MarkAsUsedAsync(string token)
        {
            var entity = await _context.tbFeedbackTokens
                .FirstOrDefaultAsync(x => x.Token == token);
            if (entity == null)
                return false;
            // Feedback submitted
            entity.IsUsed = true;
            // Disable feedback link
            entity.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion
    }
}