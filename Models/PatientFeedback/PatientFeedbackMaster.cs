using System.ComponentModel.DataAnnotations;

namespace DoctorMobileApp.Models.PatientFeedback
{
    public class FeedBackPatientMaster
    {
        [Key]
        public int FeedBackPatientIDP { get; set; }

        public DateTime RegistrationDateTime { get; set; }

        public string FeedBackRegCode { get; set; } = "";

        public byte RegistrationType { get; set; }

        public int RegistrationIDF { get; set; }

        public int PatientIDF { get; set; }

        public int? DoctorIDF { get; set; }

        public int? ReferenceDocIDF { get; set; }

        public int FeedbackMapIDF { get; set; }

        public string? Remarks { get; set; }


        public int HospitalIDF { get; set; }

        public int UserIDF { get; set; }

        public DateTime EntryDate { get; set; }

        public string? IPAddress { get; set; }

        public string? BrowserName { get; set; }


        // Edit Audit
        public int? EditUserIDF { get; set; }

        public DateTime? EditEntryDateTime { get; set; }

        public string? EditIPAddress { get; set; }

        public string? EditBrowserName { get; set; }
    }



    public class FeedBackPatientDetail
    {
        [Key]
        public int FeedBackPatientDetailIDP { get; set; }


        public int FeedBackPatientIDF { get; set; }


        public int FeedbackCategoryIDF { get; set; }


        public int? FeedbackQuestionIDF { get; set; }

        public byte Type { get; set; }

        public int? FeedbackOptionsTypeIDF { get; set; }

        public bool Answer { get; set; }

        public string? Text { get; set; }
    }
}