using DoctorMobileApp.CommonClass;
using DoctorMobileApp.Models;
using DoctorMobileApp.WebService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
namespace DoctorMobileApp.Controllers
{
    [Route("api/doctor-visit")]
    [ApiController]
    public class DoctorFirstVisitController : ControllerBase
    {
        private readonly DoctorFirstVisitService _DoctorFirstservice;
        private readonly IDbConnectionFactory _db;
        private readonly IConfiguration _configuration;
        private int hospitalidf => int.TryParse(User.FindFirst("HospitalIDF")?.Value, out var id) ? id : 0;
        private int hospitalgroupidf => int.TryParse(User.FindFirst("HospitalGroupIDF")?.Value, out var id) ? id : 0;
        private int UserIdf => int.TryParse(User.FindFirst("UserIdf")?.Value, out var id) ? id : 0;
        public DoctorFirstVisitController(IDbConnectionFactory db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
            _DoctorFirstservice = new DoctorFirstVisitService(_db, _configuration);
        }
        [Authorize]
        // 🔹 POST: api/doctor-visit/get-details
        [HttpPost("get-details")]
        public async Task<IActionResult> GetVisitDetails(VisitDetailsRequest request)
        {
            var Data = await _DoctorFirstservice.GetVisitDetailsAsync(request, hospitalidf, hospitalgroupidf);
            return Ok(Data);
        }
        [Authorize]
        // 🔹 POST: api/doctor-visit/get-charge
        [HttpPost("get-charge")]
        public async Task<IActionResult> GetCalcVisitCharge(VisitChargeRequest request)
        {
            var data = await _DoctorFirstservice.GetCalcVisitChargeAsync(
                request, hospitalidf, hospitalgroupidf);
            return Ok(data);
        }
        [Authorize]
        // 🔹 POST: api/doctor-visit/get-foodlist
        [HttpPost("get-foodlist")]
        public async Task<IActionResult> GetFoodList(int dietCategoryIDF)
        {
            if (dietCategoryIDF <= 0)
                return BadRequest("Invalid Diet Category ID");
            var result = await _DoctorFirstservice.GetFoodListAsync(dietCategoryIDF, hospitalgroupidf);
            return Ok(result);
        }
        [Authorize]
        // 🔹 POST: api/doctor-visit/get-pathotest-group
        [HttpPost("get-pathotest-group")]
        public async Task<IActionResult> GetPathoTesGrouptList()
        {
            var result = await _DoctorFirstservice.GetPathoTesGrouptListAsync(hospitalidf);
            return Ok(result);
        }
        [Authorize]
        // 🔹 POST: api/doctor-visit/get-pathotest-pricelist
        [HttpPost("get-pathotest-pricelist")]
        public async Task<IActionResult> GetPathoTestPriceList(TestPriceRequest request)
        {
            var result = await _DoctorFirstservice.GetPathoTestPriceListAsync(request, hospitalidf, hospitalgroupidf);
            return Ok(result);
        }
        [Authorize]
        // 🔹 POST: api/doctor-visit/get-visit-pathotestlist
        [HttpPost("get-visit-patholist")]
        public async Task<IActionResult> GetVisitPathoTestList(VisitTestRequest request)
        {
            var result = await _DoctorFirstservice.GetVisitPathoTestListAsync(request, hospitalidf, hospitalgroupidf);
            return Ok(result);
        }
        [Authorize]
        // 🔹 POST: api/doctor-visit/get-radio-categorylist
        [HttpPost("get-radio-categorylist")]
        public async Task<IActionResult> GetRadioCategoryList()
        {
            var result = await _DoctorFirstservice.GetRadioCategoryListAsync(hospitalgroupidf);
            return Ok(result);
        }
        [Authorize]
        // 🔹 POST: api/doctor-visit/get-radiotest-pricelist
        [HttpPost("get-radiotest-pricelist")]
        public async Task<IActionResult> GetRadioTestPriceList(TestPriceRequest request)
        {
            var result = await _DoctorFirstservice.GetRadioTestPriceListAsync(request, hospitalidf, hospitalgroupidf);
            return Ok(result);
        }
        [Authorize]
        // 🔹 POST: api/doctor-visit/get-visit-radiotestlist
        [HttpPost("get-visit-radiolist")]
        public async Task<IActionResult> GetVisitRadioTestList(VisitTestRequest request)
        {
            var result = await _DoctorFirstservice.GetVisitRadioTestListAsync(request, hospitalidf, hospitalgroupidf);
            return Ok(result);
        }
        [Authorize]
        // 🔹 POST: api/doctor-visit/get-procedure-categorylist
        [HttpPost("get-procedure-categorylist")]
        public async Task<IActionResult> GetProcedureCategoryList()
        {
            var result = await _DoctorFirstservice.GetProcedureCategoryListAsync(hospitalgroupidf);
            return Ok(result);
        }
        [Authorize]
        // 🔹 POST: api/doctor-visit/get-proceduretesttest-pricelist
        [HttpPost("get-proceduretest-pricelist")]
        public async Task<IActionResult> GetProcedureTestPriceList(TestPriceRequest request)
        {
            var result = await _DoctorFirstservice.GetProcedureTestPriceListAsync(request, hospitalidf, hospitalgroupidf);
            return Ok(result);
        }
        [Authorize]
        // 🔹 POST: api/doctor-visit/get-visit-proceduretestlist
        [HttpPost("get-visit-procedurelist")]
        public async Task<IActionResult> GetVisitProcedureTestList(VisitTestRequest request)
        {
            var result = await _DoctorFirstservice.GetVisitProcedureTestListAsync(request, hospitalidf, hospitalgroupidf);
            return Ok(result);
        }
        [Authorize]
        // 🔹 POST: api/doctor-visit/save-visit
        [HttpPost("save-visit")]
        public async Task<IActionResult> SaveVisit([FromBody] DoctorFirstVisit model)
        {
            if (model == null)
                return BadRequest("Invalid request data");
            try
            {
                model.VisitDetails.UserIDF = UserIdf;
                model.VisitDetails.HospitalIDF = hospitalidf;
                model.VisitDetails.HospitalGroupIDF = hospitalgroupidf;
                bool isUpdate = model.VisitDetails.DocVisitIDP > 0;
                var result = await _DoctorFirstservice.SaveVisitAsync(model);
                if (result.StartsWith("Transaction Failed"))
                    return StatusCode(500, result);
                return Ok(new
                {
                    success = true,
                    //message = result
                    message = isUpdate ? "Visit updated successfully" : "Visit saved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal Server Error",
                    error = ex.Message
                });
            }
        }
    }
}
