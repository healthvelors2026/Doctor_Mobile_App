using System.ComponentModel.DataAnnotations;

namespace DoctorMobileApp.Models.PatientFeedback
{
    #region Feedback Master <-- Main feedback tables
    public class FeedbackMap
    {
        [Key]
        public int FeedbackMapIDP { get; set; }

        public byte OPDIPDFlag { get; set; }

        public bool IsCurrentFormat { get; set; }

        public int VersionNumber { get; set; }

        public int HospitalGroupIDF { get; set; }
    }

    public class FeedbackCategory
    {
        [Key]
        public int FeedbackCategoryIDP { get; set; }

        public string? CategoryName { get; set; }

        public byte Type { get; set; }

        public bool NonActive { get; set; }
    }

    public class FeedbackQuestion
    {
        [Key]
        public int FeedbackQuestionIDP { get; set; }

        public string? Question { get; set; }

        public byte Type { get; set; }

        public bool NonActive { get; set; }
    }

    public class FeedbackOptionType
    {
        [Key]
        public int FeedbackOptionsTypeIDP { get; set; }

        public string? Type { get; set; }

        public string? Code { get; set; }

        public short SrNo { get; set; }

        public byte TypeEnum { get; set; }

        public int Marks { get; set; }

        public bool IsCurrentFormat { get; set; }

        public int HospitalGroupIDF { get; set; }
    }

    public class FeedbackMapCategoryDetail
    {
        [Key]
        public int FeedbackMapCategoryIDP { get; set; }

        public int FeedbackMapIDF { get; set; }

        public int FeedbackCategoryIDF { get; set; }

        public int SrNo { get; set; }
    }

    public class FeedbackMapQuestionDetail
    {
        [Key]
        public int FeedbackMapQuestionIDP { get; set; }

        public int FeedbackMapIDF { get; set; }

        public int FeedbackMapCategoryIDF { get; set; }

        public int FeedbackQuestionIDF { get; set; }

        public int SrNo { get; set; }
    }
    #endregion
}