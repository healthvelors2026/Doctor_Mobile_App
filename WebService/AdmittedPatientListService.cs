using DoctorMobileApp.CommonClass;
using DoctorMobileApp.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DoctorMobileApp.WebService
{
    public class AdmittedPatientListService
    {
        private readonly IDbConnectionFactory _dbHelper;
        private readonly IConfiguration _configuration;
        public AdmittedPatientListService(IDbConnectionFactory db, IConfiguration configuration)
        {
            _dbHelper = db;
            _configuration = configuration;
        }
        public async Task<AdmittedPatientListRequest> GetWardDoctorListAsync(int hospitalidf)
        {
            var response = new AdmittedPatientListRequest
            {
                WardList = new List<WardList>(),
                DoctorList = new List<DoctorList>(),
            };
            var wardParams = new[]
            {
                    new SqlParameter("@HospitalIDF", hospitalidf)
            };
            var doctorParams = new[]
            {
                 new SqlParameter("@HospitalIDF", hospitalidf)
            };
            // Run both queries in parallel (faster)
            var wardTask = _dbHelper.QueryAsync<WardList>(
                "API_GetWardName",
                CommandType.StoredProcedure,
                wardParams);

            var doctorTask = _dbHelper.QueryAsync<DoctorList>(
                "API_DoctorList",
                CommandType.StoredProcedure,
                doctorParams);
            await Task.WhenAll(wardTask, doctorTask);
            response.WardList = wardTask.Result;
            response.DoctorList = doctorTask.Result;
            return response;
        }
        public async Task<AdmittedPatientListRequest> GetAdmittedPatientListAsync(AdmittedPatienttRequest request, int hospitalidf)
        {
            var response = new AdmittedPatientListRequest
            {
                WardIDF = request.WardIDF,
                HospitalIDF = hospitalidf,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                WardList = new List<WardList>(),
                DoctorList = new List<DoctorList>(),
                AdmittedPatientList = new List<AdmittedPatientList>()
            };
            var totalRecordsParam = new SqlParameter("@TotalRecords", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            var totalPagesParam = new SqlParameter("@TotalPages", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            var patientParams = new[]
            {
                        new SqlParameter("@WardIDF", request.WardIDF),
                        new SqlParameter("@Filter", request.Filter),
                        new SqlParameter("@PageNumber", request.PageNumber),
                        new SqlParameter("@PageSize", request.PageSize),
                        totalRecordsParam,
                        totalPagesParam,
                        new SqlParameter("@HospitalIDF", hospitalidf),
            };
            response.AdmittedPatientList = await _dbHelper.QueryAsync<AdmittedPatientList>(
            "API_SP_GetWardAdmissionPatientList",
             CommandType.StoredProcedure,
             patientParams);
            response.TotalRecords = totalRecordsParam.Value != DBNull.Value
            ? Convert.ToInt32(totalRecordsParam.Value) : 0;
            response.TotalPages = totalPagesParam.Value != DBNull.Value
           ? Convert.ToInt32(totalPagesParam.Value) : 0;
            return response;
        }
    }
}
