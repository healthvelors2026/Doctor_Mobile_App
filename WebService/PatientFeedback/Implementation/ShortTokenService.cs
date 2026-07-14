using System.Security.Cryptography;
using DoctorMobileApp.WebService.PatientFeedback.Interface;

namespace DoctorMobileApp.WebService.PatientFeedback.Implementation
{
    // Service to generate secure short feedback tokens
    public class ShortTokenService : IShortTokenService
    {
        // Allowed characters for token generation
        private const string Characters =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";


        // Generate random token
        // Default length = 10 characters
        public string Generate(int length = 10)
        {
            // Create random byte array
            var bytes = new byte[length];
            // Secure random generator
            using var rng = RandomNumberGenerator.Create();
            // Fill byte array with random values
            rng.GetBytes(bytes);
            // Store generated characters
            char[] result = new char[length];
            // Convert random bytes into token characters
            for (int i = 0; i < length; i++)
            {
                result[i] = Characters[
                    bytes[i] % Characters.Length];
            }

            // Return final token string
            return new string(result);
        }
    }
}