using DoctorMobileApp.CommonClass;
using DoctorMobileApp.WebService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public AdmittedPatientEMRRecordsController(IDbConnectionFactory db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
            _AdmittedPatientEMRRecordsService = new AdmittedPatientEMRRecordsService(_db, _configuration);
        }
        [Authorize]
        [HttpPost("getEmrVitals")]
        public async Task<IActionResult> getEmrVitals(int AdmissionIDF)
        {
            var Data = await _AdmittedPatientEMRRecordsService.getEmrVital(hospitalidf, AdmissionIDF);
            return Ok(new { Data.lstVital });
        }
        [Authorize]
        [HttpPost("getLastVisitPathoRadioProcedureRecords")]
        public async Task<IActionResult> getLastVisitPathoRadioProcedureRecords(int AdmissionIDF, int Type)
        {
            var Data = await _AdmittedPatientEMRRecordsService.getLastVisitPathoRadioProcedureRecords(hospitalidf, AdmissionIDF, Type);

            List<dynamic> obj = new List<dynamic>();

            foreach (var itm in Data.lstPathoRadioProcedure)
            {
                if (Type == 0) // Pathology
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
                else if (Type == 1) // Radiology
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
                else if (Type == 2) // Procedure
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
        [HttpPost("getGetValueFeedPathoTestReportList")]
        public async Task<IActionResult>getGetValueFeedPathoTestReport(int PathoRegistrationIDP)
        {
            var Data = await _AdmittedPatientEMRRecordsService.getGetValueFeedPathoTestReportList(PathoRegistrationIDP);
            return Ok(new { Data });
        }
    }
}
