using DoctorMobileApp.CommonClass;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Net;
using static DoctorMobileApp.Models.KioskModel;

namespace DoctorMobileApp.WebServices
{
    public class KioskService
    {
        private readonly IDbConnectionFactory _dbHelper;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;


        public KioskService(IDbConnectionFactory db, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, HttpClient httpClient)
        {
            _dbHelper = db;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClient;
        }
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
        public async Task<List<SkillSetResponseModel>> GetSkillSetListAsync(int hospitalgroupidf, string hospitalCode, string baseUrl, CancellationToken cancellationToken = default)
        {
            var skillSetParams = new[]
            {
                new SqlParameter("@HospitalGroupIDF", hospitalgroupidf)
            };

            cancellationToken.ThrowIfCancellationRequested();

            var list = await _dbHelper.QueryAsync<SkillSetResponseModel>("API_SP_GetStandardSkillSetList", CommandType.StoredProcedure, skillSetParams);

            cancellationToken.ThrowIfCancellationRequested();

            foreach (var item in list)
            {
                if (string.IsNullOrWhiteSpace(item.IconPath) || string.IsNullOrWhiteSpace(hospitalCode) || string.IsNullOrWhiteSpace(baseUrl))
                {
                    item.IconPath = null;
                    continue;
                }

                string fileName;
                try
                {
                    fileName = Path.GetFileName(item.IconPath);
                }
                catch (ArgumentException)
                {
                    item.IconPath = null;
                    continue;
                }

                string extension = Path.GetExtension(fileName);
                string nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

                bool hasSupportedExtension =
                    extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".gif", StringComparison.OrdinalIgnoreCase);

                bool hasValidFileName =
                    !string.IsNullOrWhiteSpace(nameWithoutExtension) &&
                    nameWithoutExtension.All(character =>
                        char.IsLetterOrDigit(character) || character == '_' || character == '-');

                item.IconPath = hasSupportedExtension && hasValidFileName
                    ? $"{baseUrl.TrimEnd('/')}/{hospitalCode}/MobileApp/DoctorSkillset/{fileName}"
                    : null;
            }

            return list;
        }
        public async Task<GeneratePatientOTPResponseModel?> GenerateOTPAsync(GeneratePatientOTPRequestModel requestModel, int hospitalidf)
        {
            var otpParams = new[]
            {
              new SqlParameter("@PatientIDF", requestModel.PatientIDF),
              new SqlParameter("@CRNumber",requestModel.CRNumber ?? (object)DBNull.Value),
              new SqlParameter("@MobileNo",requestModel.MobileNo ?? (object)DBNull.Value),
              new SqlParameter("@HospitalIDF", hospitalidf)
            };

            var results = await _dbHelper.QueryAsync<GeneratePatientOTPResponseModel>("Kiosk_API_GeneratePatientOTP", CommandType.StoredProcedure, otpParams);
            var result = results.FirstOrDefault();
            if (result == null)
            {
                return null;
            }

            string? generatedOtp = result.OTP;

            if (string.IsNullOrWhiteSpace(result.MobileNo) || string.IsNullOrWhiteSpace(generatedOtp))
            {
                result.Message = "OTP generated but SMS could not be sent.";
                return result;
            }
            bool smsSent = false;
            if (System.Diagnostics.Debugger.IsAttached == false)
                smsSent = await SendOtpAsync(result.MobileNo, generatedOtp, hospitalidf);

            result.Message = smsSent ? $"OTP sent successfully on {MaskMobileNumber(result.MobileNo)}" : "OTP generated but SMS could not be sent.";

            return result;
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
        public async Task<SaveOPDTestReceiptResponseModel> SaveOPDTestReceiptAsync(SaveOPDTestReceiptRequestModel model, int userIdf, int hospitalidf)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("InvestigationRegistrationIDP", typeof(int));
                dt.Columns.Add("InvestigationType", typeof(int));
                dt.Columns.Add("Rate", typeof(decimal));

                if (model.OPDTestReceiptList == null || !model.OPDTestReceiptList.Any())
                {
                    return new SaveOPDTestReceiptResponseModel();
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
                var voucherNAParam = new SqlParameter("@VoucherIDP_NA_Return", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                var voucherNumberParam = new SqlParameter("@VoucherNumber_Return", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output };
                var voucherNumberNAParam = new SqlParameter("@VoucherNumber_NA_Return", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output };
                var parameters = new SqlParameter[]
                {
                   new SqlParameter("@HospitalIDF",hospitalidf),
                   new SqlParameter("@PatientIDF", model.PatientIDF),
                   new SqlParameter("@OPDRegistrationIDF", model.OPDRegistrationIDF),tvpParam,
                   new SqlParameter("@UserIDF",userIdf),
                   new SqlParameter("@UPITransactionNo",
                   string.IsNullOrWhiteSpace(model.UPITransactionNo)? DBNull.Value: (object)model.UPITransactionNo),
                   voucherParam,
                   voucherNAParam,
                   voucherNumberParam,
                   voucherNumberNAParam
                };
                await _dbHelper.ExecuteNonQueryAsync("Kiosk_API_OPDTestReceipt_Save", CommandType.StoredProcedure, parameters);
                int voucherId = voucherParam.Value == null || voucherParam.Value == DBNull.Value ? 0 : Convert.ToInt32(voucherParam.Value);
                int voucherIdNA = voucherNAParam.Value == null || voucherNAParam.Value == DBNull.Value ? 0 : Convert.ToInt32(voucherNAParam.Value);
                string voucherNumber = voucherNumberParam.Value == null || voucherNumberParam.Value == DBNull.Value ? string.Empty : Convert.ToString(voucherNumberParam.Value) ?? string.Empty;
                string voucherNumberNA = voucherNumberNAParam.Value == null || voucherNumberNAParam.Value == DBNull.Value ? string.Empty : Convert.ToString(voucherNumberNAParam.Value) ?? string.Empty;
                var receiptDetail = voucherId > 0 ? await GetVoucherResultAsync(voucherId) : null;
                var notApplicableReceiptDetail = voucherIdNA > 0 ? await GetVoucherResultAsync(voucherIdNA) : null;
                return new SaveOPDTestReceiptResponseModel
                {
                    VoucherIDP = voucherId,
                    VoucherIDP_NA = voucherIdNA,
                    VoucherNumber = voucherNumber,
                    VoucherNumber_NA = voucherNumberNA,
                    ReceiptDetail = receiptDetail,
                    NotApplicableReceiptDetail = notApplicableReceiptDetail
                };
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
                new SqlParameter("@SkillSetID", requestModel.SkillSetID),
                new SqlParameter("@PatientID", requestModel.PatientID)
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
        public async Task<SaveOPDRegistrationReceiptResponseModel?> SaveAdvanceDepositAsync(AdvanceDepositModel model, int hospitalidf, int fasModeOFPaymentIDF, int userIdf)
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

                int voucherId = voucherParam.Value == DBNull.Value ? 0 : Convert.ToInt32(voucherParam.Value);

                if (voucherId <= 0)
                    return null;

                return await GetVoucherResultAsync(voucherId);
            }
            catch
            {
                return null;
            }
        }
        public async Task<SaveOPDRegistrationReceiptResponseModel?> SaveOPDRegistrationAsync(SaveOPDRegistrationModel model, int userIdf, int hospitalidf)
        {
            try
            {
                var voucherParam = new SqlParameter("@VoucherIDP", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                var parameters = new SqlParameter[]
                {
                    new("@PatientIDF", model.PatientIDF),
                    new("@DoctorIDF", model.DoctorIDF),
                    new("@HospitalIDF", hospitalidf),
                    new("@Kiosk_UserIDF", userIdf),
                    new("@UPITransactionNo",string.IsNullOrWhiteSpace(model.UPITransactionNo)? DBNull.Value: model.UPITransactionNo),
                    new("@BrowserName",string.IsNullOrWhiteSpace(model.BrowserName)? DBNull.Value: model.BrowserName),
                    new("@IPAdress",string.IsNullOrWhiteSpace(model.IPAdress)? DBNull.Value: model.IPAdress),voucherParam
                };

                await _dbHelper.ExecuteNonQueryAsync("Kiosk_API_Insert_OPD_Registration", CommandType.StoredProcedure, parameters);
                int voucherId = Convert.ToInt32(voucherParam.Value);

                if (voucherId <= 0)
                    return null;

                return await GetVoucherResultAsync(voucherId);
            }
            catch
            {
                return null;
            }
        }
        public async Task<List<KioskBannerResponseModel>> GetActiveKioskBannersAsync(int hospitalidf, string hospitalCode, string baseUrl)
        {
            var parameters = new[] { new SqlParameter("@HospitalIDP", hospitalidf) };
            var list = await _dbHelper.QueryAsync<KioskBannerResponseModel>("Kiosk_API_GetActiveKioskBanners_GetList", CommandType.StoredProcedure, parameters);

            foreach (var item in list)
            {
                if (string.IsNullOrWhiteSpace(item.KioskBannerPath) || string.IsNullOrWhiteSpace(hospitalCode) || string.IsNullOrWhiteSpace(baseUrl))
                {
                    item.BannerImageUrl = null;
                    continue;
                }
                string fileName = Path.GetFileName(item.KioskBannerPath);
                string extension = Path.GetExtension(fileName);

                bool validExtension =
                    extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
                    extension.Equals(".gif", StringComparison.OrdinalIgnoreCase);

                item.BannerImageUrl = validExtension ? $"{baseUrl.TrimEnd('/')}/{hospitalCode}/Kiosk/KioskBanners/{fileName}" : null;
            }
            return list;
        }
        public async Task<SaveOPDRegistrationReceiptResponseModel?> GetVoucherResultAsync(int voucherId)
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@VoucherIDP", voucherId)
            };

            var results = await _dbHelper.QueryAsync<SaveOPDRegistrationReceiptResponseModel>("Kiosk_API_Get_OPD_Registration_Receipt_Result", CommandType.StoredProcedure, parameters);

            return results.FirstOrDefault();
        }
        public async Task<bool> SendOtpAsync(string mobileNo, string otp, int hospitalidf)
        {

            string? smsUrl = _configuration["AppSettings:SMSUrl"];
            string? smsTemplate = _configuration["AppSettings:SMSTemplate"];
            if (!string.IsNullOrEmpty(smsUrl) && !string.IsNullOrEmpty(smsTemplate))
            {
                return false;
            }
            try
            {
                var configurationParameters = new[]
                {
                    new SqlParameter("@HospitalIDF", hospitalidf)
            };
                var configurations = await _dbHelper.QueryAsync<SmsConfigurationModel>(
                        @"SELECT URL, SMSText FROM tbSMSConfiguration INNER JOIN tbSMSConfigurationDetail
                        ON SMSConfigurationIDP = SMSConfigurationIDF WHERE ConfigureType = 86
                        AND HospitalIDF = @HospitalIDF", CommandType.Text, configurationParameters);

                var databaseConfiguration = configurations.FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(databaseConfiguration?.URL) && !string.IsNullOrWhiteSpace(databaseConfiguration.SMSText))
                {
                    smsUrl = databaseConfiguration.URL;
                    smsTemplate = databaseConfiguration.SMSText;
                }
            }
            catch (Exception ex)
            {
                _dbHelper.LogError(ex, "GetSmsConfiguration", new[] { new SqlParameter("@HospitalIDF", hospitalidf) });
            }

            if (string.IsNullOrWhiteSpace(smsUrl) || string.IsNullOrWhiteSpace(smsTemplate))
            {
                return false;
            }

            try
            {
                string correctedMobileNumber = mobileNo.Replace("+91", string.Empty, StringComparison.Ordinal).Trim();
                string smsMessage = smsTemplate.Replace("{#var#}", otp, StringComparison.Ordinal).Replace("'OTP '", otp, StringComparison.Ordinal);
                string encodedMessage = Uri.EscapeDataString(smsMessage);
                string requestUrl = smsUrl.Replace("&amp;", "&", StringComparison.OrdinalIgnoreCase)
                                          .Replace("'@YourMobNo'", correctedMobileNumber, StringComparison.Ordinal)
                                          .Replace("'@YourMessage'", encodedMessage, StringComparison.Ordinal)
                                          .Replace("@YourMobNo", correctedMobileNumber, StringComparison.Ordinal)
                                          .Replace("@YourMessage", encodedMessage, StringComparison.Ordinal);

                using HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _dbHelper.LogError(ex, nameof(SendOtpAsync), new[]
                {
                    new SqlParameter("@HospitalIDF", hospitalidf)
                });
                return false;
            }
        }
        public static string MaskMobileNumber(string mobileNo)
        {
            string correctedMobileNumber = mobileNo.Replace("+91", string.Empty, StringComparison.Ordinal).Trim();
            string lastFourDigits = correctedMobileNumber.Length > 4 ? correctedMobileNumber[^4..] : correctedMobileNumber;

            return $"XXXXXX{lastFourDigits}";
        }
        //private sealed class SmsConfigurationModel
        //{
        //    public string? URL { get; set; }
        //    public string? SMSText { get; set; }
        //}
    }
}
