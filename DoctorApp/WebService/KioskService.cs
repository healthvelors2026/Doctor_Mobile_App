using DoctorMobileApp.CommonClass;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Net;
using System.Runtime.Serialization;
using static DoctorMobileApp.Models.KioskModel;
using System.Net.Http.Json;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace DoctorMobileApp.WebServices
{
    public class KioskService
    {
        private readonly IDbConnectionFactory _dbHelper;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly IWebHostEnvironment _environment;


        public KioskService(IDbConnectionFactory db, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, HttpClient httpClient, IWebHostEnvironment environment)
        {
            _dbHelper = db;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClient;
            _environment = environment;
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
                    new("@HealthCardPatientIssueDetailIDP",model.HealthCardPatientIssueDetailIDP),
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
        public async Task<List<HealthCardActivePatientResponseModel>> GetHealthCardActivePatientListAsync(HealthCardActivePatientRequestModel requestModel, int hospitalidf)
        {
            var HCAPatientlist = new List<HealthCardActivePatientResponseModel>();
            var HCAPParam = new[]
            {
                new SqlParameter("@PatientID" , requestModel.PatientID),
                new SqlParameter("@HospitalIDF", hospitalidf)
            };

            HCAPatientlist = await _dbHelper.QueryAsync<HealthCardActivePatientResponseModel>("Kiosk_API_GetActivePatientHealthCard_GetList", CommandType.StoredProcedure, HCAPParam);
            return HCAPatientlist;
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

        #region PDF Prints

        private string SavePdfFile(byte[] pdfBytes, string fileName, out string relativeFilePath)
        {
            relativeFilePath = string.Empty;

            if (pdfBytes == null || pdfBytes.Length == 0)
            {
                return string.Empty;
            }

            string reportOutputFolder = _configuration["AppSettings:ReportOutputFolder"];

            if (string.IsNullOrWhiteSpace(reportOutputFolder))
            {
                reportOutputFolder = "Reports";
            }

            string rootOutputFolder = Path.Combine(_environment.ContentRootPath, reportOutputFolder);

            if (!Directory.Exists(rootOutputFolder))
            {
                Directory.CreateDirectory(rootOutputFolder);
            }

            string currentDateFolderName = DateTime.Now.ToString("yyyyMMdd");

            string currentDateOutputFolder = Path.Combine(rootOutputFolder, currentDateFolderName);

            #region Delete Previous Date Folders

            string[] existingDirectories = Directory.GetDirectories(rootOutputFolder);

            foreach (string directory in existingDirectories)
            {
                string folderName = Path.GetFileName(directory);

                if (!string.Equals(folderName, currentDateFolderName, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        Directory.Delete(directory, true);
                    }
                    catch
                    {
                        // Ignore deletion error
                    }
                }
            }

            #endregion

            if (!Directory.Exists(currentDateOutputFolder))
            {
                Directory.CreateDirectory(currentDateOutputFolder);
            }

            string filePath = Path.Combine(currentDateOutputFolder, fileName);

            File.WriteAllBytes(filePath, pdfBytes);

            relativeFilePath = Path.Combine("Reports", currentDateFolderName, fileName).Replace("\\", "/");

            return filePath;
        }



        public async Task<OPDReceiptResponseModel> GetOPDReceiptAsync(int voucherId, int hospitalidf)
        {
            OPDReceiptResponseModel receiptModel = new OPDReceiptResponseModel();

            try
            {
                #region Get Receipt Details

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@VoucherID", voucherId)
                };

                var results = await _dbHelper.QueryAsync<OPDReceiptResponseModel>("Kiosk_API_GetOPDReceipt_Print", CommandType.StoredProcedure, parameters);

                receiptModel = results.FirstOrDefault();

                if (receiptModel == null)
                {
                    return new OPDReceiptResponseModel();
                }

                #endregion

                #region Get Print Configuration

                var parametersHCP = new SqlParameter[]
                {
                    new SqlParameter("@HospitalID", hospitalidf),
                    new SqlParameter("@ReceiptName", "OPD Normal Reg.")
                };

                var resultsHCP = await _dbHelper.QueryAsync<HospitalPrintConfigurationModel>("Kiosk_API_GetHospitalPrintConfiguration_Print", CommandType.StoredProcedure, parametersHCP);

                HospitalPrintConfigurationModel printConfig = resultsHCP.FirstOrDefault();

                if (printConfig == null)
                {
                    return receiptModel;
                }

                #endregion

                #region Get Formula Fields

                var parametersFormulaFields = new SqlParameter[]
                {
                    new SqlParameter("@HMSEmailConfigurationIDP", printConfig.HospitalPrintConfigIDP),
                    new SqlParameter("@VoucherIDP", voucherId),
                    new SqlParameter("@RegistrationIDP", receiptModel.OPDReceiptPDFPath)
                };

                var resultsFormulaFields = await _dbHelper.QueryAsync<FormulaFieldForOPDRegistrationReceiptModel>("SP_GetFormulaFieldForOPDRegistrationReceipt", CommandType.StoredProcedure, parametersFormulaFields);

                FormulaFieldForOPDRegistrationReceiptModel formulaModel = resultsFormulaFields.FirstOrDefault();

                #endregion

                #region Prepare PDF Request

                string applicationPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                string outputFolderPath = Path.Combine(applicationPath, "Reports");

                if (!Directory.Exists(outputFolderPath))
                {
                    Directory.CreateDirectory(outputFolderPath);
                }

                var pdfRequest = new OPDReceiptPdfRequest
                {
                    VoucherId = voucherId,
                    HospitalID = Convert.ToInt32(receiptModel.hospitalIDF),
                    OPDRegistrationID = Convert.ToInt32(receiptModel.OPDRegistrationIDP),
                    IsRefund = printConfig.ReceiptType == Convert.ToInt32(CommonClass.clsEnum.EnumEmailReceiptType.EnumOPDRefund) ? 1 : 0,
                    ReportRootPath = _configuration["AppSettings:CrReportPath"] ?? string.Empty,
                    PaperType = Convert.ToString(printConfig.PaperType),
                    CrNumber = Convert.ToString(receiptModel.CrNumber),
                    UserName = Convert.ToString(formulaModel?.UserName),
                    DOB = Convert.ToString(formulaModel?.DOB),
                    CurrentAvailableAdvance = string.IsNullOrEmpty(Convert.ToString(formulaModel?.CurrentAvailableAdvance)) ? "0" : Convert.ToString(formulaModel?.CurrentAvailableAdvance),
                    HealthCardNo = Convert.ToString(formulaModel?.HealthCardNo),
                    GrpOPDReg = Convert.ToString(formulaModel?.GrpOPDReg),
                    RefVouNo = Convert.ToString(formulaModel?.RefVouNo),
                    RefVouAmt = Convert.ToString(formulaModel?.RefVouAmt),
                    HospitalCode = Convert.ToString(formulaModel?.HospitalCode),
                    IPAddress = "",
                    ImagePath = _configuration["AppSettings:ReceiptHeader"] ?? string.Empty,
                    OutputFolderPath = outputFolderPath
                };

                #endregion

                #region Call .NET Framework PDF Service

                byte[] pdfBytes = await GenerateOPDReceiptPDFAsync(pdfRequest);

                if (pdfBytes != null && pdfBytes.Length > 0)
                {
                    string relativeFilePath;

                    string uniqueNumber = Guid.NewGuid().ToString("N").Substring(0, 10);

                    string fileName = pdfRequest.CrNumber + "NOPDReceiptCasePaper_" + uniqueNumber + ".pdf";

                    string savedFilePath = SavePdfFile(pdfBytes, fileName, out relativeFilePath);

                    if (!string.IsNullOrWhiteSpace(savedFilePath))
                    {
                        string reportDownloadBaseUrl = _configuration["AppSettings:ReportDownloadBaseUrl"];

                        if (!string.IsNullOrWhiteSpace(reportDownloadBaseUrl))
                        {
                            string downloadPath = relativeFilePath.Replace("\\", "/").TrimStart('/');

                            receiptModel.OPDReceiptPDFPath = reportDownloadBaseUrl.TrimEnd('/') + "/" + downloadPath;
                        }
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return receiptModel;
        }

        private async Task<byte[]> GenerateOPDReceiptPDFAsync(OPDReceiptPdfRequest request)
        {
            try
            {
                string? reportGenerationAPIUrl = _configuration["AppSettings:ReportGenerationAPI:BaseUrl"];

                if (string.IsNullOrWhiteSpace(reportGenerationAPIUrl))
                {
                    return null;
                }

                string apiUrl = reportGenerationAPIUrl.TrimEnd('/') + "/api/ReportGeneration/GenerateOPDReceipt";

                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(apiUrl, request);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                byte[] pdfBytes = await response.Content.ReadAsByteArrayAsync();

                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    return null;
                }

                return pdfBytes;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                return null;
            }
        }



        public async Task<OPDTestReceiptPrintResponseModel> GetOPDTestReceiptAsync(int voucherId, int hospitalidf)
        {
            OPDTestReceiptPrintResponseModel receiptModel = new OPDTestReceiptPrintResponseModel();

            try
            {
                #region Get Receipt Details

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@VoucherID", voucherId)
                };

                var results = await _dbHelper.QueryAsync<OPDTestReceiptPrintResponseModel>("Kiosk_API_GetOPDReceipt_Print", CommandType.StoredProcedure, parameters);

                receiptModel = results.FirstOrDefault();

                if (receiptModel == null)
                {
                    return new OPDTestReceiptPrintResponseModel();
                }

                #endregion

                #region Get Print Configuration

                var parametersHCP = new SqlParameter[]
                {
                    new SqlParameter("@HospitalID", hospitalidf),
                    new SqlParameter("@ReceiptName", "OPD Normal Reg.")
                };

                var resultsHCP = await _dbHelper.QueryAsync<HospitalPrintConfigurationModel>("Kiosk_API_GetHospitalPrintConfiguration_Print", CommandType.StoredProcedure, parametersHCP);

                HospitalPrintConfigurationModel printConfig = resultsHCP.FirstOrDefault();

                if (printConfig == null)
                {
                    return receiptModel;
                }

                #endregion

                #region Get Formula Fields

                var parametersFormulaFields = new SqlParameter[]
                {
                    new SqlParameter("@HMSEmailConfigurationIDP", printConfig.HospitalPrintConfigIDP),
                    new SqlParameter("@VoucherIDP", voucherId),
                    new SqlParameter("@RegistrationIDP", receiptModel.OPDTestReceiptPDFPath)
                };

                var resultsFormulaFields = await _dbHelper.QueryAsync<FormulaFieldForOPDRegistrationReceiptModel>("SP_GetFormulaFieldForOPDRegistrationReceipt", CommandType.StoredProcedure, parametersFormulaFields);

                FormulaFieldForOPDRegistrationReceiptModel formulaModel = resultsFormulaFields.FirstOrDefault();

                #endregion

                #region Prepare PDF Request

                string applicationPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                string outputFolderPath = Path.Combine(applicationPath, "Reports");

                if (!Directory.Exists(outputFolderPath))
                {
                    Directory.CreateDirectory(outputFolderPath);
                }

                var pdfRequest = new OPDTestReceiptPdfRequest
                {
                    VoucherId = voucherId,
                    HospitalID = Convert.ToInt32(receiptModel.hospitalIDF),
                    OPDRegistrationID = Convert.ToInt32(receiptModel.OPDRegistrationIDP),
                    IsRefund = printConfig.ReceiptType == Convert.ToInt32(CommonClass.clsEnum.EnumEmailReceiptType.EnumOPDRefund) ? 1 : 0,
                    ReportRootPath = _configuration["AppSettings:CrReportPath"] ?? string.Empty,
                    PaperType = Convert.ToString(printConfig.PaperType),
                    CrNumber = Convert.ToString(receiptModel.CrNumber),
                    UserName = Convert.ToString(formulaModel?.UserName),
                    DOB = Convert.ToString(formulaModel?.DOB),
                    CurrentAvailableAdvance = string.IsNullOrEmpty(Convert.ToString(formulaModel?.CurrentAvailableAdvance)) ? "0" : Convert.ToString(formulaModel?.CurrentAvailableAdvance),
                    HealthCardNo = Convert.ToString(formulaModel?.HealthCardNo),
                    GrpOPDReg = Convert.ToString(formulaModel?.GrpOPDReg),
                    RefVouNo = Convert.ToString(formulaModel?.RefVouNo),
                    RefVouAmt = Convert.ToString(formulaModel?.RefVouAmt),
                    HospitalCode = Convert.ToString(formulaModel?.HospitalCode),
                    IPAddress = "",
                    ImagePath = _configuration["AppSettings:ReceiptHeader"] ?? string.Empty,
                    OutputFolderPath = outputFolderPath
                };

                #endregion

                #region Call .NET Framework PDF Service

                byte[] pdfBytes = await GenerateOPDTestReceiptPDFAsync(pdfRequest);

                if (pdfBytes != null && pdfBytes.Length > 0)
                {
                    string relativeFilePath;

                    string uniqueNumber = Guid.NewGuid().ToString("N").Substring(0, 10);

                    string fileName = pdfRequest.CrNumber + "NOPDTestReceipt_" + uniqueNumber + ".pdf";

                    string savedFilePath = SavePdfFile(pdfBytes, fileName, out relativeFilePath);

                    if (!string.IsNullOrWhiteSpace(savedFilePath))
                    {
                        string reportDownloadBaseUrl = _configuration["AppSettings:ReportDownloadBaseUrl"];

                        if (!string.IsNullOrWhiteSpace(reportDownloadBaseUrl))
                        {
                            string downloadPath = relativeFilePath.Replace("\\", "/").TrimStart('/');

                            receiptModel.OPDTestReceiptPDFPath = reportDownloadBaseUrl.TrimEnd('/') + "/" + downloadPath;
                        }
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return receiptModel;
        }

        private async Task<byte[]> GenerateOPDTestReceiptPDFAsync(OPDTestReceiptPdfRequest request)
        {
            try
            {
                string? reportGenerationAPIUrl = _configuration["AppSettings:ReportGenerationAPI:BaseUrl"];

                if (string.IsNullOrWhiteSpace(reportGenerationAPIUrl))
                {
                    return null;
                }

                string apiUrl = reportGenerationAPIUrl.TrimEnd('/') + "/api/ReportGeneration/GenerateOPDTestReceipt";

                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(apiUrl, request);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                byte[] pdfBytes = await response.Content.ReadAsByteArrayAsync();

                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    return null;
                }

                return pdfBytes;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                return null;
            }
        }



        public async Task<AdvanceReceiptPrintResponseModel> GetAdvanceReceiptAsync(int voucherId, string voucherNumber, int hospitalidf)
        {
            AdvanceReceiptPrintResponseModel receiptModel = new AdvanceReceiptPrintResponseModel();

            try
            {
                //#region Get Receipt Details

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@VoucherID", voucherId)
                };

                var results = await _dbHelper.QueryAsync<AdvanceReceiptPrintResponseModel>("Kiosk_API_GetOPDReceipt_Print", CommandType.StoredProcedure, parameters);

                receiptModel = results.FirstOrDefault();

                if (receiptModel == null)
                {
                    return new AdvanceReceiptPrintResponseModel();
                }

                //#endregion

                #region Get Print Configuration

                var parametersHCP = new SqlParameter[]
                {
                    new SqlParameter("@HospitalID", hospitalidf),
                    new SqlParameter("@ReceiptName", "Advance")
                };

                var resultsHCP = await _dbHelper.QueryAsync<HospitalPrintConfigurationModel>("Kiosk_API_GetHospitalPrintConfiguration_Print", CommandType.StoredProcedure, parametersHCP);

                HospitalPrintConfigurationModel printConfig = resultsHCP.FirstOrDefault();

                if (printConfig == null)
                {
                    return receiptModel;
                }

                #endregion

                #region Get Formula Fields

                var parametersFormulaFields = new SqlParameter[]
                {
                    new SqlParameter("@HMSEmailConfigurationIDP", printConfig.HospitalPrintConfigIDP),
                    new SqlParameter("@VoucherIDP", voucherId),
                    new SqlParameter("@RegistrationIDP", 0)
                };

                var resultsFormulaFields = await _dbHelper.QueryAsync<FormulaFieldForOPDRegistrationReceiptModel>("SP_GetFormulaFieldForOPDRegistrationReceipt", CommandType.StoredProcedure, parametersFormulaFields);

                FormulaFieldForOPDRegistrationReceiptModel formulaModel = resultsFormulaFields.FirstOrDefault();

                #endregion

                #region Prepare PDF Request

                string applicationPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                string outputFolderPath = Path.Combine(applicationPath, "Reports");

                if (!Directory.Exists(outputFolderPath))
                {
                    Directory.CreateDirectory(outputFolderPath);
                }

                var pdfRequest = new AdvanceReceiptPdfRequest
                {
                    VoucherId = voucherId,
                    VoucherNumber = voucherNumber,
                    HospitalID = Convert.ToInt32(receiptModel.hospitalIDF),
                    ReportRootPath = _configuration["AppSettings:CrReportPath"] ?? string.Empty,
                    PaperType = Convert.ToString(printConfig.PaperType),
                    CrNumber = Convert.ToString(receiptModel.CrNumber),
                    UserName = Convert.ToString(formulaModel?.UserName),
                    HospitalCode = Convert.ToString(formulaModel?.HospitalCode),
                    IPAddress = "",
                    ImagePath = _configuration["AppSettings:ReceiptHeader"] ?? string.Empty,
                    OutputFolderPath = outputFolderPath
                };

                #endregion

                #region Call .NET Framework PDF Service

                byte[] pdfBytes = await GenerateAdvanceReceiptPDFAsync(pdfRequest);

                if (pdfBytes != null && pdfBytes.Length > 0)
                {
                    string relativeFilePath;

                    string uniqueNumber = Guid.NewGuid().ToString("N").Substring(0, 10);

                    string fileName = pdfRequest.CrNumber + "NAdvanceReceipt_" + uniqueNumber + ".pdf";

                    string savedFilePath = SavePdfFile(pdfBytes, fileName, out relativeFilePath);

                    if (!string.IsNullOrWhiteSpace(savedFilePath))
                    {
                        string reportDownloadBaseUrl = _configuration["AppSettings:ReportDownloadBaseUrl"];

                        if (!string.IsNullOrWhiteSpace(reportDownloadBaseUrl))
                        {
                            string downloadPath = relativeFilePath.Replace("\\", "/").TrimStart('/');

                            receiptModel.AdvanceReceiptPDFPath = reportDownloadBaseUrl.TrimEnd('/') + "/" + downloadPath;
                        }
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return receiptModel;
        }

        private async Task<byte[]> GenerateAdvanceReceiptPDFAsync(AdvanceReceiptPdfRequest request)
        {
            try
            {
                string? reportGenerationAPIUrl = _configuration["AppSettings:ReportGenerationAPI:BaseUrl"];

                if (string.IsNullOrWhiteSpace(reportGenerationAPIUrl))
                {
                    return null;
                }

                string apiUrl = reportGenerationAPIUrl.TrimEnd('/') + "/api/ReportGeneration/GenerateAdvanceReceipt";

                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(apiUrl, request);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                byte[] pdfBytes = await response.Content.ReadAsByteArrayAsync();

                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    return null;
                }

                return pdfBytes;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                return null;
            }
        }



        public async Task<PathologyReportPrintResponseModel> GetPathologyReportAsync(string PathoRegistrationIDs, string CrNumber, int hospitalidf)
        {
            PathologyReportPrintResponseModel receiptModel = new PathologyReportPrintResponseModel();

            try
            {
                #region Get Hospital and Hospital Configuration

                var parametersHCP = new SqlParameter[]
                {
                    new SqlParameter("@HospitalIDF", hospitalidf)
                };

                var resultsHCP = await _dbHelper.QueryAsync<HospitalConfigurationModel>("KIOSK_Sp_GetHospitalAndConfiguration", CommandType.StoredProcedure, parametersHCP);

                HospitalConfigurationModel hospConfig = resultsHCP.FirstOrDefault();

                if (hospConfig == null)
                {
                    return receiptModel;
                }

                #endregion

                #region Prepare PDF Request

                string applicationPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                string outputFolderPath = Path.Combine(applicationPath, "Reports");

                if (!Directory.Exists(outputFolderPath))
                {
                    Directory.CreateDirectory(outputFolderPath);
                }

                var pdfRequest = new PathologyReportPrintPdfRequest
                {
                    PathoRegistrationIDs = PathoRegistrationIDs,
                    HospitalID = hospitalidf,
                    ReportRootPath = _configuration["AppSettings:CrReportPath"] ?? string.Empty,
                    CrNumber = Convert.ToString(receiptModel.CrNumber),
                    UserName = "",
                    HospitalCode = hospConfig.HospitalCode,
                    IPAddress = _configuration["AppSettings:IPReceiptHeader"] ?? string.Empty,
                    ImagePath = _configuration["AppSettings:ReceiptHeader"] ?? string.Empty,
                    OutputFolderPath = outputFolderPath
                };

                #endregion

                #region Call .NET Framework PDF Service

                byte[] pdfBytes = await GeneratePathologyReportPDFAsync(pdfRequest);

                if (pdfBytes != null && pdfBytes.Length > 0)
                {
                    string relativeFilePath;

                    string uniqueNumber = Guid.NewGuid().ToString("N").Substring(0, 10);

                    string fileName = pdfRequest.CrNumber + "NPathologyReport_" + uniqueNumber + ".pdf";

                    string savedFilePath = SavePdfFile(pdfBytes, fileName, out relativeFilePath);

                    if (!string.IsNullOrWhiteSpace(savedFilePath))
                    {
                        string reportDownloadBaseUrl = _configuration["AppSettings:ReportDownloadBaseUrl"];

                        if (!string.IsNullOrWhiteSpace(reportDownloadBaseUrl))
                        {
                            string downloadPath = relativeFilePath.Replace("\\", "/").TrimStart('/');

                            receiptModel.PathologyReportPDFPath = reportDownloadBaseUrl.TrimEnd('/') + "/" + downloadPath;
                        }
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return receiptModel;
        }

        private async Task<byte[]> GeneratePathologyReportPDFAsync(PathologyReportPrintPdfRequest request)
        {
            try
            {
                string? reportGenerationAPIUrl = _configuration["AppSettings:ReportGenerationAPI:BaseUrl"];

                if (string.IsNullOrWhiteSpace(reportGenerationAPIUrl))
                {
                    return null;
                }

                string apiUrl = reportGenerationAPIUrl.TrimEnd('/') + "/api/ReportGeneration/GeneratePathologyReport";

                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(apiUrl, request);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                byte[] pdfBytes = await response.Content.ReadAsByteArrayAsync();

                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    return null;
                }

                return pdfBytes;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                return null;
            }
        }
        #endregion
    }
}
