using DoctorMobileApp.CommonClass;
using DoctorMobileApp.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Transactions;
namespace DoctorMobileApp.WebService
{
    public class DoctorFirstVisitService
    {
        private readonly IDbConnectionFactory _dbHelper;
        private readonly IConfiguration _configuration;
        public DoctorFirstVisitService(IDbConnectionFactory db, IConfiguration configuration)
        {
            _dbHelper = db;
            _configuration = configuration;
        }
        public async Task<DoctorFirstVisit> GetVisitDetailsAsync(VisitDetailsRequest request, int hospitalidf, int hospitalgroupidf)
        {
            var parameters = new[]
            {
                new SqlParameter("@DocVisitIDF", request.DocVisitIDF),
                new SqlParameter("@AdmissionIDF", request.AdmissionIDF),
                new SqlParameter("@DoctorIDF", request.DoctorIDF), 
                new SqlParameter("@HospitalIDF", hospitalidf),
                new SqlParameter("@HospitalGroupIDF", hospitalgroupidf)
            };

            var result = await _dbHelper.QueryMultipleAsync(
                "API_SP_GetFirstVisitDetails",
                new Func<SqlDataReader, object>[]
                {
                    r => ReadRowExtensions.MapToClass<VisitDetails>(r),
                    r => ReadRowExtensions.MapToClass<TabDetail>(r),
                    r => ReadRowExtensions.MapToClass<VisitChargeDetail>(r),
                    r => ReadRowExtensions.MapToClass<DiagnosisModel>(r),
                    r => ReadRowExtensions.MapToClass<Doctor>(r),
                    r => ReadRowExtensions.MapToClass<IDNamePair>(r),
                    r => ReadRowExtensions.MapToClass<VitalDetail>(r),
                    r => ReadRowExtensions.MapToClass<IDNamePair>(r)
                },
                parameters);

            var response = new DoctorFirstVisit
            {
                VisitDetails = ReadSingle<VisitDetails>(result, 0) ?? new VisitDetails(),
                TabDetaillist = ReadList<TabDetail>(result, 1),
                VisitChargeDetail = ReadSingle<VisitChargeDetail>(result, 2) ?? new VisitChargeDetail(),
                DoctorList = ReadList<Doctor>(result, 4),
                VisitTypeList = ReadList<IDNamePair>(result, 5),
                VitalDetails = ReadList<VitalDetail>(result, 6),
                DietCategoryList = ReadList<IDNamePair>(result, 7),
                RegTypeList = new List<IDNamePair>
                {
                  new() { ID = 0, Name = "Normal" },
                  new() { ID = 1, Name = "Day Emer" },
                  new() { ID = 2, Name = "Night Emer" }
                }
            };
            var diagnosisList = ReadList<DiagnosisModel>(result, 3);
            response.ProvisionalDiagnosisList = diagnosisList
                .Where(x => x.DiagnosisType == 0)
                .ToList();
            response.FinalDiagnosisList = diagnosisList
                .Where(x => x.DiagnosisType == 1)
                .ToList();
            return response;
        }
        public async Task<VisitChargeDetail?> GetCalcVisitChargeAsync(VisitChargeRequest request, int hospitalidf, int hospitalgroupidf)
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@VisitTypeIDF", request.VisitTypeIDF),
                new SqlParameter("@ClassIDF", request.ClassIDF),
                new SqlParameter("@NonCashLess", request.NonCashLess),
                new SqlParameter("@Visitdate", request.Visitdate),
                new SqlParameter("@HospitalIDF", hospitalidf),
                new SqlParameter("@HospitalGroupIDF", hospitalgroupidf)
            };
            var dataTable = await _dbHelper.ExecuteDataTableAsync(
                "API_SP_GetCalcVisitCharge",
                CommandType.StoredProcedure,
                parameters
            );
            if (dataTable == null || dataTable.Rows.Count == 0)
                return null;
            var row = dataTable.Rows[0];
            var response = new VisitChargeDetail
            {
                ServiceIDF = row.ToInt("ServiceIDF"),
                OriginalAmt = row.ToInt("OriginalAmt"),
                CostAddition = row.ToInt("CostAddition"),
                DicountPer = row.ToInt("DicountPer"),
                DiscountAmount = row.ToInt("DiscountAmount"),
                VisitAmount = row.ToInt("VisitAmount")
            };
            return response;
        }
        public async Task<List<IDNamePair>> GetFoodListAsync(int DietCategoryIDF, int hospitalgroupidf)
        {
            var list = new List<IDNamePair>();
            var patientParams = new[]
                   {
                new SqlParameter("@DietCategoryIDF", DietCategoryIDF),
                new SqlParameter("@HospitalGroupIDF", hospitalgroupidf),

            };
            list = await _dbHelper.QueryAsync<IDNamePair>(
                "API_SP_GetDietFood",
                CommandType.StoredProcedure,
                patientParams);

            return list;
        }
        public async Task<List<IDNamePair>> GetPathoTesGrouptListAsync(int hospitalidf)
        {
            var list = new List<IDNamePair>();
            var patientParams = new[]
                   {
                new SqlParameter("@HospitalIDF", hospitalidf)

            };
            list = await _dbHelper.QueryAsync<IDNamePair>(
                "API_SP_GetPathoTestGroup",
                CommandType.StoredProcedure,
                patientParams);
            return list;
        }
        public async Task<List<InvestigationTestReport>> GetPathoTestPriceListAsync(
            TestPriceRequest request, int hospitalidf, int hospitalgroupidf)
        {
            var list = new List<InvestigationTestReport>();
            DateTime visitDateValue = string.IsNullOrEmpty(request.Visitdate)
               ? DateTime.Now
               : Convert.ToDateTime(request.Visitdate);
            SqlParameter[] patientParams =
             {
                new SqlParameter("@AdmissionIDF", request.AdmissionIDF),
                new SqlParameter("@ChargeType", request.ChargeType),
                new SqlParameter("@Visitdate", visitDateValue),
                new SqlParameter("@PathoTestGroupIDF", request.CategoryIDF),
                new SqlParameter("@ClassIDF", request.ClassIDF),
                new SqlParameter("@SkillSetIDF", request.SkillSetIDF),
                new SqlParameter("@WardTypeIDF", request.WardTypeIDF),
                new SqlParameter("@BedTrackingIDF", request.BedTrackingIDF),
                new SqlParameter("@SearchTest", request.SearchTest ?? ""),
                new SqlParameter("@HospitalIDF", hospitalidf),
                new SqlParameter("@HospitalGroupIDF", hospitalgroupidf)
            };
            if (request.NonCashLess == 0 || request.NonCashLess == 1)
            {
                list = await _dbHelper.QueryAsync<InvestigationTestReport>(
                  "API_SP_GetNormalPathoTestPriceList",
                  CommandType.StoredProcedure,
                  patientParams);
            }
            else if (request.NonCashLess == 2)
            {
                list = await _dbHelper.QueryAsync<InvestigationTestReport>(
                "API_SP_GetCashlessPathoTestPriceList",
                CommandType.StoredProcedure,
                patientParams);
            }
            return list;
        }
        public async Task<List<InvestigationTestReport>> GetVisitPathoTestListAsync(
           VisitTestRequest request, int hospitalidf, int hospitalgroupidf)
        {
            var list = new List<InvestigationTestReport>();
            SqlParameter[] patientParams =
             {
                new SqlParameter("@AdmissionIDF", request.AdmissionIDF),
                new SqlParameter("@VisitIDF", request.VisitIDF),
                new SqlParameter("@VisitFlag", request.VisitFlag),
                new SqlParameter("@HospitalIDF", hospitalidf),
                new SqlParameter("@HospitalGroupIDF", hospitalgroupidf)
            };
            list = await _dbHelper.QueryAsync<InvestigationTestReport>(
              "API_SP_GetVisitPathoTestList",
              CommandType.StoredProcedure,
              patientParams);
            return list;
        }
        public async Task<List<IDNamePair>> GetRadioCategoryListAsync(int hospitaligroupdf)
        {
            var list = new List<IDNamePair>();
            var patientParams = new[]
                   {
                new SqlParameter("@HospitalGroupIDF", hospitaligroupdf)
            };
            list = await _dbHelper.QueryAsync<IDNamePair>(
                "API_SP_GetRadioCategoryList",
                CommandType.StoredProcedure,
                patientParams);
            return list;
        }
        public async Task<List<InvestigationTestReport>> GetRadioTestPriceListAsync(
            TestPriceRequest request, int hospitalidf, int hospitalgroupidf)
        {
            var list = new List<InvestigationTestReport>();
            DateTime visitDateValue = string.IsNullOrEmpty(request.Visitdate)
               ? DateTime.Now
               : Convert.ToDateTime(request.Visitdate);
            SqlParameter[] patientParams =
             {
                new SqlParameter("@AdmissionIDF", request.AdmissionIDF),
                new SqlParameter("@ChargeType", request.ChargeType),
                new SqlParameter("@Visitdate", visitDateValue),
                new SqlParameter("@RadioCategoryIDF", request.CategoryIDF),
                new SqlParameter("@ClassIDF", request.ClassIDF),
                new SqlParameter("@WardTypeIDF", request.WardTypeIDF),
                new SqlParameter("@BedTrackingIDF", request.BedTrackingIDF),
                new SqlParameter("@SearchTest", request.SearchTest ?? ""),
                new SqlParameter("@HospitalIDF", hospitalidf),
                new SqlParameter("@HospitalGroupIDF", hospitalgroupidf)
            };
            if (request.NonCashLess == 0 || request.NonCashLess == 1)
            {
                list = await _dbHelper.QueryAsync<InvestigationTestReport>(
                  "API_SP_GetNormalRadioTestPriceList",
                  CommandType.StoredProcedure,
                  patientParams);
            }
            else if (request.NonCashLess == 2)
            {
                list = await _dbHelper.QueryAsync<InvestigationTestReport>(
                "API_SP_GetCashlessRadioTestPriceList",
                CommandType.StoredProcedure,
                patientParams);
            }
            return list;
        }
        public async Task<List<InvestigationTestReport>> GetVisitRadioTestListAsync(
          VisitTestRequest request, int hospitalidf, int hospitalgroupidf)
        {
            var list = new List<InvestigationTestReport>();
            SqlParameter[] patientParams =
             {
                new SqlParameter("@AdmissionIDF", request.AdmissionIDF),
                new SqlParameter("@VisitIDF", request.VisitIDF),
                new SqlParameter("@VisitFlag", request.VisitFlag),
                new SqlParameter("@HospitalIDF", hospitalidf),
                new SqlParameter("@HospitalGroupIDF", hospitalgroupidf)
            };
            list = await _dbHelper.QueryAsync<InvestigationTestReport>(
              "API_SP_GetVisitRadioTestList",
              CommandType.StoredProcedure,
              patientParams);
            return list;
        }
        public async Task<List<IDNamePair>> GetProcedureCategoryListAsync(int hospitaligroupdf)
        {
            var list = new List<IDNamePair>();
            var patientParams = new[]
                   {
                new SqlParameter("@HospitalGroupIDF", hospitaligroupdf)
            };
            list = await _dbHelper.QueryAsync<IDNamePair>(
                "API_SP_GetProcedureCategoryList",
                CommandType.StoredProcedure,
                patientParams);
            return list;
        }
        public async Task<List<InvestigationTestReport>> GetProcedureTestPriceListAsync(
          TestPriceRequest request, int hospitalidf, int hospitalgroupidf)
        {
            var list = new List<InvestigationTestReport>();
            DateTime visitDateValue = string.IsNullOrEmpty(request.Visitdate)
               ? DateTime.Now
               : Convert.ToDateTime(request.Visitdate);
            SqlParameter[] patientParams =
             {
                new SqlParameter("@AdmissionIDF", request.AdmissionIDF),
                new SqlParameter("@ChargeType", request.ChargeType),
                new SqlParameter("@Visitdate", visitDateValue),
                new SqlParameter("@ProcedureCategoryIDF", request.CategoryIDF),
                new SqlParameter("@ClassIDF", request.ClassIDF),
                new SqlParameter("@WardTypeIDF", request.WardTypeIDF),
                new SqlParameter("@BedTrackingIDF", request.BedTrackingIDF),
                new SqlParameter("@SearchTest", request.SearchTest ?? ""),
                new SqlParameter("@HospitalIDF", hospitalidf),
                new SqlParameter("@HospitalGroupIDF", hospitalgroupidf)
            };
            if (request.NonCashLess == 0 || request.NonCashLess == 1)
            {
                list = await _dbHelper.QueryAsync<InvestigationTestReport>(
                  "API_SP_GetNormalProcedureTestPriceList",
                  CommandType.StoredProcedure,
                  patientParams);
            }
            else if (request.NonCashLess == 2)
            {
                list = await _dbHelper.QueryAsync<InvestigationTestReport>(
                   "API_SP_GetCashlessProcedureTestPriceList",
                   CommandType.StoredProcedure,
                   patientParams);
            }
            return list;
        }
        public async Task<List<InvestigationTestReport>> GetVisitProcedureTestListAsync(
         VisitTestRequest request, int hospitalidf, int hospitalgroupidf)
        {
            var list = new List<InvestigationTestReport>();
            SqlParameter[] patientParams =
             {
                new SqlParameter("@AdmissionIDF", request.AdmissionIDF),
                new SqlParameter("@VisitIDF", request.VisitIDF),
                new SqlParameter("@VisitFlag", request.VisitFlag),
                new SqlParameter("@HospitalIDF", hospitalidf),
                new SqlParameter("@HospitalGroupIDF", hospitalgroupidf)
            };
            list = await _dbHelper.QueryAsync<InvestigationTestReport>(
              "API_SP_GetVisitProcedureTestList",
              CommandType.StoredProcedure,
              patientParams);
            return list;
        }
        public async Task<string> SaveVisitAsync(DoctorFirstVisit model)
        {
            using TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                DataTable tbIPDTestServiceRegAmtTVP = CreateTestServiceRegAmtTVPTable();
                // 🔹 1. Save Doctor Visit (Get VisitId)
                #region Insert-Update Doctor Visit
                var VisitDetails = model.VisitDetails;
                var VisitChargeDetail = model.VisitChargeDetail;
                SqlParameter[] parameters =
                {
                            new("@DocVisitIDP", SqlDbType.Int) { Direction = ParameterDirection.InputOutput, Value = VisitDetails.DocVisitIDP },
                            new("@VisitCode", SqlDbType.NVarChar) { Value = VisitDetails.VisitCode },
                            new("@VisitType", SqlDbType.Int) { Value = VisitDetails.VisitType},
                            new("@VisitDateTime", SqlDbType.DateTime) { Value = DateTime.Now },
                            new("@AdmissionIDF", SqlDbType.Int) { Value = VisitDetails.AdmissionIDF },
                            new("@DoctorIDF", SqlDbType.Int) { Value = VisitDetails.DoctorIDF },
                            new("@PatientIDF", SqlDbType.Int) { Value = VisitDetails.PatientIDF },
                            new("@BedIDF", SqlDbType.Int) { Value = VisitDetails.BedIDF },
                            new("@WardIDF", SqlDbType.Int) { Value = VisitDetails.WardIDF },
                            new("@Complaints", SqlDbType.NVarChar) { Value = VisitDetails.Complaints },
                            new("@FindingAndSuggestions", SqlDbType.NVarChar) { Value = VisitDetails.FindingAndSuggestions },
                            new("@DietOrOtherInstruction", SqlDbType.NVarChar) { Value = VisitDetails.DietOrOtherInstruction },
                            new("@ChargeType", SqlDbType.Int) { Value = VisitDetails.ChargeType },
                            new("@VisitCharge", SqlDbType.Decimal) { Value = VisitChargeDetail.VisitAmount },
                            new("@DietCategoryIDF", SqlDbType.Int) { Value = VisitDetails.DietCategoryIDF == 0 ? DBNull.Value : VisitDetails.DietCategoryIDF },
                            new("@DietFoodIDF", SqlDbType.Int) { Value = VisitDetails.DietFoodIDF == 0 ? DBNull.Value : VisitDetails.DietFoodIDF },
                            new("@Advice", SqlDbType.NVarChar) { Value = VisitDetails.Advice },
                            new("@IncludedInPackage", SqlDbType.TinyInt) { Value = 0 },
                            new("@Urgent", SqlDbType.Bit) { Value = VisitDetails.Urgent },
                            new("@CheckedByIDF", SqlDbType.Int) { Value = VisitDetails.DoctorIDF },
                            new("@UserIDF", SqlDbType.Int) { Value = VisitDetails.UserIDF },
                            new("@HospitalIDF", SqlDbType.Int) { Value = VisitDetails.HospitalIDF },
                            new("@EntryDateTime", SqlDbType.DateTime) { Value = DateTime.Now },
                            new("@Planofcare", SqlDbType.NVarChar) { Value = VisitDetails.Planofcare },
                            new("@ReferDocIDF", SqlDbType.Int) { Value = VisitDetails.ReferDocIDF == 0 ? DBNull.Value : VisitDetails.ReferDocIDF },
                            new("@ReferDocVisitDate", SqlDbType.DateTime) { Value = VisitDetails.ReferDocIDF > 0 ? VisitDetails.ReferDocVisitDate : DBNull.Value },
                            new("@ReferDocArrivalDatetime", SqlDbType.DateTime) { Value = VisitDetails.ReferDocIDF > 0 ? VisitDetails.ReferDocArrivalDatetime : DBNull.Value },
                            new("@ReferDocRemarks", SqlDbType.NVarChar) { Value = VisitDetails.ReferDocIDF > 0 ? VisitDetails.ReferDocRemarks : DBNull.Value },
                            new("@BedTrackingIDF", SqlDbType.Int) { Value = VisitDetails.BedTrackingIDF == 0 ? DBNull.Value : VisitDetails.BedTrackingIDF },
                            new("@IPAddress", SqlDbType.NVarChar) { Value = VisitDetails.IPAddress},
                            new("@BrowserName", SqlDbType.NVarChar) { Value = VisitDetails.BrowserName },
                            new("@IsBackDatedIPDDrVisit", SqlDbType.Bit) { Value = false },
                            new("@PainMeasurementScaleLevel", SqlDbType.Int) { Value = VisitDetails.PainMeasurementScaleLevel },
                            new("@OriginalAmt", SqlDbType.Decimal) { Value = VisitChargeDetail.OriginalAmt },
                            new("@CostAddition", SqlDbType.Decimal) { Value = VisitChargeDetail.CostAddition },
                            new("@DiscLedgerIDF", SqlDbType.Int) { Value = VisitDetails.DiscLedgerIDF == 0 ? DBNull.Value : VisitDetails.DiscLedgerIDF },
                            new("@DiscountAmt", SqlDbType.Decimal) { Value = VisitChargeDetail.DiscountAmount},
                            new("@DiscPercent", SqlDbType.Decimal) { Value = VisitChargeDetail.DicountPer},
                            new("@EditUserIDF", SqlDbType.Int) { Value = VisitDetails.DocVisitIDP != 0 ? VisitDetails.UserIDF : DBNull.Value },
                            new("@EditEntryDateTime", SqlDbType.DateTime) { Value = VisitDetails.DocVisitIDP != 0 ? DateTime.Now: DBNull.Value },
                            new("@EditIPAddress", SqlDbType.NVarChar) { Value = VisitDetails.DocVisitIDP != 0 ? VisitDetails.EditIPAddress : DBNull.Value },
                            new("@EditBrowserName", SqlDbType.NVarChar) { Value = VisitDetails.DocVisitIDP != 0 ? VisitDetails.EditBrowserName : DBNull.Value }
                        };
                await _dbHelper.ExecuteNonQueryAsync("API_SP_InsertUpdateDocVisit", CommandType.StoredProcedure, parameters);
                VisitDetails.DocVisitIDP = Convert.ToInt32(parameters[0].Value);
                #endregion
                #region Insert-Update FirstVisit Details
                if (model.TabDetaillist != null && model.TabDetaillist.Count > 0)
                {
                    DataTable tbIPDFirstVisitDetailsTVP = new DataTable();
                    tbIPDFirstVisitDetailsTVP.Columns.Add("DocVisitIDF", typeof(int));
                    tbIPDFirstVisitDetailsTVP.Columns.Add("ReferenceIDF", typeof(int));
                    tbIPDFirstVisitDetailsTVP.Columns.Add("ReferenceType", typeof(byte));
                    tbIPDFirstVisitDetailsTVP.Columns.Add("Answer", typeof(string));
                    foreach (var item in model.TabDetaillist)
                    {
                        tbIPDFirstVisitDetailsTVP.Rows.Add(
                            VisitDetails.DocVisitIDP,
                            item.SubCategoryIDP,
                            item.ReferenceType,
                            item.Answer ?? (object)DBNull.Value
                        );
                    }
                    SqlParameter[] tvpParam =
                    {
                     new("@FirstVisitDetails", SqlDbType.Structured) {TypeName = "dbo.tbIPDFirstVisitDetailsTVP", Value =tbIPDFirstVisitDetailsTVP }
                    };
                    await _dbHelper.ExecuteNonQueryAsync("API_SP_InsertUpdateFirstVisitDetails", CommandType.StoredProcedure, tvpParam);
                }
                #endregion
                #region Insert-update Vital Details
                if (model.VitalDetails != null && model.VitalDetails.Count > 0)
                {
                    DataTable tbIPDVisitVitalDetailsTVP = new DataTable();
                    tbIPDVisitVitalDetailsTVP.Columns.Add("ReferenceIDF", typeof(int));
                    tbIPDVisitVitalDetailsTVP.Columns.Add("ReferenceType", typeof(byte));
                    tbIPDVisitVitalDetailsTVP.Columns.Add("VitalRecordsIDF", typeof(int));
                    tbIPDVisitVitalDetailsTVP.Columns.Add("Value", typeof(string));
                    foreach (var item in model.VitalDetails)
                    {
                        tbIPDVisitVitalDetailsTVP.Rows.Add(
                            VisitDetails.DocVisitIDP,
                            0,
                            item.VitalRecordIDP,
                            item.Answer ?? (object)DBNull.Value
                        );
                    }
                    SqlParameter[] parametersTab =
                    {
                     new("@VisitVitalDetails", SqlDbType.Structured) {TypeName = "dbo.tbIPDVisitVitalDetailsTVP", Value =tbIPDVisitVitalDetailsTVP}
                    };
                    await _dbHelper.ExecuteNonQueryAsync("API_SP_InsertUpdateVitalDetail", CommandType.StoredProcedure, parametersTab);
                }
                #endregion
                #region Insert-Update-Delete Pathology Test
                if (model.PathoTestList != null && model.PathoTestList.Count > 0)
                {
                    tbIPDTestServiceRegAmtTVP.Clear();
                    foreach (var item in model.PathoTestList)
                    {
                        item.CashFlag = CheckCashFlag(VisitDetails.NonCashLess, item.NotApplicable, VisitDetails.ClassForReimbursement);
                        tbIPDTestServiceRegAmtTVP.Rows.Add(
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
                    SqlParameter[] parameterspathoTest =
                    {
                     new SqlParameter("@RegistrationIDF", VisitDetails.AdmissionIDF),
                     new SqlParameter("@OPDIPDFlag",1),
                     new SqlParameter("@VisitIDF",VisitDetails.DocVisitIDP),
                     new SqlParameter("@VisitFlag",0),
                     new SqlParameter("@DiscLedgerIDF", VisitDetails.DiscLedgerIDF),
                     new SqlParameter("@Remarks",VisitDetails.PathoRemarks),
                     new SqlParameter("@UserIDF",VisitDetails.UserIDF),
                     new SqlParameter("@HospitalIDF",VisitDetails.HospitalIDF),
                     new SqlParameter("@HospitalGroupIDF",VisitDetails.HospitalGroupIDF),
                     new SqlParameter("@tbIPDTestServiceRegAmtTVP", SqlDbType.Structured)
                     {
                        TypeName = "dbo.tbIPDTestServiceRegAmtTVP",
                        Value = tbIPDTestServiceRegAmtTVP
                     }
                     };
                    await _dbHelper.ExecuteNonQueryAsync("API_SP_InsertUpdateTestPathoServiceRegAmt", CommandType.StoredProcedure, parameterspathoTest);
                }

                #endregion
                #region Insert-Update-Delete Radiology Test
                if (model.RadioTestList != null && model.RadioTestList.Count > 0)
                {
                    tbIPDTestServiceRegAmtTVP.Clear();
                    foreach (var item in model.RadioTestList)
                    {
                        if (item.IsPortable)//Test Is Portable
                        {
                            item.OriginalAmt = item.PortableOriginalAmt;
                            item.DiscountAmt = item.PortableDiscountAmt;
                            item.RoundingAmt = item.PortableRoundingAmt;
                            item.Amount = item.PortableAmount;
                        }
                        item.CashFlag = CheckCashFlag(VisitDetails.NonCashLess, item.NotApplicable, VisitDetails.ClassForReimbursement);
                        tbIPDTestServiceRegAmtTVP.Rows.Add(
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
                    SqlParameter[] parameterspathoTest =
                    {
                     new SqlParameter("@RegistrationIDF", VisitDetails.AdmissionIDF),
                     new SqlParameter("@OPDIPDFlag",1),
                     new SqlParameter("@VisitIDF",VisitDetails.DocVisitIDP),
                     new SqlParameter("@VisitFlag",0),
                     new SqlParameter("@DiscLedgerIDF", VisitDetails.DiscLedgerIDF),
                     new SqlParameter("@Remarks",VisitDetails.RadioRemarks),
                     new SqlParameter("@UserIDF",VisitDetails.UserIDF),
                     new SqlParameter("@HospitalIDF",VisitDetails.HospitalIDF),
                     new SqlParameter("@HospitalGroupIDF",VisitDetails.HospitalGroupIDF),
                     new SqlParameter("@tbIPDTestServiceRegAmtTVP", SqlDbType.Structured)
                     {
                        TypeName = "dbo.tbIPDTestServiceRegAmtTVP",
                        Value = tbIPDTestServiceRegAmtTVP
                     }
                     };
                    await _dbHelper.ExecuteNonQueryAsync("API_SP_InsertUpdateTestRadioServiceRegAmt", CommandType.StoredProcedure, parameterspathoTest);
                }
                #endregion
                #region Insert-Update-Delete Procedure Test
                if (model.ProcedureTestList != null && model.ProcedureTestList.Count > 0)
                {
                    tbIPDTestServiceRegAmtTVP.Clear();
                    foreach (var item in model.ProcedureTestList)
                    {
                        item.CashFlag = CheckCashFlag(VisitDetails.NonCashLess, item.NotApplicable, VisitDetails.ClassForReimbursement);
                        tbIPDTestServiceRegAmtTVP.Rows.Add(
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
                    SqlParameter[] parameterspathoTest =
                    {
                      new SqlParameter("@RegistrationIDF", VisitDetails.AdmissionIDF),
                      new SqlParameter("@OPDIPDFlag",1),
                      new SqlParameter("@VisitIDF",VisitDetails.DocVisitIDP),
                      new SqlParameter("@VisitFlag",0),
                      new SqlParameter("@DiscLedgerIDF", VisitDetails.DiscLedgerIDF),
                      new SqlParameter("@Remarks",VisitDetails.ProcRemarks),
                      new SqlParameter("@UserIDF",VisitDetails.UserIDF),
                      new SqlParameter("@HospitalIDF",VisitDetails.HospitalIDF),
                      new SqlParameter("@HospitalGroupIDF",VisitDetails.HospitalGroupIDF),
                      new SqlParameter("@tbIPDTestServiceRegAmtTVP", SqlDbType.Structured)
                      {
                       TypeName = "dbo.tbIPDTestServiceRegAmtTVP",
                       Value = tbIPDTestServiceRegAmtTVP
                     }
                  };
                    await _dbHelper.ExecuteNonQueryAsync("API_SP_InsertUpdateTestProcedureServiceRegAmt", CommandType.StoredProcedure, parameterspathoTest);
                }
                #endregion
                // ✅ Commit everything
                scope.Complete();
                return "All Data Saved Successfully";
            }
            catch (Exception ex)
            {
                // ❌ Rollback everything if any step fails
                return "Transaction Failed: " + ex.Message;
            }
        }
        public async Task<DoctorFirstVisit> GetRoutineVisitDetailsAsync(VisitDetailsRequest request, int hospitalidf, int hospitalgroupidf)
        {
            var parameters = new[]
            {
                new SqlParameter("@DocVisitIDF", request.DocVisitIDF),
                new SqlParameter("@AdmissionIDF", request.AdmissionIDF),
                new SqlParameter("@DoctorIDF", request.DoctorIDF), 
                new SqlParameter("@HospitalIDF", hospitalidf),
                new SqlParameter("@HospitalGroupIDF", hospitalgroupidf)
            };

            var result = await _dbHelper.QueryMultipleAsync(
                "API_SP_GetRoutineVisitDetails",
                new Func<SqlDataReader, object>[]
                {
                    r => ReadRowExtensions.MapToClass<VisitDetails>(r),
                    r => ReadRowExtensions.MapToClass<VisitChargeDetail>(r),
                    r => ReadRowExtensions.MapToClass<DiagnosisModel>(r),
                    r => ReadRowExtensions.MapToClass<Doctor>(r),
                    r => ReadRowExtensions.MapToClass<IDNamePair>(r),
                    r => ReadRowExtensions.MapToClass<VitalDetail>(r),
                    r => ReadRowExtensions.MapToClass<IDNamePair>(r)
                },
                parameters);

            var response = new DoctorFirstVisit
            {
                VisitDetails = ReadSingle<VisitDetails>(result, 0) ?? new VisitDetails(),
                VisitChargeDetail = ReadSingle<VisitChargeDetail>(result, 1) ?? new VisitChargeDetail(),
                DoctorList = ReadList<Doctor>(result,3),
                VisitTypeList = ReadList<IDNamePair>(result, 4),
                VitalDetails = ReadList<VitalDetail>(result, 5),
                DietCategoryList = ReadList<IDNamePair>(result, 6),
                RegTypeList = new List<IDNamePair>
                {
                  new() { ID = 0, Name = "Normal" },
                  new() { ID = 1, Name = "Day Emer" },
                  new() { ID = 2, Name = "Night Emer" }
                }
            };
            var diagnosisList = ReadList<DiagnosisModel>(result, 2);
            response.ProvisionalDiagnosisList = diagnosisList
                .Where(x => x.DiagnosisType == 0)
                .ToList();
            response.FinalDiagnosisList = diagnosisList
                .Where(x => x.DiagnosisType == 1)
                .ToList();
            return response;
        }
        public bool CheckCashFlag(int nonCashLess, int na, bool classForReimbursement)
        {
            return nonCashLess != Convert.ToByte(2)//EnmCashLess=2
                   || classForReimbursement
                   || na == 1;
        }
        private DataTable CreateTestServiceRegAmtTVPTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("TestReportIDF", typeof(int));
            dt.Columns.Add("ServiceIDF", typeof(int));
            dt.Columns.Add("TestGroupIDF", typeof(int));
            dt.Columns.Add("LabFlag", typeof(byte));
            dt.Columns.Add("LabIDF", typeof(int));
            dt.Columns.Add("NotApplicable", typeof(int));
            dt.Columns.Add("IsPortable", typeof(bool));
            dt.Columns.Add("CashFlag", typeof(bool));
            dt.Columns.Add("Qty", typeof(int));
            dt.Columns.Add("Rate", typeof(decimal));
            dt.Columns.Add("CostAdditionRate", typeof(decimal));
            dt.Columns.Add("DiscPercent", typeof(decimal));
            dt.Columns.Add("DiscountAmt", typeof(decimal));
            dt.Columns.Add("RoundingAmt", typeof(decimal));
            dt.Columns.Add("NetServiceRate", typeof(decimal));
            dt.Columns.Add("IsDelete", typeof(int));
            return dt;
        }
        private static T? ReadSingle<T>(List<object> result, int index) where T : class
        {
            if (result.Count <= index)
                return null;

            return (result[index] as List<object>)?
                .Cast<T>()
                .FirstOrDefault();
        }
        private static List<T> ReadList<T>(List<object> result, int index)
        {
            if (result.Count <= index)
                return new List<T>();

            return (result[index] as List<object>)?
                .Cast<T>()
                .ToList() ?? new List<T>();
        }
    }
}
