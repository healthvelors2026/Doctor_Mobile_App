using DoctorMobileApp.CommonClass;
using DoctorMobileApp.Models;
using DoctorMobileApp.WebService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DoctorMobileApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OPDRegistrationController : ControllerBase
    {
        private readonly OPDRegistrationService _OPDRegistrationservice;
        private readonly IDbConnectionFactory _db;
        private readonly IConfiguration _configuration;
        private int hospitalidf => int.TryParse(User.FindFirst("HospitalIDF")?.Value, out var id) ? id : 0;
        private int hospitalgroupidf => int.TryParse(User.FindFirst("HospitalGroupIDF")?.Value, out var id) ? id : 0;
        private int UserIdf => int.TryParse(User.FindFirst("UserIdf")?.Value, out var id) ? id : 0;
        public OPDRegistrationController(IDbConnectionFactory db, IConfiguration configuration, ITokenDisplayBroadcastService tokenDisplayBroadcast)
        {
            _db = db;
            _configuration = configuration;
            _OPDRegistrationservice = new OPDRegistrationService(_db, _configuration, tokenDisplayBroadcast);
        }
        [Authorize]
        [HttpPost("get-opd-registration-details")]
        public async Task<IActionResult> GetOPDRegistrationDetails(OPDRegistrationDetailsRequest request)
        {
            var result = await _OPDRegistrationservice.GetOPDRegistrationDetailsAsync(request, hospitalidf, hospitalgroupidf);
            return Ok(result);
        }
        [Authorize]
        [HttpPost("save-opdentry-with-test")]
        public async Task<IActionResult> SaveOPDEntryWithTest([FromBody] OPDRegistration request)
        {
            if (request == null)
                return BadRequest("Request is required.");

            if (request.OPDRegistrationDetails.OPDRegistrationIDP <= 0)
                return BadRequest("Invalid OPDRegistrationIDF.");

            if (!(request.PathoTestList?.Any() == true ||
                  request.RadioTestList?.Any() == true ||
                  request.ProcedureTestList?.Any() == true))
            {
                return BadRequest("At least one investigation test list is required.");
            }
            var result = await _OPDRegistrationservice.SaveOPDEntryWithTestAsync(
                request,
                UserIdf,
                hospitalidf,
                hospitalgroupidf);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("get-doctor-opd-entry-token-list")]
        public async Task<IActionResult> GetDoctorOPDEntryTokenList(DoctorOPDEntryTokenListRequest request)
        {
            var result = await _OPDRegistrationservice.GetDoctorOPDEntryTokenListAsync(request.DoctorIDF);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("get-consulting-room-list")]
        public async Task<IActionResult> GetConsultingRoomList()
        {
            var result = await _OPDRegistrationservice.GetConsultingRoomListAsync(hospitalidf);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("insert-token-display")]
        public async Task<IActionResult> InsertTokenDisplay(InsertTokenDisplayRequest request, CancellationToken cancellationToken)
        {
            // Normal tablet fire - never a promotion, decided here, not by the caller.
            var result = await _OPDRegistrationservice.InsertTokenDisplayAsync(request, isDirectSelection: false, cancellationToken);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("select-pending-token")]
        public async Task<IActionResult> SelectPendingToken(InsertTokenDisplayRequest request, CancellationToken cancellationToken)
        {
            // Direct selection from the Pending list - always promotes the existing Upcoming to Running, decided here, not by the caller.
            var result = await _OPDRegistrationservice.InsertTokenDisplayAsync(request, isDirectSelection: true, cancellationToken);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("get-running-upcoming-token")]
        public async Task<IActionResult> GetRunningAndUpcomingToken(RunningAndUpcomingTokenRequest request)
        {
            var result = await _OPDRegistrationservice.GetRunningAndUpcomingTokenAsync(request);
            return Ok(result);
        }
    }
}
