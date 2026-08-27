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

        public async Task<InsertTokenDisplayResult> InsertTokenDisplayAsync(InsertTokenDisplayRequest request, bool isDirectSelection, CancellationToken cancellationToken = default)
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@RoomIDF", request.RoomIDF),
                new SqlParameter("@TokenIDF", request.TokenIDF),
                new SqlParameter("@DoctorIDF", request.DoctorIDF),
                new SqlParameter("@IsDirectSelection", isDirectSelection)
            };
            var rows = await _dbHelper.QueryAsync<InsertTokenDisplayResult>(
                "API_Sp_InsertTokenDisplay", CommandType.StoredProcedure, parameters);
            var result = rows.FirstOrDefault() ?? new InsertTokenDisplayResult { IsInserted = 0, Message = "No result returned" };
            if (result.IsInserted == 1)
            {
                if (result.IsPromotion) // true only when a promote happened - one click changed two cells
                {
                    var promotedCRNumber = await GetPatientCRNumberAsync(result.PromotedTokenIDF);
                    var clickedCRNumber = await GetPatientCRNumberAsync(request.TokenIDF);
                    await _tokenDisplayBroadcast.BroadcastOPDEntryTokenAsync(result.PromotedRoomIDF, result.PromotedTokenIDF, request.DoctorIDF, false, promotedCRNumber, cancellationToken); // old Upcoming's value -> update Running cell
                    await _tokenDisplayBroadcast.BroadcastOPDEntryTokenAsync(request.RoomIDF, request.TokenIDF, request.DoctorIDF, true, clickedCRNumber, cancellationToken); // clicked token -> update Upcoming cell
                }
                else
                {
                    var crNumber = await GetPatientCRNumberAsync(request.TokenIDF);
                    await _tokenDisplayBroadcast.BroadcastOPDEntryTokenAsync(request.RoomIDF, request.TokenIDF, request.DoctorIDF, result.IsUpcomingToken, crNumber, cancellationToken);
                }
            }
            return result;
        }

        // Added by Poonam Vasani on 27-Aug-2026 : Purpose: resolve the patient's CR number for a token so the old
        // project's Hub can look up and display a real patient name instead of leaving it blank. Same two join
        // paths as API_Sp_GetDoctorOPDEntryTokenList_WithStatus (voucher-linked or transaction-linked registration).
        private async Task<string?> GetPatientCRNumberAsync(int tokenIdf)
        {
            var parameters = new SqlParameter[] { new SqlParameter("@TokenIDF", tokenIdf) };
            var rows = await _dbHelper.QueryAsync<CRNumberLookup>(
                @"SELECT TOP 1 CRNumber FROM
                  (
                      SELECT PM.CRNumber FROM tbTokenIssueTransaction TIT
                      INNER JOIN tbFASVoucherMaster FVM ON FVM.VoucherIDP = TIT.VoucherIDF AND FVM.RegistrationType = 0 AND FVM.VoucherTypeIDF = 11
                      INNER JOIN tbOPDRegistration OPDReg ON OPDReg.OPDRegistrationIDP = FVM.RegistrationIDF
                      INNER JOIN tbPatientMaster PM ON PM.PatientIDP = OPDReg.PatientIDF
                      WHERE TIT.TokenIssueIDF = @TokenIDF
                      UNION ALL
                      SELECT PM.CRNumber FROM tbTokenIssueTransaction TIT
                      INNER JOIN tbOPDRegistration OPDReg ON OPDReg.OPDRegistrationIDP = TIT.RegistrationIDF
                      INNER JOIN tbPatientMaster PM ON PM.PatientIDP = OPDReg.PatientIDF
                      WHERE TIT.TokenIssueIDF = @TokenIDF
                  ) Combined",
                CommandType.Text, parameters);
            return rows.FirstOrDefault()?.CRNumber;
        }
    }
}
