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
        private readonly ITokenDisplayBroadcastService _tokenDisplayBroadcast;
        public OPDRegistrationService(IDbConnectionFactory db, IConfiguration configuration, ITokenDisplayBroadcastService tokenDisplayBroadcast)
        {
            _dbHelper = db;
            _configuration = configuration;
            _tokenDisplayBroadcast = tokenDisplayBroadcast;
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

        public async Task<List<DoctorOPDEntryTokenList>> GetDoctorOPDEntryTokenListAsync(int doctorIdf)
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@DoctorIDF", doctorIdf)
            };
            return await _dbHelper.QueryAsync<DoctorOPDEntryTokenList>(
                "API_Sp_GetDoctorOPDEntryTokenList_WithStatus", CommandType.StoredProcedure, parameters);
        }

        public async Task<List<ConsultingRoom>> GetConsultingRoomListAsync(int hospitalidf)
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@HospitalIDF", hospitalidf)
            };
            return await _dbHelper.QueryAsync<ConsultingRoom>(
                "API_Sp_GetConsultingRoomList", CommandType.StoredProcedure, parameters);
        }

        public async Task<InsertTokenDisplayResult> InsertTokenDisplayAsync(InsertTokenDisplayRequest request, CancellationToken cancellationToken = default)
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@RoomIDF", request.RoomIDF),
                new SqlParameter("@TokenIDF", request.TokenIDF),
                new SqlParameter("@DoctorIDF", request.DoctorIDF)
            };
            var rows = await _dbHelper.QueryAsync<InsertTokenDisplayResult>(
                "API_Sp_InsertTokenDisplay", CommandType.StoredProcedure, parameters);
            var result = rows.FirstOrDefault() ?? new InsertTokenDisplayResult { IsInserted = 0, Message = "No result returned" };
            if (result.IsInserted == 1)
            {
                await _tokenDisplayBroadcast.BroadcastOPDEntryTokenAsync(request.RoomIDF, request.TokenIDF, request.DoctorIDF, cancellationToken);
            }
            return result;
        }
    }
}
