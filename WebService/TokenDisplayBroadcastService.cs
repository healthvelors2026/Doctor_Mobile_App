// Triggers the existing TokenDisplayed SignalR broadcast (old StellaWeb project) after a successful token-display insert.
using System.Net.Http.Json; // For JsonContent.Create when building the bridge request body

namespace DoctorMobileApp.WebService
{
    // Contract for broadcasting a newly inserted OPD-entry token to the old TokenDisplay SignalR clients
    public interface ITokenDisplayBroadcastService
    {
        // Sends the token details to the bridge so the old Hub can broadcast them
        Task BroadcastOPDEntryTokenAsync(int roomIdf, int tokenIdf, int doctorIdf, CancellationToken cancellationToken = default);
    }

    // Shared broadcast trigger - posts to a bridge endpoint in the OLD StellaWeb project since its SignalR 1.2.2 Hub (.NET Framework 4.0) can't be referenced directly from this .NET 8 project.
    public class TokenDisplayBroadcastService : ITokenDisplayBroadcastService
    {
        private readonly HttpClient _httpClient; // Used to call the OLD project's bridge endpoint
        private readonly IConfiguration _configuration; // Reads the bridge URL and API key from appsettings
        private readonly ILogger<TokenDisplayBroadcastService> _logger; // Logs skips/failures without throwing

        // Injected via DI (AddHttpClient<ITokenDisplayBroadcastService, TokenDisplayBroadcastService>() in Program.cs)
        public TokenDisplayBroadcastService(HttpClient httpClient, IConfiguration configuration, ILogger<TokenDisplayBroadcastService> logger)
        {
            _httpClient = httpClient; // Store the typed HttpClient
            _configuration = configuration; // Store configuration for later lookups
            _logger = logger; // Store logger for warnings/errors
        }

        // Called by OPDRegistrationService only when API_Sp_InsertTokenDisplay reports IsInserted = 1
        public async Task BroadcastOPDEntryTokenAsync(int roomIdf, int tokenIdf, int doctorIdf, CancellationToken cancellationToken = default)
        {
            var bridgeUrl = _configuration["TokenDisplayBridge:Url"]; // Base URL of the OLD project's bridge endpoint
            var apiKey = _configuration["TokenDisplayBridge:ApiKey"]; // Shared secret for the bridge endpoint

            // No bridge configured yet - skip quietly instead of failing the caller
            if (string.IsNullOrWhiteSpace(bridgeUrl))
            {
                _logger.LogWarning("TokenDisplayBridge:Url not configured - broadcast skipped for TokenIDF={TokenIDF}", tokenIdf); // Record that nothing was sent
                return; // Nothing more to do without a URL
            }

            // Shape matches what the OLD Hub's BroadcastData(TokenDisplay) reads for the EnumOPDEntry branch
            var payload = new
            {
                TokenDisplayType = 1, // EnumOPDEntry - matches API_Sp_InsertTokenDisplay's hardcoded value
                ConsultingRoomID = roomIdf, // Maps to RoomIDF
                TokenIssueIDP = tokenIdf, // Maps to TokenIDF
                DocIDFOPDEntry = doctorIdf, // Maps to DoctorIDF
                IsInsideOPDEntry = true // Tells the old Hub to resolve ConsultingRoomNo from ConsultingRoomID
            };

            // Build the outgoing POST request to the bridge endpoint
            using var request = new HttpRequestMessage(HttpMethod.Post, bridgeUrl)
            {
                Content = JsonContent.Create(payload) // Serialize the payload as JSON
            };
            // Attach the shared-secret header only when one is configured
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                request.Headers.Add("X-Bridge-ApiKey", apiKey); // Bridge endpoint validates this header
            }

            try
            {
                var response = await _httpClient.SendAsync(request, cancellationToken); // Send the request
                // Log a warning if the bridge rejected or failed the call
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("TokenDisplay broadcast bridge returned {StatusCode} for TokenIDF={TokenIDF}", response.StatusCode, tokenIdf); // Non-2xx response
                }
            }
            catch (Exception ex)
            {
                // The DB insert already succeeded - a broadcast failure must not fail the API response.
                _logger.LogError(ex, "Failed to reach TokenDisplay broadcast bridge for TokenIDF={TokenIDF}", tokenIdf); // Network/timeout/etc.
            }
        }
    }
}
