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
        public async Task<AdmittedPatientListRequest> GetWardListAsync(int hospitalidf)
        {
            var response = new AdmittedPatientListRequest
            {
                WardList = new List<WardList>(),
              
            };
            var wardParams = new[]
            {
                    new SqlParameter("@HospitalIDF", hospitalidf)
            };
            var wardTask = await _dbHelper.QueryAsync<WardList>(
                "API_GetWardName",
                CommandType.StoredProcedure,
                wardParams);
            response.WardList = wardTask;
            return response;
        }
        public async Task<AdmittedPatientListRequest> GetDoctorListAsync(int hospitalidf)
        {
            var response = new AdmittedPatientListRequest
            {
                DoctorList = new List<DoctorList>(),
            };
            var doctorParams = new[]
            {
                 new SqlParameter("@HospitalIDF", hospitalidf)
            };
            var doctorTask = await _dbHelper.QueryAsync<DoctorList>("API_DoctorList",CommandType.StoredProcedure,doctorParams);
            response.DoctorList = doctorTask;
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
        private static string GetAge(DateTime dateOfBirth, DateTime referenceDate)
        {
            int years = referenceDate.Year - dateOfBirth.Year;
            int months = referenceDate.Month - dateOfBirth.Month;
            int days = referenceDate.Day - dateOfBirth.Day;

            if (days < 0)
            {
                months--;
                days += DateTime.DaysInMonth(referenceDate.AddMonths(-1).Year,
                                             referenceDate.AddMonths(-1).Month);
            }

            if (months < 0)
            {
                years--;
                months += 12;
            }

            if (years > 0)
                return $"{years} Year{(years > 1 ? "s" : "")}";

            if (months > 0)
                return $"{months} Month{(months > 1 ? "s" : "")}";

            return $"{days} Day{(days > 1 ? "s" : "")}";
        }
        public async Task<BedTransferResponse> GetBedTransferAsync(BedTransferRequest request, int hospitalidf)
        {
            var response = new BedTransferResponse();

            var patientParams = new[]
            {
                new SqlParameter("@HospitalIDP", hospitalidf),
                new SqlParameter("@PatientIDF", request.PatientID)
            };
            var patient = (await _dbHelper.QueryAsync<BedTransferPatientDBModel>("DoctorApp_API_GetBedTransferPatientDetail",CommandType.StoredProcedure,
             patientParams)).FirstOrDefault();
            
            if (patient == null)
                return response;

            response.AdmissionID = patient.IPDAdmissionDischargeIDP;
            response.IPDRegistrationCode = patient.IPDRegistrationCode;
            response.PatientName = $"{patient.FName} {patient.MName} {patient.LName}".Trim();
            response.AdmissionDate = patient.AdmissionDateTime.ToString("dd/MM/yyyy hh:mm tt");
            response.ClassName = patient.ClassName;
            switch (patient.Gender)
            {
                case 0:
                    response.Gender = "Female";
                    break;

                case 1:
                    response.Gender = "Male";
                    break;

                case 2:
                    response.Gender = "Trans";
                    break;

                default:
                    response.Gender = string.Empty;
                    break;
            }
            response.ConsultDoctor = $"{patient.EmpFName} {patient.EmpMName} {patient.EmpLName}".Trim();
            response.Age = GetAge(patient.DateOfBirth, patient.AdmissionDateTime);

            var historyParams = new[]
            {
                new SqlParameter("@PatientID", request.PatientID),
                new SqlParameter("@HospitalIDF", hospitalidf)
            };
            var history = await _dbHelper.QueryAsync<BedTransferHistoryModel>("SP_GetAdmissionDischargeWithDetailForBedTransfer",
                          CommandType.StoredProcedure,
                          historyParams);

            foreach (var item in history)
            {
                item.TransferBy = item.UserName;

                switch (item.TransferType)
                {
                    case 0:
                        item.Mode = "-";
                        break;

                    case 1:
                        item.Mode = $"Transferred From Bed {item.ReBedname}";
                        break;

                    case 2:
                        item.Mode = $"Temporary Bed {item.ReBedname}";
                        break;

                    default:
                        item.Mode = "";
                        break;
                }
            }
            response.BedTransferHistory = history;
            return response;
        }
    }
}
