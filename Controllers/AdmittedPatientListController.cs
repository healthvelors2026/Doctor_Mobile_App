using DoctorMobileApp.CommonClass;
using DoctorMobileApp.Models;
using DoctorMobileApp.WebService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
namespace DoctorMobileApp.Controllers
{
    [Route("api/admitted-patients")]
    [ApiController]
    public class AdmittedPatientListController : ControllerBase
    {
        private readonly AdmittedPatientListService _AdmittedListservice;
        private readonly IDbConnectionFactory _db;
        private readonly IConfiguration _configuration;
        private int hospitalidf => int.TryParse(User.FindFirst("HospitalIDF")?.Value, out var id) ? id : 0;
        public AdmittedPatientListController(IDbConnectionFactory db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
            _AdmittedListservice = new AdmittedPatientListService(_db, _configuration);
            
        }
        [Authorize]
        [HttpPost("get-ward-doctor")]
        public async Task<IActionResult> GetWardDoctorList()
        {
            var Data = await _AdmittedListservice.GetWardDoctorListAsync(hospitalidf);
            return Ok(new
            {
                Data.WardList,
                Data.DoctorList
            });
        }
        [Authorize]
        [HttpPost("search")]
        public async Task<IActionResult> GetAdmittedPatientList(AdmittedPatienttRequest request)
        {
            var Data = await _AdmittedListservice.GetAdmittedPatientListAsync(request, hospitalidf);
            return Ok(new
            {
                Data.AdmittedPatientList,
                Data.TotalRecords,
                Data.TotalPages
            });
        }
    }
}
