namespace ReportGenerationAPI.CommonClass
{
    public class clsEnum
    {


        //---Added by Akanksha Laheri : 09-Dec-2019 ---EmailReferenceType
        public enum EnumEmailReferenceType
        {
            EnumReceipts = 1,
            EnumOPDInvestigationReport = 2,
            EnumIPDInvestigationReport = 3,
            EnumManagement = 4,
            EnumIndividualInvestigationReport = 5,
            EnumInventoryServices = 6,                  //-- Added By Sanjeev Kumar : 06-Jan-2020
            EnumHRMS = 7
        }

        //---Added by Akanksha Laheri : 09-Dec-2019 ---EmailReceiptType
        public enum EnumEmailReceiptType
        {
            EnumOPDReceipt = 1,
            EnumAdvanceReceipt = 2,
            EnumOPDDirectReceipt = 3,
            EnumOPDHealthCheckUpReceipt = 4,
            EnumAdvanceRefund = 5,
            EnumOPDRefund = 6,
            EnumOPDDirectRefund = 7,
            EnumOPDHealthCheckUpRefund = 8,
            EnumOPDTestBillRefund = 9,
            EnumDirectOTBillRefund = 10,                   //-- OPD OT Bill as Direct
            EnumOPDServiceBillRefund = 11,
            EnumOPDTestReceipt = 12,
            EnumOPDCreditBillReceipt = 13,
            EnumOPDServiceBillReceipt = 14,

            EnumDirectOTBillReceipt = 15,
            EnumIPDTestReceipt = 16,
            EnumIPDTestBillRefund = 17,
            EnumIPDOTBillReceipt = 18,
            EnumIPDOTBillRefund = 19,
            EnumIPDServiceBillReceipt = 20,
            EnumDirectServiceBillReceipt = 21,
            EnumIPDServiceBillRefund = 22,
            EnumDirectServiceBillRefund = 23,
            EnumOPDEditReceipt = 24,
            EnumOPDEditDirectReceipt = 25,
            EnumOPDEditHealthCheckUpReceipt = 26,
            EnumIPDDischargeSummary = 27,
            EnumAppointment = 28,
            EnumFinalBillCreditBillReceipt = 29,    //--Sanjeev Kumar : 04-Mar-2020
            EnumFinalBill = 30,
            EnumIPDCurrentOutstanding = 31,         //--Sanjeev Kumar : 16-Mar-2020
            EnumAppointmentReminderMasterDays = 32,
        }

        public enum EnumEmailHRMSReceiptType
        {
            EnumLeaveRequest = 1,
            EnumPermissionRequest = 2,
            EnumTourRequest = 3,

            EnumLeaveRequestApprove = 4,
            EnumLeaveRequestApproved = 5,
            EnumLeaveRequestReject = 6,

            EnumPermissionRequestApprove = 7,
            EnumPermissionRequestApproved = 8,
            EnumPermissionRequestReject = 9,

            EnumTourRequestApprove = 10,
            EnumTourRequestApproved = 11,
            EnumTourRequestReject = 12,
        }

        //-- Added By Sanjeev Kumar : 06-Jan-2020
        public enum EnumEmailInventoryReceiptType
        {
            EnumInvSupplierRateContract = 1,
            EnumPurchaseOrder = 2,
            EnumWorkOrderForPO = 3,
            EnumGRN = 4,
            EnumRequisitionMaster = 5,
            EnumRequisitionIndent = 6,
            EnumSRN = 7,
            EnumSalesInvoice = 8,
            EnumSalesReturn = 9,
            EnumPOApprove = 10,
            EnumPOAudit = 11,
            EnumWorkOrderForPOApprove = 12,
            EnumWorkOrderForPOAudit = 13,
            EnumGRNQC = 14,
            EnumGRNAudit = 15,
            EnumGRNCancel = 16,
            EnumRequisitionApprove = 17,
            EnumRequisitionAudit = 18,
            EnumRequisitionForcefullyClose = 19,
            EnumIndentApproved = 20,
            EnumIndentAudit = 21,
            EnumIndentForcefullyClosed = 22,
            EnumSRNApprove = 23,
            EnumSRNCancel = 24,
            EnumPOForcefullyClose = 25,
            EnumWOForcefullyClose = 26,
            EnumRateContractForcefullyClose = 27,
        }

        //---Added by Akanksha Laheri : 09-Dec-2019 ---EmailInvestigationReportType
        public enum EnumEmailInvestigationReportType
        {
            EnumNPathologyReport = 1,
            EnumMCPathologyReport = 2,
            EnumBPathologyReport = 3,
            EnumRadiologyReport = 4,
            EnumProcedureReport = 5,
        }

        //---Added by Akanksha Laheri : 09-Dec-2019 ---EmailDoctorFlag
        public enum EnumEmailDoctorFlag
        {
            EnumPatient = 0,
            EnumConsultingDoctor = 1,
            EnumReferenceDoctor = 2,
            EnumLRRecommandBy = 3,
            EnumLRApproveBy = 4,
            EnumLREmployee = 5,
            EnumGRNQCEmp = 6,
            EnumGRNAuditEmp = 7,
            EnumGRNEmp = 8,
            EnumSRNApproveEmp = 9,
            EnumSRNEmp = 10,
        }


        public enum EnumEmailManagementReceiptType
        {
            EnumDailyReceipt = 1,
            EnumDailyStatusSummary = 2,
            EnumDailyCollectionServiceWise = 3,
            EnumAdmittedPatientOutStanding = 4,
            EnumBedOccupancy = 5,
            EnumLicenseManagement = 6,
            EnumFeedBack = 7,
            EnumSalesRegister = 8,
            EnumSalesReturnRegister = 9,
            EnumNearExpiryItem = 10,
            EnumReorderStockLevel = 11,
            EnumExpiryAgeingReport = 12,    // -- Added By Sanjeev Kumar : 03-Apr-2020
            EnumNonMovingItems = 13,        // -- Added By Sanjeev Kumar : 03-Apr-2020
        }

        ////---Added by Akanksha Laheri : 09-Dec-2019 ---EmailReferenceType
        //public enum EnumEmailReferenceType
        //{
        //    EnumReceipts = 1,
        //    EnumOPDInvestigationReport = 2,
        //    EnumIPDInvestigationReport = 3,
        //    EnumManagement = 4,
        //    EnumIndividualInvestigationReport = 5,
        //    EnumInventoryServices = 6,                  //-- Added By Sanjeev Kumar : 06-Jan-2020
        //    EnumHRMS = 7,
        //    EnumIPD = 8,
        //}

        ////---Added by Akanksha Laheri : 09-Dec-2019 ---EmailReceiptType
        //public enum EnumEmailReceiptType
        //{
        //    EnumOPDReceipt = 1,
        //    EnumAdvanceReceipt = 2,
        //    EnumOPDDirectReceipt = 3,
        //    EnumOPDHealthCheckUpReceipt = 4,
        //    EnumAdvanceRefund = 5,
        //    EnumOPDRefund = 6,
        //    EnumOPDDirectRefund = 7,
        //    EnumOPDHealthCheckUpRefund = 8,
        //    EnumOPDTestBillRefund = 9,
        //    EnumDirectOTBillRefund = 10,
        //    EnumOPDServiceBillRefund = 11,
        //    EnumOPDTestReceipt = 12,
        //    EnumOPDCreditBillReceipt = 13,
        //    EnumOPDServiceBillReceipt = 14,
        //    EnumInvSupplierRateContract = 15,           //-- Added By Sanjeev Kumar : 06-Jan-2020
        //    EnumPurchaseOrder = 16,                     //-- Added By Sanjeev Kumar : 06-Jan-2020
        //    EnumWorkOrderForPO = 17,                    //-- Added By Sanjeev Kumar : 07-Jan-2020
        //    EnumLeaveRequest = 18,
        //    EnumPermissionRequest = 19,
        //    EnumTourRequest = 20,
        //    EnumGRN = 21,
        //    EnumRequisitionMaster = 22,                 //-- Added By Sanjeev Kumar : 20-Jan-2020
        //    EnumRequisitionIndent = 23,                  //-- Added By Sanjeev Kumar : 22-Jan-2020
        //    EnumSRN = 24,
        //    EnumDirectOTBillReceipt = 25,
        //    EnumIPDTestReceipt = 26,
        //    EnumIPDTestBillRefund = 27,
        //    EnumIPDOTBillReceipt = 28,
        //    EnumIPDOTBillRefund = 29,
        //    EnumIPDServiceBillReceipt = 30,
        //    EnumDirectServiceBillReceipt = 31,
        //    EnumIPDServiceBillRefund = 32,
        //    EnumDirectServiceBillRefund = 33,
        //    EnumOPDEditReceipt = 34,
        //    EnumOPDEditDirectReceipt = 35,
        //    EnumOPDEditHealthCheckUpReceipt = 36,
        //    EnumIPDDischargeSummary = 37
        //}

        ////---Added by Akanksha Laheri : 09-Dec-2019 ---EmailInvestigationReportType
        //public enum EnumEmailInvestigationReportType
        //{
        //    EnumNPathologyReport = 1,
        //    EnumMCPathologyReport = 2,
        //    EnumBPathologyReport = 3,

        //}

        ////---Added by Akanksha Laheri : 09-Dec-2019 ---EmailDoctorFlag
        //public enum EnumEmailDoctorFlag
        //{
        //    EnumPatient = 0,
        //    EnumConsultingDoctor = 1,
        //    EnumReferenceDoctor = 2,
        //    EnumLRRecommandBy = 3,
        //    EnumLRApproveBy = 4,
        //    EnumLREmployee = 5,
        //    EnumGRNQCEmp = 6,
        //    EnumGRNAuditEmp = 7,
        //    EnumGRNEmp = 8,
        //    EnumSRNApproveEmp = 9,
        //    EnumSRNEmp = 10,
        //}

        // for patient selection

        public enum EnumPatientSearch
        {
            EnumAll = 0,
            EnumCrNumber = 1,
            EnumPatientName = 2,
            EnumUIDNumber = 3,
            EnumCompanyNo = 4
        }
        /// <summary>
        /// Sanjeev Kumar
        /// 17-Mar-2020
        /// </summary>
        public enum EnumServiceType
        {
            EnmGeneralService = 0,             //General Service Type (Service Bill)
            EnmOPDConsultation = 1,           //OPD Consultation Service Type 
            EnmVisit = 2,                     //IPD Visit Service Type
            EnmPathology = 3,                  //Pathology Service Type
            EnmRadiology = 4,                  //Radiology Service Type
            EnmProcedure = 5,                 //Procedure Service Type 
            EnmOT = 6,                        //OT Service Type
            EnmBloodBank = 7,                  //BloodBank Service Type  
            EnmBedCharge = 8,                  //IPD Bed Charge Service Type   
            EnmAmenityCharge = 9,              //IPD Amenity Charge Service Type   
            EnmPharmacy = 10,
            EnmInventory = 11
        }

        public enum EnmIPDFinalBillType
        {
            EnumBasedOnService = 0,
            EnumBasedOnServiceType = 1
        }


        public enum EnumAgeType
        {
            enmDays = 0,
            enmMonths = 1,
            enmYear = 2
        }

        public enum EnumAddressFlag
        {
            enmHospital = 1,
            enmEmployee = 2,
            enmPatient = 3,
            enmReferenceDoctor = 4,
            enmAccompaniedBy = 5,
            enumClient = 6,
            enumLab = 7,
            enumBloodDonor = 8,
            enumFundDonor = 9,
            enmInvCompany = 10,
            enmResidence = 11,
            enmClinic = 12,
            enmSupplier = 13,
            enmOTConsentBy = 14,
            enmDataBank = 15,
            enmTPA = 16,
            enmVisitor = 17,
            enmOwner = 18,
            enmDonor = 19,//Add by Deepa on 08-Apr-2015
            enmDayCareAccompaniedBy = 20,//Add by Chet@n 04/Feb/2017
            enmPatientRegistration = 21,
            enmHROutSourceCompany = 22,
            enmLostBusinessRecord = 23
        }
    }
}
