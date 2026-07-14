namespace DoctorMobileApp.DTOs.PatientFeedbackDto
{
    public class SaveFeedbackRequestDto
    {
        public FeedBackPatientMasterDto Master { get; set; } = new();

        public List<FeedBackPatientDetailDto> Details { get; set; } = new();

        public string? Token { get; set; }
    }

    public class FeedBackPatientMasterDto
    {
        public DateTime RegistrationDateTime { get; set; }

        public byte RegistrationType { get; set; }

        public int RegistrationIDF { get; set; }

        public int PatientIDF { get; set; }

        public int? DoctorIDF { get; set; }

        public int? ReferenceDocIDF { get; set; }

        public string? Remarks { get; set; }

        public string? RegistrationCode { get; set; }
    }

    public class FeedBackPatientDetailDto
    {
        public int? FeedbackCategoryIDF { get; set; }

        public int? FeedbackQuestionIDF { get; set; }

        public int? SelectedOptionIDF { get; set; }

        public string? TextAnswer { get; set; }

        public bool IsSelected { get; set; }

        public bool Answer { get; set; }

        public byte Type { get; set; }
    }
}