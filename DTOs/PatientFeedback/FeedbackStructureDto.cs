using System.Text.Json.Serialization;

namespace DoctorMobileApp.DTOs.PatientFeedbackDto
{
    public class FeedbackFormDto
    {
        [JsonPropertyName("categories")]
        public List<FeedbackCategoryDto> Categories { get; set; } = new();
    }

    public class FeedbackCategoryDto
    {
        public int FeedbackCategoryId { get; set; }

        public int FeedbackMapCategoryId { get; set; }

        public string? CategoryName { get; set; }

        public int SrNo { get; set; }

        public int Type { get; set; }

        public List<FeedbackQuestionDto> Questions { get; set; } = new();
    }

    public class FeedbackQuestionDto
    {
        public int FeedbackQuestionId { get; set; }

        public string? Question { get; set; }

        public byte Type { get; set; }

        public int SrNo { get; set; }

        public int? SelectedOptionIDF { get; set; }

        public string? TextAnswer { get; set; }

        public bool? IsSelected { get; set; }

        public decimal? NumericAnswer { get; set; }

        public List<FeedbackOptionDto> Options { get; set; } = new();
    }

    public class FeedbackOptionDto
    {
        public int FeedbackOptionTypeId { get; set; }

        public string? Type { get; set; }

        public string? Code { get; set; }

        public int Marks { get; set; }

        public bool IsSelected { get; set; }
    }

    public class FeedbackAnswerDto
    {
        public int FeedbackQuestionId { get; set; }

        public int FeedbackOptionTypeId { get; set; }

        public string? Remarks { get; set; }
    }

    public class FeedbackMapDto
    {
        public int FeedbackMapId { get; set; }

       public byte OPDIPDFlag { get; set; }
    }
}