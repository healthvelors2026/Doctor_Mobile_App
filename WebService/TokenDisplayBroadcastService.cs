// Posts to a bridge endpoint in the OLD StellaWeb project so its existing TokenDisplayHub can broadcast - no compatible SignalR client exists for calling it directly from this .NET 8 project.
using System.Net.Http.Json;

namespace DoctorMobileApp.WebService
{
    public interface ITokenDisplayBroadcastService
    {
        // Broadcasts a successfully inserted OPD-entry token to the old TokenDisplay screen
        Task BroadcastOPDEntryTokenAsync(int roomIdf, int tokenIdf, int doctorIdf, bool isUpcomingToken, string? tcrNumber, CancellationToken cancellationToken = default);
    }

    public class TokenDisplayBroadcastService : ITokenDisplayBroadcastService
    {
        private const string BridgeApiKey = "tdbridge-9f3a7c1e-mobileapp";

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<TokenDisplayBroadcastService> _logger;

        // Injected via DI - HttpClient calls the bridge, IHttpContextAccessor derives its host
        public TokenDisplayBroadcastService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ILogger<TokenDisplayBroadcastService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        // Called by OPDRegistrationService only when API_Sp_InsertTokenDisplay reports IsInserted = 1
        public async Task BroadcastOPDEntryTokenAsync(int roomIdf, int tokenIdf, int doctorIdf, bool isUpcomingToken, string? tcrNumber, CancellationToken cancellationToken = default)
        {
            var bridgeUrl = ResolveBridgeUrl();
            if (bridgeUrl == null)
            {
                _logger.LogWarning("Could not resolve the TokenDisplay bridge URL - broadcast skipped for TokenIDF={TokenIDF}", tokenIdf);
                return;
            }
            _logger.LogInformation("TokenDisplay broadcast bridge URL resolved to {BridgeUrl} for TokenIDF={TokenIDF}", bridgeUrl, tokenIdf);
            var payload = new
            {
                TokenDisplayType = 1,
                ConsultingRoomID = roomIdf,
                TokenIssueIDP = tokenIdf,
                DocIDFOPDEntry = doctorIdf,
                IsInsideOPDEntry = true,
                IsUpcomingToken = isUpcomingToken,
                TCRNumber = tcrNumber // lets the old Hub resolve a real patient name; null when no registration data links to this token
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, bridgeUrl)
            {
                Content = JsonContent.Create(payload)
            };
            request.Headers.Add("X-Bridge-ApiKey", BridgeApiKey);
            _logger.LogInformation("TokenDisplay broadcast raw JSON body: {Json}", await request.Content!.ReadAsStringAsync(cancellationToken));

            try
            {
                var response = await _httpClient.SendAsync(request, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("TokenDisplay broadcast bridge returned {StatusCode} for TokenIDF={TokenIDF}", response.StatusCode, tokenIdf);
                }
                else
                {
                    _logger.LogInformation("TokenDisplay broadcast bridge call succeeded ({StatusCode}) for TokenIDF={TokenIDF}", response.StatusCode, tokenIdf);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reach TokenDisplay broadcast bridge for TokenIDF={TokenIDF}", tokenIdf);
            }
        }

        // Local-dev override wins if set, otherwise derives host+port-80 from the current request
        private string? ResolveBridgeUrl()
        {
            var configuredUrl = _configuration["TokenDisplayBridge:Url"];
            if (!string.IsNullOrWhiteSpace(configuredUrl))
            {
                return configuredUrl;
            }

            var currentRequest = _httpContextAccessor.HttpContext?.Request;
            if (currentRequest == null)
            {
                return null;
            }
            // Host.Value includes the port the caller actually connected on (e.g. "server:3765"),
            // not just the hostname - correct as long as this API is deployed as an IIS Application
            // under the same site/port as StellaWeb, whatever that port is.
            return $"{currentRequest.Scheme}://{currentRequest.Host.Value}/TokenDisplayBroadcast/Broadcast";
        }
    }
}
