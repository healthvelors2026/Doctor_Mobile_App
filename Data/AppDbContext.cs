using DoctorMobileApp.Models.PatientFeedback;
using DoctorMobileApp.PatientFeedback;
using Microsoft.EntityFrameworkCore;
namespace DoctorMobileApp.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ===========================
            // OPD
            // ===========================
            modelBuilder.Entity<OpdRegistration>()
                .ToTable("tbOPDRegistration");

            modelBuilder.Entity<Patient>()
                .ToTable("tbPatientMaster");

            modelBuilder.Entity<Employee>()
                .ToTable("tbEmployeeMaster");

            modelBuilder.Entity<OpdRegistration>()
                .HasKey(x => x.OPDRegistrationIDP);

            modelBuilder.Entity<Patient>()
                .HasKey(x => x.PatientIDP);

            modelBuilder.Entity<Employee>()
                .HasKey(x => x.EmployeeIDP);

            // ===========================
            // IPD
            // ===========================
            modelBuilder.Entity<IpdAdmissionDischarge>()
                .ToTable("tbIPDAdmissionDischarge");

            modelBuilder.Entity<IpdAdmissionDischarge>()
                .HasKey(x => x.IPDAdmissionDischargeIDP);

            // ===========================
            // CLASS
            // ===========================
            modelBuilder.Entity<ClassMaster>()
                .ToTable("tbClassMaster");

            modelBuilder.Entity<ClassMaster>()
                .HasKey(x => x.ClassIDP);

            // ===========================
            // FEEDBACK MODULE
            // ===========================

            modelBuilder.Entity<FeedbackMap>()
                .ToTable("tbFeedbackMap");

            modelBuilder.Entity<FeedbackMap>()
                .HasKey(x => x.FeedbackMapIDP);

            modelBuilder.Entity<FeedbackCategory>()
                .ToTable("tbFeedbackCategory");

            modelBuilder.Entity<FeedbackCategory>()
                .HasKey(x => x.FeedbackCategoryIDP);

            modelBuilder.Entity<FeedbackQuestion>()
                .ToTable("tbFeedbackQuestion");

            modelBuilder.Entity<FeedbackQuestion>()
                .HasKey(x => x.FeedbackQuestionIDP);

            modelBuilder.Entity<FeedbackOptionType>()
                .ToTable("tbFeedbackOptionsType");

            modelBuilder.Entity<FeedbackOptionType>()
                .HasKey(x => x.FeedbackOptionsTypeIDP);

            modelBuilder.Entity<FeedbackMapCategoryDetail>()
                .ToTable("tbFeedbackMapCategoryDetails");

            modelBuilder.Entity<FeedbackMapCategoryDetail>()
                .HasKey(x => x.FeedbackMapCategoryIDP);

            modelBuilder.Entity<FeedbackMapQuestionDetail>()
                .ToTable("tbFeedbackMapQuestionDetails");

            modelBuilder.Entity<FeedbackMapQuestionDetail>()
                .HasKey(x => x.FeedbackMapQuestionIDP);

            // ===========================
            // FEEDBACK SAVE TABLES
            // ===========================

            modelBuilder.Entity<FeedBackPatientMaster>()
                .ToTable("tbFeedBackPatientMaster");

            modelBuilder.Entity<FeedBackPatientMaster>()
                .HasKey(x => x.FeedBackPatientIDP);

            modelBuilder.Entity<FeedBackPatientDetail>()
                .ToTable("tbFeedBackPatientDetails");

            modelBuilder.Entity<FeedBackPatientDetail>()
                .HasKey(x => x.FeedBackPatientDetailIDP);

            modelBuilder.Entity<tbUserMaster>()
                 .ToTable("tbUserMaster");

            modelBuilder.Entity<tbUserMaster>()
                .HasKey(x => x.UserIDP);


            modelBuilder.Entity<tbFeedbackToken>()
             .ToTable("tbFeedbackToken");

            modelBuilder.Entity<tbFeedbackToken>()
                .HasKey(x => x.FeedbackTokenID);
        }

        // ===========================
        // OPD
        // ===========================
        public DbSet<OpdRegistration> OpdRegistrations { get; set; }

        public DbSet<Patient> Patients { get; set; }

        public DbSet<Employee> Employees { get; set; }

        // ===========================
        // IPD
        // ===========================
        public DbSet<IpdAdmissionDischarge> IpdAdmissionDischarges { get; set; }

        // ===========================
        // CLASS
        // ===========================
        public DbSet<ClassMaster> ClassMasters { get; set; }

        // ===========================
        // FEEDBACK MODULE
        // ===========================
        public DbSet<FeedbackMap> FeedbackMaps { get; set; }

        public DbSet<FeedbackCategory> FeedbackCategories { get; set; }

        public DbSet<FeedbackQuestion> FeedbackQuestions { get; set; }

        public DbSet<FeedbackOptionType> FeedbackOptionTypes { get; set; }

        public DbSet<Models.PatientFeedback.FeedbackMapCategoryDetail> FeedbackMapCategoryDetails { get; set; }

        public DbSet<Models.PatientFeedback.FeedbackMapQuestionDetail> FeedbackMapQuestionDetails { get; set; }

        // ===========================
        // FEEDBACK SAVE TABLES
        // ===========================
        public DbSet<Models.PatientFeedback.FeedBackPatientMaster> FeedBackPatientMasters { get; set; }

        public DbSet<FeedBackPatientDetail> FeedBackPatientDetails { get; set; }

        // ===========================
        // USER MASTER
        // ===========================
        public DbSet<tbUserMaster> tbUserMasters { get; set; }


        public DbSet<tbFeedbackToken> tbFeedbackTokens { get; set; }
    }
}