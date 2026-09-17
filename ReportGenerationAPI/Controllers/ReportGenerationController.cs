using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using ReportGenerationAPI.CommonClass;
using ReportGenerationAPI.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Net.Http;
using System.Web.Http;

namespace ReportGenerationAPI.Controllers
{
    [RoutePrefix("api/ReportGeneration")]
    public class ReportGenerationController : ApiController
    {

        [HttpPost]
        [Route("GenerateOPDReceipt")]
        public IHttpActionResult GenerateOPDReceipt(ReportGenerationModel.OPDReceiptPdfRequest request)
        {
            ReportGenerationModel.PdfGenerationResponse response = new ReportGenerationModel.PdfGenerationResponse();

            ReportDocument rdVoucher = null;

            string pdfFilePath = string.Empty;
            string opdCasePaperFilePath = string.Empty;
            string finalPdfFilePath = string.Empty;

            try
            {
                #region Validation
                if (request == null)
                {
                    response.IsSuccess = false;
                    response.ErrorMessage = "Request cannot be null.";
                    return Ok(response);
                }

                if (string.IsNullOrWhiteSpace(request.ReportRootPath))
                {
                    response.IsSuccess = false;
                    response.ErrorMessage = "ReportRootPath is required.";
                    return Ok(response);
                }

                if (string.IsNullOrWhiteSpace(request.OutputFolderPath))
                {
                    response.IsSuccess = false;
                    response.ErrorMessage = "OutputFolderPath is required.";
                    return Ok(response);
                }
                #endregion

                #region Create Date Wise Output Folder
                string currentDateFolderName;
                string currentDateOutputFolder = GetDateWiseOutputFolder(out currentDateFolderName);
                #endregion

                #region Determine Report Name
                string reportName;
                if (string.Equals(request.PaperType, "A4", StringComparison.OrdinalIgnoreCase))
                {
                    reportName = "OPDReceiptA4.rpt";
                }
                else
                {
                    reportName = "OPDReceiptA5.rpt";
                }
                #endregion

                #region Load Crystal Report
                string reportPath = Path.Combine(request.ReportRootPath, "Reception", reportName);
                if (!File.Exists(reportPath))
                {
                    response.IsSuccess = false;
                    response.ErrorMessage = "Report file not found: " + reportPath;
                    return Ok(response);
                }
                rdVoucher = new ReportDocument();
                rdVoucher.Load(reportPath);
                #endregion

                #region Paper Size
                if (string.Equals(request.PaperType, "A4", StringComparison.OrdinalIgnoreCase))
                {
                    rdVoucher.PrintOptions.PaperSize = PaperSize.PaperA4;
                }
                else
                {
                    rdVoucher.PrintOptions.PaperSize = PaperSize.PaperA5;
                }
                #endregion

                #region Crystal Report Parameters
                rdVoucher.SetParameterValue("@a_lng_HospitalIDF", request.HospitalID);
                rdVoucher.SetParameterValue("@a_lng_OPDRegistrationIDP", request.OPDRegistrationID);
                rdVoucher.SetParameterValue("@a_lng_VoucherIDP", request.VoucherId);
                rdVoucher.SetParameterValue("@a_int_IsRefund", Convert.ToInt32(request.IsRefund));
                #endregion

                #region Formula Fields
                rdVoucher.DataDefinition.FormulaFields["ShowDemo"].Text = "'false'";
                SetFormulaField(rdVoucher, "UserName", request.UserName);
                SetFormulaField(rdVoucher, "DOB", request.DOB);
                SetFormulaField(rdVoucher, "CurrentAvailableAdvance", string.IsNullOrEmpty(request.CurrentAvailableAdvance) ? "0" : request.CurrentAvailableAdvance);
                SetFormulaField(rdVoucher, "HealthCardNo", request.HealthCardNo);
                SetFormulaField(rdVoucher, "PatientSearch", "CRNumber");
                SetFormulaField(rdVoucher, "CompanyNo", "Company No");
                SetFormulaField(rdVoucher, "GrpOPDReg", request.GrpOPDReg);
                SetFormulaField(rdVoucher, "RefVouNo", request.RefVouNo);
                SetFormulaField(rdVoucher, "RefVouAmt", request.RefVouAmt);
                rdVoucher.DataDefinition.FormulaFields["HeaderRequired"].Text = "1";
                string receiptHeaderImagePath = request.IPAddress + "\\" + request.HospitalCode + request.ImagePath + "\\" + "ReceiptHeader.png";
                SetFormulaField(rdVoucher, "ImagePath", receiptHeaderImagePath);
                #endregion

                #region Database Connection
                SetReportConnection(rdVoucher);
                #endregion

                #region Generate OPD Receipt PDF
                string uniqueNumber = Guid.NewGuid().ToString("N").Substring(0, 10);
                string pdfFileName = request.CrNumber + "NReceipt_" + uniqueNumber + ".pdf";
                pdfFilePath = Path.Combine(currentDateOutputFolder, pdfFileName);
                rdVoucher.ExportToDisk(ExportFormatType.PortableDocFormat, pdfFilePath);
                rdVoucher.Close();
                rdVoucher.Dispose();
                rdVoucher = null;
                #endregion

                #region Generate OPD Case Paper

                ReportGenerationModel.PdfGenerationResponse OPDCashPaperResponse = GenerateOPDCasePaper(request, currentDateOutputFolder);

                if (OPDCashPaperResponse == null || !OPDCashPaperResponse.IsSuccess || string.IsNullOrWhiteSpace(OPDCashPaperResponse.FilePath) || !File.Exists(OPDCashPaperResponse.FilePath))
                {
                    response.IsSuccess = false;
                    response.ErrorMessage = OPDCashPaperResponse != null && !string.IsNullOrWhiteSpace(OPDCashPaperResponse.ErrorMessage) ? OPDCashPaperResponse.ErrorMessage : "OPD Case Paper PDF generation failed.";
                    return Ok(response);
                }

                opdCasePaperFilePath = OPDCashPaperResponse.FilePath;
                #endregion

                #region Merge PDFs
                string finalPdfFileName = request.CrNumber + "NReceiptOPDCasePaper_" + uniqueNumber + ".pdf";
                finalPdfFilePath = Path.Combine(currentDateOutputFolder, finalPdfFileName);
                MergePDF.MergeFiles(new List<string> { pdfFilePath, opdCasePaperFilePath }, finalPdfFilePath);
                #endregion

                #region Delete Temporary PDFs
                try
                {
                    if (File.Exists(pdfFilePath))
                    {
                        File.Delete(pdfFilePath);
                    }
                }
                catch
                {
                    // Ignore cleanup error
                }

                try
                {
                    if (File.Exists(opdCasePaperFilePath))
                    {
                        File.Delete(opdCasePaperFilePath);
                    }
                }
                catch
                {
                    // Ignore cleanup error
                }
                #endregion

                #region Response
                string relativeFilePath = Path.Combine(currentDateFolderName, finalPdfFileName).Replace("\\", "/");
                response.IsSuccess = true;
                response.FilePath = "Reports/" + relativeFilePath;
                response.FileName = finalPdfFileName;
                response.ErrorMessage = string.Empty;
                #endregion

                #region Return Final PDF

                if (!File.Exists(finalPdfFilePath))
                {
                    response.IsSuccess = false;
                    response.ErrorMessage = "Final PDF file was not created.";
                    return Ok(response);
                }

                byte[] pdfBytes = File.ReadAllBytes(finalPdfFilePath);

                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    response.IsSuccess = false;
                    response.ErrorMessage = "Final PDF file is empty.";
                    return Ok(response);
                }

                HttpResponseMessage httpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                ByteArrayContent pdfContent = new ByteArrayContent(pdfBytes);
                pdfContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                pdfContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment") { FileName = finalPdfFileName };
                httpResponse.Content = pdfContent;
                #endregion

                return ResponseMessage(httpResponse);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ErrorMessage = ex.Message + (ex.InnerException != null ? " | Inner Exception: " + ex.InnerException.Message : "");
            }
            finally
            {
                if (rdVoucher != null)
                {
                    try
                    {
                        rdVoucher.Close();
                        rdVoucher.Dispose();
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
            }

            return Ok(response);
        }



        private ReportGenerationModel.PdfGenerationResponse GenerateOPDCasePaper(ReportGenerationModel.OPDReceiptPdfRequest request, string outputFolder)
        {
            ReportGenerationModel.PdfGenerationResponse OPDCashPaperResponse = new ReportGenerationModel.PdfGenerationResponse();

            ReportDocument rdVoucher = null;

            try
            {
                #region Validation
                if (string.IsNullOrWhiteSpace(outputFolder))
                {
                    OPDCashPaperResponse.IsSuccess = false;
                    OPDCashPaperResponse.ErrorMessage = "Output folder is required.";
                    return OPDCashPaperResponse;
                }

                if (!Directory.Exists(outputFolder))
                {
                    Directory.CreateDirectory(outputFolder);
                }
                #endregion

                #region Determine Report Name
                string reportName = "OPDCasePaper.rpt";
                #endregion

                #region Load Crystal Report
                string reportPath = Path.Combine(request.ReportRootPath, "Reception", reportName);

                if (!File.Exists(reportPath))
                {
                    OPDCashPaperResponse.IsSuccess = false;
                    OPDCashPaperResponse.ErrorMessage = "Report file not found: " + reportPath;
                    return OPDCashPaperResponse;
                }

                rdVoucher = new ReportDocument();
                rdVoucher.Load(reportPath);
                #endregion

                #region Paper Size
                rdVoucher.PrintOptions.PaperSize = CrystalDecisions.Shared.PaperSize.PaperA5;
                #endregion

                #region Crystal Report Parameters
                rdVoucher.SetParameterValue(0, request.HospitalID);
                rdVoucher.SetParameterValue(1, request.OPDRegistrationID);
                rdVoucher.SetParameterValue(2, clsEnum.EnumAddressFlag.enmPatient);
                #endregion

                #region Formula Fields
                SetFormulaField(rdVoucher, "GrpOPDReg", request.GrpOPDReg);
                SetFormulaField(rdVoucher, "DOB", request.DOB);
                SetFormulaField(rdVoucher, "Original", "0");
                SetFormulaField(rdVoucher, "PatientSearch", "CRNumber");
                SetFormulaField(rdVoucher, "CompanyNo", "Company No");
                string receiptHeaderImagePath = request.IPAddress + "\\" + request.HospitalCode + request.ImagePath + "\\" + "ReceiptHeader.png";
                SetFormulaField(rdVoucher, "ImagePath", receiptHeaderImagePath);
                rdVoucher.DataDefinition.FormulaFields["HeaderRequired"].Text = "1";
                rdVoucher.DataDefinition.FormulaFields["PatientPhoto"].Text = "";
                SetFormulaField(rdVoucher, "ReportInstruction", "");
                SetFormulaField(rdVoucher, "ReportDigitalInstrument", "");
                #endregion

                #region Database Connection
                SetReportConnection(rdVoucher);
                #endregion

                #region Generate PDF
                string uniqueNumber = Guid.NewGuid().ToString("N").Substring(0, 10);
                string pdfFileName = request.CrNumber + "NOPDCasePaper_" + uniqueNumber + ".pdf";
                string pdfFilePath = Path.Combine(outputFolder, pdfFileName);
                rdVoucher.ExportToDisk(ExportFormatType.PortableDocFormat, pdfFilePath);
                rdVoucher.Close();
                rdVoucher.Dispose();
                rdVoucher = null;
                #endregion

                OPDCashPaperResponse.IsSuccess = true;
                OPDCashPaperResponse.FilePath = pdfFilePath;
                OPDCashPaperResponse.FileName = pdfFileName;
                OPDCashPaperResponse.ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                OPDCashPaperResponse.IsSuccess = false;
                OPDCashPaperResponse.ErrorMessage = ex.Message + (ex.InnerException != null ? " | Inner Exception: " + ex.InnerException.Message : "");
            }
            finally
            {
                if (rdVoucher != null)
                {
                    try
                    {
                        rdVoucher.Close();
                        rdVoucher.Dispose();
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
            }

            return OPDCashPaperResponse;
        }



        [HttpPost]
        [Route("GenerateOPDTestReceipt")]
        public IHttpActionResult GenerateOPDTestReceipt(ReportGenerationModel.OPDTestReceiptPdfRequest request)
        {
            ReportGenerationModel.PdfGenerationResponse response = new ReportGenerationModel.PdfGenerationResponse();

            ReportDocument rdVoucher = null;

            string pdfFilePath = string.Empty;
            string opdCasePaperFilePath = string.Empty;
            

            try
            {
                #region Validation
                if (request == null)
                {
                    response.IsSuccess = false;
                    response.ErrorMessage = "Request cannot be null.";
                    return Ok(response);
                }

                if (string.IsNullOrWhiteSpace(request.ReportRootPath))
                {
                    response.IsSuccess = false;
                    response.ErrorMessage = "ReportRootPath is required.";
                    return Ok(response);
                }

                if (string.IsNullOrWhiteSpace(request.OutputFolderPath))
                {
                    response.IsSuccess = false;
                    response.ErrorMessage = "OutputFolderPath is required.";
                    return Ok(response);
                }
                #endregion

                #region Create Date Wise Output Folder
                string currentDateFolderName;
                string currentDateOutputFolder = GetDateWiseOutputFolder(out currentDateFolderName);
                #endregion

                #region Determine Report Name
                string reportName;
                if (string.Equals(request.PaperType, "A5", StringComparison.OrdinalIgnoreCase))
                {
                    reportName = "OPDTestBill.rpt";
                }
                else
                {
                    reportName = "OPDTestBillA4.rpt";
                }
                #endregion

                #region Load Crystal Report
                string reportPath = Path.Combine(request.ReportRootPath, "Reception", reportName);
                if (!File.Exists(reportPath))
                {
                    response.IsSuccess = false;
                    response.ErrorMessage = "Report file not found: " + reportPath;
                    return Ok(response);
                }
                rdVoucher = new ReportDocument();
                rdVoucher.Load(reportPath);
                #endregion

                #region Paper Size
                if (string.Equals(request.PaperType, "A4", StringComparison.OrdinalIgnoreCase))
                {
                    rdVoucher.PrintOptions.PaperSize = PaperSize.PaperA4;
                }
                else
                {
                    rdVoucher.PrintOptions.PaperSize = PaperSize.PaperA5;
                }
                #endregion

                #region Crystal Report Parameters
                rdVoucher.SetParameterValue(0, request.HospitalID);
                rdVoucher.SetParameterValue(1, request.OPDRegistrationID);
                rdVoucher.SetParameterValue(2, request.VoucherId);
                rdVoucher.SetParameterValue(3, request.IsRefund);
                #endregion

                #region Formula Fields
                rdVoucher.DataDefinition.FormulaFields["ShowDemo"].Text = "'false'";
                SetFormulaField(rdVoucher, "UserName", request.UserName);
                SetFormulaField(rdVoucher, "DOB", request.DOB);
                SetFormulaField(rdVoucher, "CurrentAvailableAdvance", string.IsNullOrEmpty(request.CurrentAvailableAdvance) ? "0" : request.CurrentAvailableAdvance);
                SetFormulaField(rdVoucher, "HealthCardNo", request.HealthCardNo);
                SetFormulaField(rdVoucher, "PatientSearch", "CRNumber");
                SetFormulaField(rdVoucher, "CompanyNo", "Company No");
                SetFormulaField(rdVoucher, "GrpOPDReg", request.GrpOPDReg);
                SetFormulaField(rdVoucher, "RefVouNo", request.RefVouNo);
                SetFormulaField(rdVoucher, "RefVouAmt", request.RefVouAmt);
                rdVoucher.DataDefinition.FormulaFields["HeaderRequired"].Text = "1";
                string receiptHeaderImagePath = request.IPAddress + "\\" + request.HospitalCode + request.ImagePath + "\\" + "ReceiptHeader.png";
                SetFormulaField(rdVoucher, "ImagePath", receiptHeaderImagePath);
                #endregion

                #region Database Connection
                SetReportConnection(rdVoucher);
                #endregion

                #region Generate OPD Receipt PDF
                string uniqueNumber = Guid.NewGuid().ToString("N").Substring(0, 10);
                string finalPdfFileName = request.CrNumber + "NOPDTestReceipt_" + uniqueNumber + ".pdf";
                pdfFilePath = Path.Combine(currentDateOutputFolder, finalPdfFileName);
                rdVoucher.ExportToDisk(ExportFormatType.PortableDocFormat, pdfFilePath);
                rdVoucher.Close();
                rdVoucher.Dispose();
                rdVoucher = null;
                #endregion

                #region Response
                string relativeFilePath = Path.Combine(currentDateFolderName, finalPdfFileName).Replace("\\", "/");
                response.IsSuccess = true;
                response.FilePath = "Reports/" + relativeFilePath;
                response.FileName = finalPdfFileName;
                response.ErrorMessage = string.Empty;
                #endregion

                #region Return Final PDF

                if (!File.Exists(pdfFilePath))
                {
                    response.IsSuccess = false;
                    response.ErrorMessage = "Final PDF file was not created.";
                    return Ok(response);
                }

                byte[] pdfBytes = File.ReadAllBytes(pdfFilePath);

                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    response.IsSuccess = false;
                    response.ErrorMessage = "Final PDF file is empty.";
                    return Ok(response);
                }

                HttpResponseMessage httpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                ByteArrayContent pdfContent = new ByteArrayContent(pdfBytes);
                pdfContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                pdfContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment") { FileName = finalPdfFileName };
                httpResponse.Content = pdfContent;
                #endregion

                return ResponseMessage(httpResponse);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ErrorMessage = ex.Message + (ex.InnerException != null ? " | Inner Exception: " + ex.InnerException.Message : "");
            }
            finally
            {
                if (rdVoucher != null)
                {
                    try
                    {
                        rdVoucher.Close();
                        rdVoucher.Dispose();
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
            }

            return Ok(response);
        }

        [HttpPost]
        [Route("GenerateAdvanceReceipt")]
        public IHttpActionResult GenerateAdvanceReceipt(ReportGenerationModel.AdvanceReceiptPdfRequest request)
        {
            ReportGenerationModel.PdfGenerationResponse response = new ReportGenerationModel.PdfGenerationResponse();

            ReportDocument rdVoucher = null;

            string pdfFilePath = string.Empty;
            string opdCasePaperFilePath = string.Empty;


            try
            {
                #region Validation
                if (request == null)
                {
                    response.IsSuccess = false;
                    response.ErrorMessage = "Request cannot be null.";
                    return Ok(response);
                }

                if (string.IsNullOrWhiteSpace(request.ReportRootPath))
                {
                    response.IsSuccess = false;
                    response.ErrorMessage = "ReportRootPath is required.";
                    return Ok(response);
                }

                if (string.IsNullOrWhiteSpace(request.OutputFolderPath))
                {
                    response.IsSuccess = false;
                    response.ErrorMessage = "OutputFolderPath is required.";
                    return Ok(response);
                }
                #endregion

                #region Create Date Wise Output Folder
                string currentDateFolderName;
                string currentDateOutputFolder = GetDateWiseOutputFolder(out currentDateFolderName);
                #endregion

                #region Determine Report Name
                string reportName;
                if (string.Equals(request.PaperType, "A4", StringComparison.OrdinalIgnoreCase))
                {
                    reportName = "AdvanceReceiptA4.rpt";
                }
                else
                {
                    reportName = "AdvanceReceipt.rpt";
                }
                #endregion

                #region Load Crystal Report
                string reportPath = Path.Combine(request.ReportRootPath, "Reception", reportName);
                if (!File.Exists(reportPath))
                {
                    response.IsSuccess = false;
                    response.ErrorMessage = "Report file not found: " + reportPath;
                    return Ok(response);
                }
                rdVoucher = new ReportDocument();
                rdVoucher.Load(reportPath);
                #endregion

                #region Paper Size
                if (string.Equals(request.PaperType, "A4", StringComparison.OrdinalIgnoreCase))
                {
                    rdVoucher.PrintOptions.PaperSize = PaperSize.PaperA4;
                }
                else
                {
                    rdVoucher.PrintOptions.PaperSize = PaperSize.PaperA5;
                }
                #endregion

                #region Crystal Report Parameters
                rdVoucher.SetParameterValue(0, request.HospitalID);
                rdVoucher.SetParameterValue(1, System.DateTime.Now.Date.ToString("dd/MMM/yyyy"));
                rdVoucher.SetParameterValue(2, System.DateTime.Now.Date.ToString("dd/MMM/yyyy"));
                rdVoucher.SetParameterValue(3, request.VoucherNumber);
                rdVoucher.SetParameterValue(4, Convert.ToString("VoucherIDP = " + request.VoucherId + ""));
                #endregion

                #region Formula Fields
                rdVoucher.DataDefinition.FormulaFields["ShowDemo"].Text = "'false'";
                SetFormulaField(rdVoucher, "UserName", request.UserName);
                SetFormulaField(rdVoucher, "PatientSearch", "CRNumber");

                rdVoucher.DataDefinition.FormulaFields["HeaderRequired"].Text = "1";
                string receiptHeaderImagePath = request.IPAddress + "\\" + request.HospitalCode + request.ImagePath + "\\" + "ReceiptHeader.png";
                SetFormulaField(rdVoucher, "ImagePath", receiptHeaderImagePath);

                SetFormulaField(rdVoucher, "ReportInstruction", "");
                SetFormulaField(rdVoucher, "DigitalReportInstruction", "");
                #endregion

                #region Database Connection
                SetReportConnection(rdVoucher);
                #endregion

                #region Generate OPD Receipt PDF
                string uniqueNumber = Guid.NewGuid().ToString("N").Substring(0, 10);
                string finalPdfFileName = request.CrNumber + "NAdvanceReceipt_" + uniqueNumber + ".pdf";
                pdfFilePath = Path.Combine(currentDateOutputFolder, finalPdfFileName);
                rdVoucher.ExportToDisk(ExportFormatType.PortableDocFormat, pdfFilePath);
                rdVoucher.Close();
                rdVoucher.Dispose();
                rdVoucher = null;
                #endregion

                #region Response
                string relativeFilePath = Path.Combine(currentDateFolderName, finalPdfFileName).Replace("\\", "/");
                response.IsSuccess = true;
                response.FilePath = "Reports/" + relativeFilePath;
                response.FileName = finalPdfFileName;
                response.ErrorMessage = string.Empty;
                #endregion

                #region Return Final PDF

                if (!File.Exists(pdfFilePath))
                {
                    response.IsSuccess = false;
                    response.ErrorMessage = "Final PDF file was not created.";
                    return Ok(response);
                }

                byte[] pdfBytes = File.ReadAllBytes(pdfFilePath);

                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    response.IsSuccess = false;
                    response.ErrorMessage = "Final PDF file is empty.";
                    return Ok(response);
                }

                HttpResponseMessage httpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                ByteArrayContent pdfContent = new ByteArrayContent(pdfBytes);
                pdfContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                pdfContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment") { FileName = finalPdfFileName };
                httpResponse.Content = pdfContent;
                #endregion

                return ResponseMessage(httpResponse);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ErrorMessage = ex.Message + (ex.InnerException != null ? " | Inner Exception: " + ex.InnerException.Message : "");
            }
            finally
            {
                if (rdVoucher != null)
                {
                    try
                    {
                        rdVoucher.Close();
                        rdVoucher.Dispose();
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
            }

            return Ok(response);
        }


        [HttpPost]
        [Route("GeneratePathologyReport")]
        public IHttpActionResult GeneratePathologyReport(ReportGenerationModel.PathologyReportPdfRequest request)
        {
            ReportGenerationModel.PdfGenerationResponse response = new ReportGenerationModel.PdfGenerationResponse();

            ReportDocument rdVoucher = null;

            string pdfFilePath = string.Empty;
            string opdCasePaperFilePath = string.Empty;


            try
            {
                #region Validation
                if (request == null)
                {
                    response.IsSuccess = false;
                    response.ErrorMessage = "Request cannot be null.";
                    return Ok(response);
                }

                if (string.IsNullOrWhiteSpace(request.ReportRootPath))
                {
                    response.IsSuccess = false;
                    response.ErrorMessage = "ReportRootPath is required.";
                    return Ok(response);
                }

                if (string.IsNullOrWhiteSpace(request.OutputFolderPath))
                {
                    response.IsSuccess = false;
                    response.ErrorMessage = "OutputFolderPath is required.";
                    return Ok(response);
                }
                #endregion

                #region Create Date Wise Output Folder
                string currentDateFolderName;
                string currentDateOutputFolder = GetDateWiseOutputFolder(out currentDateFolderName);
                #endregion

                #region Determine Report Name
                string reportName;
                reportName = "PathoTestResult.rpt";
                #endregion

                #region Load Crystal Report
                string reportPath = Path.Combine(request.ReportRootPath, "Pathology", reportName);
                if (!File.Exists(reportPath))
                {
                    response.IsSuccess = false;
                    response.ErrorMessage = "Report file not found: " + reportPath;
                    return Ok(response);
                }
                rdVoucher = new ReportDocument();
                rdVoucher.Load(reportPath);
                #endregion

                #region Paper Size
                rdVoucher.PrintOptions.PaperSize = PaperSize.PaperA5;
                #endregion

                #region Crystal Report Parameters
                
                rdVoucher.SetParameterValue(0, request.PathoRegistrationIDs);
                rdVoucher.SetParameterValue(1, request.SampleCollection);
                rdVoucher.SetParameterValue(2, request.HospitalID);
                
                #endregion

                #region Formula Fields
                //rdVoucher.DataDefinition.FormulaFields["ShowDemo"].Text = "'false'";
                SetFormulaField(rdVoucher, "UserName", request.UserName);
                SetFormulaField(rdVoucher, "PatientSearch", "CRNumber");
                SetFormulaField(rdVoucher, "CollectionApplicable", Convert.ToInt32(request.SampleCollection).ToString());
                SetFormulaField(rdVoucher, "DispatchApplicable", Convert.ToInt32(request.SampleCollection).ToString());
                SetFormulaField(rdVoucher, "DoneByApplicable", Convert.ToInt32(request.SampleCollection).ToString());
                rdVoucher.DataDefinition.FormulaFields["HeaderRequired"].Text = "1";
                string receiptHeaderImagePath = request.IPAddress + "\\" + request.HospitalCode + request.ImagePath + "\\" + "ReceiptHeader.png";
                SetFormulaField(rdVoucher, "ImagePath", receiptHeaderImagePath);

                rdVoucher.DataDefinition.FormulaFields["ReportDigitalInstrument"].Text = "";
                rdVoucher.DataDefinition.FormulaFields["ShowPreviousTestResult"].Text = Convert.ToInt32(request.ShowPreviousTestResult).ToString();
                rdVoucher.DataDefinition.FormulaFields["ShowDrInFooter"].Text = Convert.ToInt32(request.ShowDrInReportFooter).ToString();

                rdVoucher.DataDefinition.FormulaFields["SignaturePath"].Text = "";
                #endregion

                #region Database Connection
                SetReportConnection(rdVoucher);
                #endregion

                #region Generate OPD Receipt PDF
                string uniqueNumber = Guid.NewGuid().ToString("N").Substring(0, 10);
                string finalPdfFileName = request.CrNumber + "NPathologyReport_" + uniqueNumber + ".pdf";
                pdfFilePath = Path.Combine(currentDateOutputFolder, finalPdfFileName);
                rdVoucher.ExportToDisk(ExportFormatType.PortableDocFormat, pdfFilePath);
                rdVoucher.Close();
                rdVoucher.Dispose();
                rdVoucher = null;
                #endregion

                #region Response
                string relativeFilePath = Path.Combine(currentDateFolderName, finalPdfFileName).Replace("\\", "/");
                response.IsSuccess = true;
                response.FilePath = "Reports/" + relativeFilePath;
                response.FileName = finalPdfFileName;
                response.ErrorMessage = string.Empty;
                #endregion

                #region Return Final PDF

                if (!File.Exists(pdfFilePath))
                {
                    response.IsSuccess = false;
                    response.ErrorMessage = "Final PDF file was not created.";
                    return Ok(response);
                }

                byte[] pdfBytes = File.ReadAllBytes(pdfFilePath);

                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    response.IsSuccess = false;
                    response.ErrorMessage = "Final PDF file is empty.";
                    return Ok(response);
                }

                HttpResponseMessage httpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                ByteArrayContent pdfContent = new ByteArrayContent(pdfBytes);
                pdfContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                pdfContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment") { FileName = finalPdfFileName };
                httpResponse.Content = pdfContent;
                #endregion

                return ResponseMessage(httpResponse);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ErrorMessage = ex.Message + (ex.InnerException != null ? " | Inner Exception: " + ex.InnerException.Message : "");
            }
            finally
            {
                if (rdVoucher != null)
                {
                    try
                    {
                        rdVoucher.Close();
                        rdVoucher.Dispose();
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
            }

            return Ok(response);
        }

        #region Private Methods

        private string GetDateWiseOutputFolder(out string currentDateFolderName)
        {
            string applicationPath = AppDomain.CurrentDomain.BaseDirectory;

            string rootOutputFolder = Path.Combine(applicationPath, "Reports");

            if (!Directory.Exists(rootOutputFolder))
            {
                Directory.CreateDirectory(rootOutputFolder);
            }

            currentDateFolderName = DateTime.Now.ToString("yyyyMMdd");

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
                        // Ignore folders which cannot be deleted
                    }
                }
            }

            #endregion

            if (!Directory.Exists(currentDateOutputFolder))
            {
                Directory.CreateDirectory(currentDateOutputFolder);
            }

            return currentDateOutputFolder;
        }

        #endregion


        public static void SetReportConnection(ReportDocument l_obj_Report)
        {
            string ReportConnection = ConfigurationManager.ConnectionStrings["ReportConnectionString"].ConnectionString;
            l_obj_Report.DataSourceConnections[0].SetConnection(ReportConnection.Split('=')[1].Split(';')[0], ReportConnection.Split('=')[2].Split(';')[0], ReportConnection.Split('=')[3].Split(';')[0], ReportConnection.Split('=')[4].Split(';')[0]);
        }


        #region Helper Methods

        private void SetFormulaField(ReportDocument reportDocument, string formulaFieldName, string value)
        {
            reportDocument.DataDefinition.FormulaFields[formulaFieldName].Text = "'" + EscapeCrystalText(value) + "'";
        }


        private string EscapeCrystalText(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value.Replace("'", "''");
        }
        #endregion

    }
}