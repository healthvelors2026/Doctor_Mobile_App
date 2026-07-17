using DoctorMobileApp.CommonClass;
using Microsoft.Data.SqlClient;
using System.Data;
using static DoctorMobileApp.Models.KioskModel;

namespace DoctorMobileApp.WebServices
{
    public class KioskService
    {
        private readonly IDbConnectionFactory _dbHelper;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public KioskService(IDbConnectionFactory db, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _dbHelper = db;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }
        //TEST
        public async Task<List<PatientDetail>> GetPatientSearchListAsync(PatientSearchModel searchModel, int hospitalidf)
        {
            var list = new List<PatientDetail>();
            var patientParams = new[]
            {
                new SqlParameter("@MobileNo", searchModel.MobileNo ?? (object)DBNull.Value),
                new SqlParameter("@ABHANo", searchModel.ABHANo ?? (object)DBNull.Value),
                new SqlParameter("@CRNo", searchModel.CRNo ?? (object)DBNull.Value),
                new SqlParameter("@HospitalIDF",hospitalidf)
            };
            list = await _dbHelper.QueryAsync<PatientDetail>("Kiosk_API_PatientSearch", CommandType.StoredProcedure, patientParams);
            return list;
        }
        public async Task<List<SkillSetResponseModel>> GetSkillSetListAsync(int hospitalgroupidf = 0)
        {
            var request = _httpContextAccessor.HttpContext!.Request;

            string baseUrl = $"{request.Scheme}://{request.Host}";

            var list = new List<SkillSetResponseModel>();

            var skillSetParams = new[]
            {
                new SqlParameter("@HospitalGroupIDF", hospitalgroupidf)
            };

            list = await _dbHelper.QueryAsync<SkillSetResponseModel>("API_SP_GetStandardSkillSetList", CommandType.StoredProcedure, skillSetParams);

            foreach (var item in list)
            {
                if (!string.IsNullOrEmpty(item.IconPath))
                {
                    string[] pathParts = item.IconPath.Split('\\');
                    string hospitalCode = "";
                    if (pathParts.Length > 1)
                    {
                        hospitalCode = pathParts[2];
                    }
                    int index = item.IconPath.IndexOf("MobileApp", StringComparison.OrdinalIgnoreCase);
                    if (index >= 0)
                    {
                        string relativePath = item.IconPath.Substring(index).Replace("\\", "/");
                        item.IconPath = $"{baseUrl}/{hospitalCode}/{relativePath}";
                    }
                }
            }
            return list;
        }
        public async Task<GeneratePatientOTPResponseModel> GenerateOTPAsync(GeneratePatientOTPRequestModel requestModel, int hospitalidf)
        {
            var otpParams = new[]
            {
                new SqlParameter("@PatientIDF", requestModel.PatientIDF),
                new SqlParameter("@CRNumber", requestModel.CRNumber),
                new SqlParameter("@MobileNo", requestModel.MobileNo),
                new SqlParameter("@HospitalIDF", hospitalidf)
            };
            var result = await _dbHelper.QueryAsync<GeneratePatientOTPResponseModel>("Kiosk_API_GeneratePatientOTP", CommandType.StoredProcedure, otpParams);
            return result.FirstOrDefault();
        }
        public async Task<VerifyPatientOTPResponseModel> VerifyOTPAsync(VerifyPatientOTPRequestModel requestModel)
        {
            var otpParams = new[]
            {
                    new SqlParameter("@KioskPatientOTPIDP", requestModel.KioskPatientOTPIDP),
                    new SqlParameter("@CRNumber", requestModel.CRNumber),
                    new SqlParameter("@KioskOTP", requestModel.KioskOTP)
            };
            var result = await _dbHelper.QueryAsync<VerifyPatientOTPResponseModel>("Kiosk_API_VerifyPatientOTP", CommandType.StoredProcedure, otpParams);
            return result.FirstOrDefault();
        }
        public async Task<List<PathoReportResponseModel>> GetPathoReportListForPrintAsync(PathoReportRequestModel requestModel, int hospitalidf)
        {
            var list = new List<PathoReportResponseModel>();
            var pathoReportParams = new[]
            {
                 new SqlParameter("@PatientIDF", requestModel.PatientIDF),
                 new SqlParameter("@HospitalIDF", hospitalidf)
            };
            list = await _dbHelper.QueryAsync<PathoReportResponseModel>("Kiosk_API_GetPathologyReportListForPrint", CommandType.StoredProcedure, pathoReportParams);
            return list;
        }
        public async Task<List<OPDTestReceiptResponseModel>> GetOPDTestReceiptListAsync(OPDTestReceiptRequestModel requestModel, int hospitalidf)
        {
            var list = new List<OPDTestReceiptResponseModel>();
            var OPDParams = new[]
            {

                new SqlParameter("@PatientIDF" , requestModel.PatientIDF),
                new SqlParameter("@HospitalIDF", hospitalidf)
            };
            list = await _dbHelper.QueryAsync<OPDTestReceiptResponseModel>("Kiosk_API_OPDTestReceipt_GetList", CommandType.StoredProcedure, OPDParams);
            return list;
        }
        public async Task<int> SaveOPDTestReceiptAsync(SaveOPDTestReceiptRequestModel model, int userIdf, int hospitalidf)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("InvestigationRegistrationIDP", typeof(int));
                dt.Columns.Add("InvestigationType", typeof(int));
                dt.Columns.Add("Rate", typeof(decimal));

                if (model.OPDTestReceiptList == null || !model.OPDTestReceiptList.Any())
                {
                    return 0;
                }
                foreach (var item in model.OPDTestReceiptList)
                {
                    dt.Rows.Add(
                        item.InvestigationRegistrationIDP,
                        item.InvestigationType,
                        item.Rate
                    );
                }
                var tvpParam = new SqlParameter
                {
                    ParameterName = "@KioskOPDTestReceiptTableType",
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "dbo.KioskOPDTestReceiptTableType",
                    Value = dt
                };
                var voucherParam = new SqlParameter("@VoucherIDP_Return", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                var parameters = new SqlParameter[]
                {
                   new SqlParameter("@HospitalIDF",hospitalidf),
                   new SqlParameter("@PatientIDF", model.PatientIDF),
                   new SqlParameter("@OPDRegistrationIDF", model.OPDRegistrationIDF),
                   tvpParam,
                   new SqlParameter("@UserIDF",userIdf),
                   new SqlParameter("@UPITransactionNo",
                   string.IsNullOrWhiteSpace(model.UPITransactionNo)? DBNull.Value: (object)model.UPITransactionNo),
                   voucherParam
                };
                await _dbHelper.ExecuteNonQueryAsync("Kiosk_API_OPDTestReceipt_Save", CommandType.StoredProcedure, parameters);
                return Convert.ToInt32(voucherParam.Value);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<LastVisitDrResponseModel> GetLastVisitDoctorAsync(LastVisitDrRequestmodel requestModel, int hospitalidf)
        {
            var parameters = new[]
            {
                new SqlParameter("@PatientID", requestModel.PatientIDF),
                new SqlParameter("@HospitalID",hospitalidf)
            };

            var result = await _dbHelper.QueryAsync<LastVisitDrResponseModel>("Kiosk_API_GetLastVisitDoctorDetail", CommandType.StoredProcedure, parameters);

            return result.FirstOrDefault();
        }
        public async Task<List<DoctorResponseModel>> GetDoctorListAsync(DoctorRequestModel requestModel, int hospitalidf)
        {
            var list = new List<DoctorResponseModel>();
            var doctorParams = new[]
            {
                new SqlParameter("@HospitalID", hospitalidf),
                new SqlParameter("@SkillSetID", requestModel.SkillSetID)
            };
            list = await _dbHelper.QueryAsync<DoctorResponseModel>("KIOSK_API_GetSkillSetWise_Doctor", CommandType.StoredProcedure, doctorParams);
            return list;
        }
        public async Task<List<PatientLatestAppointmentResponseModel>> GetLatestPatientAppointmentDetailAsync(PatientLatestAppointmentRequestModel requestModel, int hospitalidf)
        {
            var list = new List<PatientLatestAppointmentResponseModel>();
            var AppointmentParams = new[]
            {
                new SqlParameter("@HospitalID", hospitalidf),
                new SqlParameter("@PatientID", requestModel.PatientID)
            };
            list = await _dbHelper.QueryAsync<PatientLatestAppointmentResponseModel>("Kiosk_API_GetPatientLatestAppointmentDetail", CommandType.StoredProcedure, AppointmentParams);
            return list;

        }
        public async Task<int> SaveAdvanceDepositAsync(AdvanceDepositModel model, int hospitalidf, int fasModeOFPaymentIDF, int userIdf)
        {
            try
            {
                var voucherParam = new SqlParameter("@VoucherIDP", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                var parameters = new[]
                {
                     new SqlParameter("@PatientIDF", model.PatientIDF),
                     new SqlParameter("@AdvanceAmount", model.AdvanceAmount),
                     new SqlParameter("@TransactionId",string.IsNullOrEmpty(model.TransactionId)? DBNull.Value: (object)model.TransactionId),
                     new SqlParameter("@HospitalIDF",hospitalidf),
                     new SqlParameter("@ModeOfPaymentIDF", fasModeOFPaymentIDF),
                     new SqlParameter("@Kiosk_UserIDF", userIdf),
                       new SqlParameter("@BrowserName",string.IsNullOrWhiteSpace(model.BrowserName)? DBNull.Value: (object)model.BrowserName),
                    new SqlParameter("@IPAdress",string.IsNullOrWhiteSpace(model.IPAdress)? DBNull.Value: (object)model.IPAdress),
                     voucherParam
                };
                await _dbHelper.ExecuteNonQueryAsync("Kiosk_API_InsertPatientAdvance", CommandType.StoredProcedure, parameters);
                return Convert.ToInt32(voucherParam.Value);
            }
            catch
            {
                return 0;
            }
        }
        public async Task<int> SaveOPDRegistrationAsync(SaveOPDRegistrationModel model,int userIdf,int hospitalidf)
        {
            try
            {
                var voucherParam = new SqlParameter("@VoucherIDP", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@PatientIDF", model.PatientIDF),
                    new SqlParameter("@DoctorIDF", model.DoctorIDF),
                    new SqlParameter("@HospitalIDF", hospitalidf),
                    new SqlParameter("@Kiosk_UserIDF", userIdf),
                    new SqlParameter("@UPITransactionNo",string.IsNullOrWhiteSpace(model.UPITransactionNo)? DBNull.Value: (object)model.UPITransactionNo),
                    new SqlParameter("@BrowserName",string.IsNullOrWhiteSpace(model.BrowserName)? DBNull.Value: (object)model.BrowserName),
                    new SqlParameter("@IPAdress",string.IsNullOrWhiteSpace(model.IPAdress)? DBNull.Value: (object)model.IPAdress),
                    voucherParam

                };

            await _dbHelper.ExecuteNonQueryAsync("Kiosk_API_Insert_OPD_Registration",CommandType.StoredProcedure,parameters);
                return Convert.ToInt32(voucherParam.Value);

            }
            catch
            {
                return 0;
            }
        }
    }
}
