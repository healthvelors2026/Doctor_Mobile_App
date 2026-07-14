

using DoctorMobileApp.CommonClass.Services.Interface;

namespace MobieAppPatientFeedback.Common.Services.Implementation
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // Fixed system user (as you already designed)
        public int UserId => 4;

        // =========================
        // HOSPITAL ID (SAFE)
        // =========================
        public int HospitalId
        {
            get
            {
                var value = _httpContextAccessor.HttpContext?
                    .User?.FindFirst("HospitalID")?.Value;

                if (int.TryParse(value, out var id) && id > 0)
                    return id;

                // fallback for testing / non-auth requests
                return 1;
            }
        }

        // =========================
        // HOSPITAL GROUP ID (SAFE)
        // =========================
        public int HospitalGroupId
        {
            get
            {
                var value = _httpContextAccessor.HttpContext?
                    .User?.FindFirst("HospitalGroupID")?.Value;

                if (int.TryParse(value, out var id) && id > 0)
                    return id;

                // fallback
                return 1;
            }
        }

        // =========================
        // IP ADDRESS
        // =========================
        public string IpAddress =>
            _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "";

        // =========================
        // BROWSER INFO
        // =========================
        public string Browser =>
            _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString() ?? "";
    }
}