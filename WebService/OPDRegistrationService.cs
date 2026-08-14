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
        public async Task<SaveOPDEntryWithTestResponse?> SaveOPDEntryWithTestAsync(OPDRegistration request, int UserIdf, int hospitalidf, int hospitalgroupidf)
        {
            SqlParameter[] parameters = new
                SqlParameter[]
            {
                new SqlParameter("@OPDRegistrationIDF", request.OPDRegistrationDetails.OPDRegistrationIDP),
                new SqlParameter("@tbPathoRegistrationUDTT", SqlDbType.Structured)
                {
                   TypeName = "dbo.tbIPDTestServiceRegAmtTVP",
                   Value = CreateTestServiceDataTable(request.PathoTestList,request.OPDRegistrationDetails.OPDRegistrationIDP,request.OPDRegistrationDetails.ClassForReimbursement)
                },
                new SqlParameter("@tbRadioRegistrationUDTT", SqlDbType.Structured)
                {
                   TypeName = "dbo.tbIPDTestServiceRegAmtTVP",
                   Value = CreateTestServiceDataTable(request.RadioTestList,request.OPDRegistrationDetails.OPDRegistrationIDP,request.OPDRegistrationDetails.ClassForReimbursement)
                },
                new SqlParameter("@tbMedicalProcedureRegistrationUDTT", SqlDbType.Structured)
                {
                   TypeName = "dbo.tbIPDTestServiceRegAmtTVP",
                   Value = CreateTestServiceDataTable(request.ProcedureTestList,request.OPDRegistrationDetails.OPDRegistrationIDP,request.OPDRegistrationDetails.ClassForReimbursement)
                },
                new SqlParameter("@PathoRemarks", request.OPDRegistrationDetails.PathoRemarks ?? (object)DBNull.Value),
                new SqlParameter("@RadioRemarks", request.OPDRegistrationDetails.RadioRemarks ?? (object)DBNull.Value),
                new SqlParameter("@ProcRemarks", request.OPDRegistrationDetails.ProcRemarks ?? (object)DBNull.Value),
                new SqlParameter("@UserIDF", UserIdf),
                new SqlParameter("@IPAddress", request.OPDRegistrationDetails.IPAddress ?? (object)DBNull.Value),
                new SqlParameter("@BrowserName", request.OPDRegistrationDetails.BrowserName ?? (object)DBNull.Value),
                new SqlParameter("@HospitalIDF", hospitalidf),
                new SqlParameter("@HospitalGroupIDF", hospitalgroupidf)
            };
            var result = await _dbHelper.QueryAsync<SaveOPDEntryWithTestResponse>(
                   "API_SP_InsertUpdateOPDEntryWithTest",
                   CommandType.StoredProcedure,
                   parameters);
            return result.FirstOrDefault();
        }
        private static DataTable CreateTestServiceDataTable(IEnumerable<InvestigationTestReport> list, int nonCashLess, bool classForReimbursement)
        {
            var table = new DataTable();
            table.Columns.Add("TestReportIDF", typeof(int));
            table.Columns.Add("ServiceIDF", typeof(int));
            table.Columns.Add("TestGroupIDF", typeof(int));
            table.Columns.Add("LabFlag", typeof(byte));
            table.Columns.Add("LabIDF", typeof(int));
            table.Columns.Add("NotApplicable", typeof(int));
            table.Columns.Add("IsPortable", typeof(bool));
            table.Columns.Add("CashFlag", typeof(bool));
            table.Columns.Add("Qty", typeof(int));
            table.Columns.Add("Rate", typeof(decimal));
            table.Columns.Add("CostAdditionRate", typeof(decimal));
            table.Columns.Add("DiscPercent", typeof(decimal));
            table.Columns.Add("DiscountAmt", typeof(decimal));
            table.Columns.Add("RoundingAmt", typeof(decimal));
            table.Columns.Add("NetServiceRate", typeof(decimal));
            table.Columns.Add("IsDelete", typeof(int));
            foreach (var item in list)
            {
                item.CashFlag = CheckCashFlag(nonCashLess, item.NotApplicable, classForReimbursement);
                table.Rows.Add(
                     item.TestReportIDP,    // TestReportIDP
                     item.ServiceIDF,       // ServiceIDF
                     item.CategoryIDF,      // TestGroupIDF
                     item.LabFlag,          // LabFlag
                     item.LabIDF,           // LabIDF
                     item.NotApplicable,    // NotApplicable
                     item.IsPortable,       // IsPortable
                     item.CashFlag,         // CashFlag
                     item.Qty,              // Qty
                     item.OriginalAmt,      // Rate
                     item.CostAddRate,      // CostAdditionRate
                     item.DiscountPer,      // DiscPercent
                     item.DiscountAmt,      // DiscountAmt
                     item.RoundingAmt,      // RoundingAmt
                     item.Amount,           // NetServiceRate
                     item.IsSelected        // IsDelete
              );
            }
            return table;
        }
        private static bool CheckCashFlag(int nonCashLess, int na, bool classForReimbursement)
        {
            return nonCashLess != 2
                   || classForReimbursement
                   || na == 1;
        }
    }
}
