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
        public OPDRegistrationController(IDbConnectionFactory db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
            _OPDRegistrationservice = new OPDRegistrationService(_db, _configuration);
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
    }
}
