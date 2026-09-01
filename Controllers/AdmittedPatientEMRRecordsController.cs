using DoctorMobileApp.CommonClass;
using DoctorMobileApp.Models;
using DoctorMobileApp.WebService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static DoctorMobileApp.Models.KioskModel;
namespace DoctorMobileApp.Controllers
{
    [Route("api/admittedPatientsEmrRecords")]
    [ApiController]
    public class AdmittedPatientEMRRecordsController : Controller
    {
        private readonly AdmittedPatientEMRRecordsService _AdmittedPatientEMRRecordsService;
        private readonly IDbConnectionFactory _db;
        private readonly IConfiguration _configuration;
        private int hospitalidf => int.TryParse(User.FindFirst("HospitalIDF")?.Value, out var id) ? id : 0;
        private string hospitalCode => User.FindFirst("HospitalCode")?.Value ?? string.Empty;
        public AdmittedPatientEMRRecordsController(IDbConnectionFactory db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
            _AdmittedPatientEMRRecordsService = new AdmittedPatientEMRRecordsService(_db, _configuration);
        }
        [Authorize]
        [HttpPost("getEmrVitals")]
        public async Task<IActionResult> getEmrVitals([FromBody] GetEmrVitalsRequest emrVitalsRequestModel)
        {
            var Data = await _AdmittedPatientEMRRecordsService.getEmrVital(hospitalidf, emrVitalsRequestModel.AdmissionIDF);
            return Ok(new { Data.lstVital });
        }
        [Authorize]
        [HttpPost("getLastVisitPathoRadioProcedureRecords")]
        public async Task<IActionResult> getLastVisitPathoRadioProcedureRecords([FromBody] GetLastVisitPathoRadioProcedureRecordsRequest requestModel)
        {
            var Data = await _AdmittedPatientEMRRecordsService.getLastVisitPathoRadioProcedureRecords(hospitalidf, requestModel.AdmissionIDF, requestModel.Type);

            List<dynamic> obj = new List<dynamic>();

            foreach (var itm in Data.lstPathoRadioProcedure)
            {
                if (requestModel.Type == 0) 
                {
                    obj.Add(new
                    {
                        itm.VisitCode,
                        itm.VisitDateTime,
                        itm.Test,
                        itm.DoctorName,
                        itm.Collected,
                        itm.IsSampleAcknowledged,
                        itm.TestDate,
                        itm.Status,
                        itm.ReportStatus,
                        itm.Paid,
                        itm.CategoryIDF,
                        itm.IsPortable,
                        itm.Flag,
                        itm.HospitalCode,
                        itm.AdmissionRegIDP,
                        itm.RegistrationIDP,
                        itm.DocVisitIDP,
                        itm.PathoTestReportIDP,
                        itm.PatientIDF,
                        itm.DoctorIDF,
                        itm.EmployeeIDP
                    });
                }
                else if (requestModel.Type == 1) 
                {
                    obj.Add(new
                    {
                        itm.VisitCode,
                        itm.VisitDateTime,
                        itm.Test,
                        itm.DoctorName,
                        itm.TestDate,
                        itm.Status,
                        itm.ReportStatus,
                        itm.RadioCategoryName,
                        itm.Paid,
                        itm.DoctorIDF,
                        itm.PatientIDF,
                        itm.EmployeeIDP,
                        itm.CategoryIDF,
                        itm.Flag,
                        itm.AdmissionRegIDP,
                        itm.RegistrationIDP,
                        itm.ReportPath,
                        itm.ExternalReportPath,
                        itm.RefundRemarks
                    });
                }
                else if (requestModel.Type == 2) 
                {
                    obj.Add(new
                    {
                        itm.DocVisitIDP,
                        itm.VisitCode,
                        itm.VisitDateTime,
                        itm.DoctorIDF,
                        itm.PatientIDF,
                        itm.Test,
                        itm.EmployeeIDP,
                        itm.EmpFName,
                        itm.EmpMName,
                        itm.EmpLName,
                        itm.ProcCnt,
                        itm.Collected,
                        itm.TestDate,
                        itm.Status,
                        itm.CategoryIDF,
                        itm.IsPortable,
                        itm.MedicalProcRegDetailIDP,
                        itm.Flag,
                        itm.RegIDP,
                        itm.RegistrationIDP,
                        itm.Paid,
                        itm.ReportPath,
                        itm.HospitalCode,
                        itm.ProcedureCategoryName,
                        itm.ExternalReportPath,
                        itm.RefundRemarks
                    });
                }
            }
            return Ok(new { Data = obj });
        }

        [Authorize]
        [HttpPost("getRadioReportHtml")]
        public async Task<IActionResult> getRadioReportHtml([FromQuery] string reportPath)
        {
            var htmlReport = await _AdmittedPatientEMRRecordsService.GetPatientRadioReportHtmlAsync(hospitalCode, reportPath);
            if (string.IsNullOrEmpty(htmlReport))
            {
                return NotFound(
                    new
                    {
                        Status = false,
                        Message = "Report Not Exist"
                    });
            }
            return Ok(
                new
                {
                    Status = true,
                    Data = htmlReport
                });
        }
        [Authorize]
        [HttpPost("getGetValueFeedPathoTestReportList")]
        public async Task<IActionResult> getGetValueFeedPathoTestReport([FromBody] GetValueFeedPathoTestReportRequest requestModel)
        {
            var Data = await _AdmittedPatientEMRRecordsService.getGetValueFeedPathoTestReportList(requestModel.PathoRegistrationIDP);
            return Ok(new { Data });
        }
        [Authorize]
        [HttpPost("getLatestPainAssessment")]
        public async Task<IActionResult> getLatestPainAssessment([FromBody] GetLatestPainAssessmentRequest requestModel)
        {
            var Data = await _AdmittedPatientEMRRecordsService.getLatestPainAssessmentList(requestModel.AdmissionIDF);
            return Ok(new { Data });
        }

        [Authorize]
        [HttpPost("GetPatientLatest10PathologyResults")]
        public async Task<IActionResult> getPatientLatest10PathologyResult([FromBody] GetPatientLatest10PathologyRequest requestModel)
        {
            var Data = await _AdmittedPatientEMRRecordsService.getPatientLatest10PathologyList(requestModel.PatientIDF);
            return Ok(new { Data });
        }
    }
}
