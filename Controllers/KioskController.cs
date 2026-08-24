using DoctorMobileApp.CommonClass;
using DoctorMobileApp.Models;
using DoctorMobileApp.WebService;
using DoctorMobileApp.WebServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static DoctorMobileApp.Models.KioskModel;

namespace DoctorMobileApp.Controllers
{
    //testss chetanss
    //testss 
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class KioskController : ControllerBase
    {
        private readonly KioskService _kioskService;
        private readonly IDbConnectionFactory _db;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private int hospitalidf => int.TryParse(User.FindFirst("HospitalIDF")?.Value, out var id) ? id : 0;
        private int hospitalgroupidf => int.TryParse(User.FindFirst("HospitalGroupIDF")?.Value, out var id) ? id : 0;
        private string hospitalCode => User.FindFirst("HospitalCode")?.Value ?? string.Empty;
        private int userIdf => int.TryParse(User.FindFirst("UserIdf")?.Value, out var id) ? id : 0;
        private int fasModeOFPaymentIDF => int.TryParse(User.FindFirst("FASModeOFPaymentIDF")?.Value, out var id) ? id : 0;

        public KioskController( IDbConnectionFactory db, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, HttpClient httpClient)
        {
           // _kioskService = kioskService;
            _db = db;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _kioskService = new KioskService(_db, _configuration, _httpContextAccessor, httpClient);
        }

        [HttpPost]
        [Route("get-patient-detail")]
        public async Task<IActionResult> GetPatientSearchDeatil([FromBody] PatientSearchModel patientSearchRequest)
        {
            var patientDetail = await _kioskService.GetPatientSearchListAsync(patientSearchRequest, hospitalidf);
            if (patientDetail == null || patientDetail.Count == 0)
            {
                return NotFound(new
                {
                    Status = false,
                    Message = "Patient Not Found"
                });
            }
            return Ok(new
            {
                Status = true,
                Message = "Success",
                Data = patientDetail
            });
        }

        [HttpPost]
        [Route("get-skill-set")]
        public async Task<IActionResult> GetSkillSet(CancellationToken cancellationToken)
        {
            string baseUrl = $"{Request.Scheme}://{Request.Host}";
            var skillSetList = await _kioskService.GetSkillSetListAsync(
                hospitalgroupidf,
                hospitalCode,
                baseUrl,
                cancellationToken);
            if (skillSetList == null || skillSetList.Count == 0)
            {
                return NotFound(new
                {
                    Status = false,
                    Message = "Record Not Found"
                });
            }
            return Ok(new
            {
                Status = true,
                Message = "Success",
                Data = skillSetList
            });
        }

        [HttpPost]
        [Route("generate-otp")]
        public async Task<IActionResult> GenerateOTP([FromBody] GeneratePatientOTPRequestModel requestModel)
        {
            var result = await _kioskService.GenerateOTPAsync(requestModel,hospitalidf);

            if (result == null)
            {
                return BadRequest(new
                {
                    Status = false,
                    Message = "Failed to generate OTP"
                });
            }

            return Ok(new
            {
                Status = true,
                Message = "OTP generated successfully",
                Data = result
            });
        }

        [HttpPost]
        [Route("verify-otp")]
        public async Task<IActionResult> VerifyOTP([FromBody] VerifyPatientOTPRequestModel requestModel)
        {
            var result = await _kioskService.VerifyOTPAsync(requestModel);
            if (result == null)
            {
                return BadRequest(new
                {
                    Status = false,
                    Message = "OTP Verification Failed"
                });
            }

            return Ok(result);
        }

        [HttpPost]
        [Route("get-patho-report-list-for-print")]
        public async Task<IActionResult> GetPathoReportListForPrint([FromBody] PathoReportRequestModel requestModel)
        {
            var pathoReportDetail = await _kioskService.GetPathoReportListForPrintAsync(requestModel,hospitalidf);
            if (pathoReportDetail == null || pathoReportDetail.Count == 0)
            {
                return NotFound(new
                {
                    Status = false,
                    Message = "Pathology Report Not Found"
                });
            }
            return Ok(new
            {
                Status = true,
                Message = "Success",
                Data = pathoReportDetail
            });
        }

        [HttpPost]
        [Route("get-opd-test-receipt")]
        public async Task<IActionResult> GetOPDTestReceipt([FromBody] OPDTestReceiptRequestModel requestModel)
        {
            var opdTestReceiptList = await _kioskService.GetOPDTestReceiptListAsync(requestModel, hospitalidf);
            if (opdTestReceiptList == null || opdTestReceiptList.Count == 0)
            {
                return NotFound(new
                {
                    Status = false,
                    Message = "Record Not Found"
                });
            }
            return Ok(new
            {
                Status = true,
                Message = "Success",
                Data = opdTestReceiptList
            });
        }

        [HttpPost]
        [Route("save-opd-test-receipt")]
        public async Task<IActionResult> SaveOPDTestReceipt([FromBody] SaveOPDTestReceiptRequestModel receiptModel)
        {
            if (receiptModel == null)
            {
                return BadRequest(new
                {
                    Status = false,
                    Message = "Invalid Request"
                });
            }
            var receipt = await _kioskService.SaveOPDTestReceiptAsync(receiptModel, userIdf, hospitalidf);

            if (receipt == null || (receipt.VoucherIDP <= 0 && receipt.VoucherIDP_NA <= 0))
            {
                return BadRequest(new
                {
                    Status = false,
                    Message = "Failed"
                });
            }

            return Ok(new
            {
                Status = true,
                Message = "OPD Test Receipt Saved Successfully",
                Receipt = receipt
            });
        }
      
        [HttpPost]
        [Route("get-last-visit-doctor")]
        public async Task<IActionResult> GetLastvisitDoctor([FromBody] LastVisitDrRequestmodel requestmodel)
        {
            var data = await _kioskService.GetLastVisitDoctorAsync(requestmodel, hospitalidf);

            if (data == null)
            {
                return NotFound(new
                {
                    Status = false,
                    Message = "Record Not Found"
                });
            }

            return Ok(new
            {
                Status = true,
                Message = "Success",
                Data = data
            });
        }

        [HttpPost]
        [Route("get-patient-appointment-detail")]
        public async Task<IActionResult> GetPatientAppointmentDetail([FromBody] PatientLatestAppointmentRequestModel requestmodel)
        {
            var data = await _kioskService.GetLatestPatientAppointmentDetailAsync(requestmodel, hospitalidf);

            if (data == null)
            {
                return NotFound(new
                {
                    Status = false,
                    Message = "Record Not Found"
                });
            }

            return Ok(new
            {
                Status = true,
                Message = "Success",
                Data = data
            });
        }

        [HttpPost]
        [Route("get-doctor-list")]
        public async Task<IActionResult> GetDoctorList([FromBody] DoctorRequestModel requestModel)
        {
            var doctorList = await _kioskService.GetDoctorListAsync(requestModel, hospitalidf);
            if (doctorList == null || doctorList.Count == 0)
            {
                return NotFound(new
                {
                    Status = false,
                    Message = "Doctor Not Found"
                });
            }
            return Ok(new
            {
                Status = true,
                Message = "Success",
                Data = doctorList
            });
        }

        [HttpPost]
        [Route("save-advance-payment")]
        public async Task<IActionResult> SaveAdvanceDeposit([FromBody] AdvanceDepositModel depositmodel)
        {
            if (depositmodel == null)
            {
                return BadRequest(new
                {
                    Status = false,
                    Message = "Invalid Request"
                });
            }
            var result = await _kioskService.SaveAdvanceDepositAsync(depositmodel, hospitalidf, fasModeOFPaymentIDF, userIdf);
            if (result == null || result.VoucherIDP <= 0)
            {
                return BadRequest(new
                {
                    Status = false,
                    Message = "Failed"
                });
            }
            return Ok(new
            {
                Status = true,
                Message = "Advance Deposit Saved Successfully",
                VoucherID = result.VoucherIDP,

                Data = new
                {
                    result.VoucherIDP,
                    result.TransactionType,
                    result.AdvanceDepositSaveDateTime
                }
            });
        }

        [HttpPost]
        [Route("save-opd-registration")]
        public async Task<IActionResult> SaveOPDRegistration([FromBody] SaveOPDRegistrationModel receiptModel)
        {
            if (receiptModel == null)
            {
                return BadRequest(new
                {
                    Status = false,
                    Message = "Invalid Request"
                });
            }

            var result = await _kioskService.SaveOPDRegistrationAsync(receiptModel, userIdf, hospitalidf);

            if (result == null || result.VoucherIDP <= 0)
            {
                return BadRequest(new
                {
                    Status = false,
                    Message = "Failed to Save OPD Registration"
                });
            }

            return Ok(new
            {
                Status = true,
                Message = "OPD Registration Saved Successfully",
                ReceiptID = result.VoucherIDP,
                data = new { 
                    result.VoucherIDP,
                    result.OPDRegistrationIDP,
                    result.VoucherNumber,
                    result.TransactionType,
                    result.OPDRegistrationSaveDateTime,
                    result.RegistrationCode,
                    result.TokenNumber,
                    result.RoomNumber
                }
            });
        }
    }
}
