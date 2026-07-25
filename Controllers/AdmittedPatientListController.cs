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
        private int userIdf => int.TryParse(User.FindFirst("UserIdf")?.Value, out var id) ? id : 0;

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
            var result = await _AdmittedListservice.GetBedTransferEditAsync(request,hospitalidf,hospitalGroupIDF);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Active patient admission was not found."
                });
            }

            return Ok(result);
        }

        [Authorize]
        [HttpPost("get-bed-swap")]
        public async Task<IActionResult> GetBedSwap([FromBody] SwapPatientRequest request)
        {
            if (request == null || request.PatientID <= 0)
            {
                return BadRequest(new
                {
                    message = "Valid patient ID is required."
                });
            }

            if (hospitalidf <= 0)
            {
                return Unauthorized(new
                {
                    message = "Hospital information was not found."
                });
            }

            try
            {
                var result = await _AdmittedListservice.GetBedSwapAsync(request, hospitalidf);

                if (result == null)
                {
                    return NotFound(new
                    {
                        message = "Active patient admission and current bed tracking were not found."
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _db.LogError(ex, "DoctorApp_API_GetSwapPatients");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Unable to load eligible swap patients."
                });
            }
        }

        [Authorize]
        [HttpPost("save-bed-transfer")]
        public async Task<IActionResult> SaveBedTransfer([FromBody] SaveBedTransferRequest request)
        {
            if (request == null)
            {
                return BadRequest(new SaveBedTransferResponse
                {
                    Success = false,
                    ResultCode = -1,
                    Message = "Request body is required."
                });
            }
            if (hospitalidf <= 0)
            {
                return Unauthorized(new SaveBedTransferResponse
                {
                    Success = false,
                    ResultCode = -401,
                    Message = "Hospital information was not found."
                });
            }
            if (userIdf <= 0)
            {
                return Unauthorized(new SaveBedTransferResponse
                {
                    Success = false,
                    ResultCode = -401,
                    Message = "User information was not found."
                });
            }
            if (employeeIDF <= 0)
            {
                return Unauthorized(new SaveBedTransferResponse
                {
                    Success = false,
                    ResultCode = -401,
                    Message = "Employee information was not found."
                });
            }
            var result = await _AdmittedListservice.SaveBedTransferAsync(
                request,
                hospitalidf,
                userIdf,
                employeeIDF);

            if (result.Success)
            {
                return Ok(result);
            }
            return result.ResultCode switch
            {
                -500 => StatusCode(StatusCodes.Status500InternalServerError, result),
                -401 => Unauthorized(result),
                -1 => BadRequest(result),
                2 => BadRequest(result),
                3 => Conflict(result),
                4 => NotFound(result),
                5 => BadRequest(result),
                6 => Conflict(result),
                7 => Conflict(result),
                8 => Conflict(result),
                9 => Conflict(result),
                10 => StatusCode(StatusCodes.Status403Forbidden, result),
                11 => BadRequest(result),
                12 => Conflict(result),
                13 => Conflict(result),
                _ => BadRequest(result)
            };
        }
    }
}


//[Authorize]
//[HttpPost("request-bed-transfer")]
//public async Task<IActionResult> RequestBedTransfer([FromBody] RequestBedTransferRequest request)
//{
//    if (request == null)
//    {
//        return BadRequest(new RequestBedTransferResponse
//        {
//            Success = false,
//            ResultCode = -1,
//            Message = "Request body is required."
//        });
//    }

//    if (hospitalidf <= 0 || hospitalGroupIDF <= 0 || userIdf <= 0 || employeeIDF <= 0)
//    {
//        return Unauthorized(new RequestBedTransferResponse
//        {
//            Success = false,
//            ResultCode = -401,
//            Message = "Authenticated hospital, hospital group, user, and employee information is required."
//        });
//    }

//    var result = await _AdmittedListservice.RequestBedTransferAsync(
//        request,
//        hospitalidf,
//        hospitalGroupIDF,
//        userIdf,
//        employeeIDF);

//    if (result.Success)
//    {
//        return Ok(result);
//    }

//    return result.ResultCode switch
//    {
//        -500 => StatusCode(StatusCodes.Status500InternalServerError, result),
//        -401 => Unauthorized(result),
//        10 => StatusCode(StatusCodes.Status403Forbidden, result),
//        4 => NotFound(result),
//        3 or 6 or 7 or 8 or 9 => Conflict(result),
//        _ => BadRequest(result)
//    };
//}
