using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReportGenerationAPI.Models
{
    public class ReportGenerationModel
    {
        public class OPDReceiptPdfRequest
        {
            public int VoucherId { get; set; }
            public int HospitalID { get; set; }
            public int OPDRegistrationID { get; set; }
            public int IsRefund { get; set; }
            public string ReportRootPath { get; set; }
            public string PaperType { get; set; }
            public string CrNumber { get; set; }
            public string UserName { get; set; }
            public string DOB { get; set; }
            public string CurrentAvailableAdvance { get; set; }
            public string HealthCardNo { get; set; }
            public string GrpOPDReg { get; set; }
            public string RefVouNo { get; set; }
            public string RefVouAmt { get; set; }
            public string HospitalCode { get; set; }
            public string IPAddress { get; set; }
            public string ImagePath { get; set; }
            public string OutputFolderPath { get; set; }
        }

        public class OPDTestReceiptPdfRequest
        {
            public int VoucherId { get; set; }
            public int HospitalID { get; set; }
            public int OPDRegistrationID { get; set; }
            public int IsRefund { get; set; }
            public string ReportRootPath { get; set; }
            public string PaperType { get; set; }
            public string CrNumber { get; set; }
            public string UserName { get; set; }
            public string DOB { get; set; }
            public string CurrentAvailableAdvance { get; set; }
            public string HealthCardNo { get; set; }
            public string GrpOPDReg { get; set; }
            public string RefVouNo { get; set; }
            public string RefVouAmt { get; set; }
            public string HospitalCode { get; set; }
            public string IPAddress { get; set; }
            public string ImagePath { get; set; }
            public string OutputFolderPath { get; set; }
        }


        public class AdvanceReceiptPdfRequest
        {
            public int VoucherId { get; set; }
            public string VoucherNumber { get; set; }
            public int HospitalID { get; set; }
            //public int IsRefund { get; set; }
            public string ReportRootPath { get; set; }
            public string PaperType { get; set; }
            public string CrNumber { get; set; }
            public string UserName { get; set; }
            public string HospitalCode { get; set; }
            public string IPAddress { get; set; }
            public string ImagePath { get; set; }
            public string OutputFolderPath { get; set; }
        }


        public class PathologyReportPdfRequest
        {
            public int PatientId { get; set; }
            public int HospitalID { get; set; }
            public string PathoRegistrationIDs { get; set; }
            public bool SampleCollection { get; set; }
            public string UserName { get; set; }

            public bool ShowDrInReportFooter { get; set; }
            public bool ShowPreviousTestResult { get; set; }

            public string ReportRootPath { get; set; }
            public string CrNumber { get; set; }
            public string HospitalCode { get; set; }
            public string IPAddress { get; set; }
            public string ImagePath { get; set; }
            public string OutputFolderPath { get; set; }
        }

        

        public class PdfGenerationResponse
        {
            public bool IsSuccess { get; set; }
            public string FilePath { get; set; }
            public string FileName { get; set; }
            public string ErrorMessage { get; set; }
        }
    }
}