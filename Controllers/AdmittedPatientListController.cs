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
        private int employeeIDF => int.TryParse(User.FindFirst("EmployeeIDF")?.Value, out var id) ? id : 0; 
        private int hospitalGroupIDF => int.TryParse(User.FindFirst("HospitalGroupIDF")?.Value,out var id) ? id : 0;
        public AdmittedPatientListController(IDbConnectionFactory db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
            _AdmittedListservice = new AdmittedPatientListService(_db, _configuration);
            
        }
        [Authorize]
        [HttpPost("get-ward-list")]
        public async Task<IActionResult> GetWardList()
        {
            var Data = await _AdmittedListservice.GetWardListAsync(hospitalidf);
            return Ok(new
            {
                Data.WardList
            });
        }
        [Authorize]
        [HttpPost("get-doctor-list")]
        public async Task<IActionResult> GetDoctorList()
        {
            var Data = await _AdmittedListservice.GetDoctorListAsync(hospitalidf);
            return Ok(new
            {
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

        [Authorize]
        [HttpPost("get-bed-transfer")]
        public async Task<IActionResult> GetBedTransfer([FromBody] BedTransferRequest request)
        {
            var data = await _AdmittedListservice.GetBedTransferAsync(request, hospitalidf);

            return Ok(data);
        }

        [Authorize]
        [HttpPost("get-addmision-check-list")]
        public async Task<IActionResult> GetAddmisionCheckList([FromBody] AddmisionCheckRequest request)
        {
            var data = await _AdmittedListservice.GetAddmisionCheckListAsync(request, hospitalidf,hospitalGroupIDF);
            return Ok(data);
        }

        [Authorize]
        [HttpPost("get-bed-transfer-edit")]
        public async Task<IActionResult> GetBedTransferEdit([FromBody] BedTransferEditRequest request)
        {
            var result = await _AdmittedListservice.GetBedTransferEditAsync(request, hospitalidf);

            return Ok(result);
        }

        //[Authorize]
        //[HttpPost("get-swap-patient-list")]
        //public async Task<IActionResult> GetSwapPatientList([FromBody] SwapPatientRequest request)
        //{
        //    var data = await _AdmittedListservice.GetSwapPatientListAsync(request, hospitalidf);

        //    return Ok(data);
        //}
    }
}
