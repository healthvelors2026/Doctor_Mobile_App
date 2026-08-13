using DoctorMobileApp.CommonClass;
using DoctorMobileApp.Models;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;

namespace DoctorMobileApp.WebService
{
    public class OPDRegistrationService
    {
        private readonly IDbConnectionFactory _dbHelper;
        private readonly IConfiguration _configuration;
        public OPDRegistrationService(IDbConnectionFactory db, IConfiguration configuration)
        {
            _dbHelper = db;
            _configuration = configuration;
        }
        public async Task<OPDRegistration?> GetOPDRegistrationDetailsAsync(OPDRegistrationDetailsRequest request, int hospitalidf, int hospitalgroupidf)
        {
            //// Get OPD Registration Details With Test Details
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@RegistrationCode", request.RegistrationCode),
                new SqlParameter("@HospitalIDF", hospitalidf),
                new SqlParameter("@HospitalGroupIDF", hospitalgroupidf)
            };
            var result = await _dbHelper.QueryMultipleAsync(
                "API_SP_GetOPDRegistrationFullDetails",
                new Func<SqlDataReader, object>[]
                {
                    r => ReadRowExtensions.MapToClass<OPDRegistrationDetails>(r),
                    r => ReadRowExtensions.MapToClass<TodayInvestigations>(r),
                    r => ReadRowExtensions.MapToClass<InvestigationTestReport>(r),
                    r => ReadRowExtensions.MapToClass<InvestigationTestReport>(r),
                    r => ReadRowExtensions.MapToClass<InvestigationTestReport>(r)
                },
                parameters);
            var response = new OPDRegistration
            {
                OPDRegistrationDetails = ReadRowExtensions.ReadSingle<OPDRegistrationDetails>(result, 0) ?? new OPDRegistrationDetails(),
                TodayInvestigations = ReadRowExtensions.ReadSingle<TodayInvestigations>(result, 1) ?? new TodayInvestigations(),
                PathoTestList = ReadRowExtensions.ReadList<InvestigationTestReport>(result, 2),
                RadioTestList = ReadRowExtensions.ReadList<InvestigationTestReport>(result, 3),
                ProcedureTestList = ReadRowExtensions.ReadList<InvestigationTestReport>(result, 4)
            };
            return response;
        }
    }
}
