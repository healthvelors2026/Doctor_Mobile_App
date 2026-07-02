using DoctorMobileApp.CommonClass;
using DoctorMobileApp.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DoctorMobileApp.WebService
{
    public class AdmittedPatientEMRRecordsService
    {
        private readonly IDbConnectionFactory _dbHelper;
        private readonly IConfiguration _configuration;
        public AdmittedPatientEMRRecordsService(IDbConnectionFactory db, IConfiguration configuration)
        {
            _dbHelper = db;
            _configuration = configuration;
        }
        public async Task<AdmittedPatientEMRRecords> getEmrVital(int HospitalIDF, int AdmissionIDF)
        {
            var response = new AdmittedPatientEMRRecords
            {
                lstVital = new List<VitalList>(),
            };
            var vitalParams = new[]
            {
                new SqlParameter("@HospitalID", HospitalIDF),
                new SqlParameter("@AdmissionID", AdmissionIDF)
            };
            var vitalTask = _dbHelper.QueryAsync<VitalList>("API_Sp_GetLast10VisitVitalRecords", CommandType.StoredProcedure, vitalParams);
            response.lstVital = vitalTask.Result;
            return response;
        }
        public async Task<AdmittedPatientPathoRadioProcedureRecords> getLastVisitPathoRadioProcedureRecords(int HospitalIDF, int AdmissionIDF, int Type)
            {
            var response = new AdmittedPatientPathoRadioProcedureRecords
            {
                lstPathoRadioProcedure = new List<PathoRadioProcedureList>()
            };
            var pathoRadioParams = new[]
            {
                new SqlParameter("@HospitalID", HospitalIDF),
                new SqlParameter("@AdmissionID", AdmissionIDF),
                new SqlParameter("@Type", Type)
            };
            response.lstPathoRadioProcedure =await _dbHelper.QueryAsync<PathoRadioProcedureList>("API_Sp_GetLastVisitPathoRadioProcRecords",CommandType.StoredProcedure,pathoRadioParams);
            return response;
        }
        public async Task<ValueFeedPathoTestReportRecords> getGetValueFeedPathoTestReportList(int PathoRegistrationIDP)
        {
            var response = new ValueFeedPathoTestReportRecords
            {
                lstFeedPathoTestReport = new List<FeedPathoTestReportList>()
            };
            var feedPathoTestReportParams = new[]
            {
                new SqlParameter("@PathoRegistrationIDP", PathoRegistrationIDP)
            };
            response.lstFeedPathoTestReport = await _dbHelper.QueryAsync<FeedPathoTestReportList>("API_SPGetValueFeedPathoTestReportList", CommandType.StoredProcedure, feedPathoTestReportParams);
            return response;
        }
    }
}
