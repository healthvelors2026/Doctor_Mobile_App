using DoctorMobileApp.CommonClass.Services.Interface;
using DoctorMobileApp.DTOs.PatientFeedback;
using DoctorMobileApp.DTOs.PatientFeedbackDto;
using DoctorMobileApp.Models;
using DoctorMobileApp.Models.PatientFeedback;
using DoctorMobileApp.Repository.PatientFeedback.Interface;
using Microsoft.EntityFrameworkCore;

namespace DoctorMobileApp.Repository.PatientFeedback.Implementation
{
    public class PatientFeedbackRepository : IPatientFeedbackRepository
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public PatientFeedbackRepository(
            AppDbContext context,
            ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        #region GET

        public async Task<PatientFeedbackDto?> GetPatientFeedbackAsync(
     int patientId,
     int registrationId,
     byte registrationType)
        {
            PatientFeedbackDto? result;
            // registrationType = 0 -> OPD
            // registrationType != 0 -> IPD
            if (registrationType == 0)
            {
                result = await GetOpdBasicAsync(patientId, registrationId);
            }
            else
            {
                result = await GetIpdBasicAsync(patientId, registrationId);
            }

            if (result == null)
                return null;
            // Fetch the latest saved feedback for this patient and registration
            var master = await _context.FeedBackPatientMasters
                .AsNoTracking()
                .Where(x =>
                    x.PatientIDF == patientId &&
                    x.RegistrationIDF == registrationId &&
                    x.RegistrationType == registrationType &&
                    x.HospitalIDF == _currentUser.HospitalId)
                .OrderByDescending(x => x.FeedBackPatientIDP)
                .FirstOrDefaultAsync();
            // Use saved map id if feedback exists, else load current map
            int mapId = master?.FeedbackMapIDF ?? 0;
            result.Remarks = master?.Remarks ?? "";
            if (master?.DoctorIDF != null)
                result.DoctorIDF = master.DoctorIDF.Value;
            if (mapId == 0)
            {
                mapId = await _context.FeedbackMaps
                    .AsNoTracking()
                    .Where(x =>
                        x.OPDIPDFlag == registrationType &&
                        x.IsCurrentFormat &&
                        x.HospitalGroupIDF == _currentUser.HospitalGroupId)
                    .OrderByDescending(x => x.VersionNumber)
                    .Select(x => x.FeedbackMapIDP)
                    .FirstOrDefaultAsync();
            }
            // Load feedback categories and questions.
            if (mapId != 0)
            {
                result.FeedbackForm.Categories =
                    await GetCategoriesAsync(
                        mapId,
                        master?.FeedBackPatientIDP ?? 0);
            }
            // Load detailed feedback answers if feedback has already been submitte
            result.FeedbackDetails =
                master == null
                ? new List<FeedBackPatientDetailDto>()
                : await GetFeedbackDetailsAsync(master.FeedBackPatientIDP);


            return result;
        }
        // =========================
        // GET OPD DETAILS
        // =========================
        private async Task<PatientFeedbackDto?> GetOpdBasicAsync(
    int patientId,
    int registrationId)
        {
            return await (
                from opd in _context.OpdRegistrations

                join patient in _context.Patients
                    on opd.PatientIDF equals patient.PatientIDP

                join doctor in _context.Employees
                    on opd.DoctorIDF equals doctor.EmployeeIDP into dg

                from doctor in dg.DefaultIfEmpty()

                where opd.OPDRegistrationIDP == registrationId
                      && opd.PatientIDF == patientId

                select new PatientFeedbackDto
                {
                    CRNumber = patient.CRNumber,

                    RegistrationCode = opd.RegistrationCode,

                    RegistrationDate = opd.RegistrationDateTime,

                    RegistrationType = 0,

                    DoctorIDF = opd.DoctorIDF,

                    DoctorName = doctor == null
                        ? "-"
                        : $"{doctor.FName} {doctor.MName} {doctor.LName}".Trim(),

                    FeedbackForm = new FeedbackFormDto()
                }

            )
            .AsNoTracking()
            .FirstOrDefaultAsync();
        }


        // =========================
        // GET IPD DETAILS
        // =========================
        // =========================
        // GET IPD DETAILS
        // =========================
        private async Task<PatientFeedbackDto?> GetIpdBasicAsync(
     int patientId,
     int registrationId)
        {
            var ipd = await _context.IpdAdmissionDischarges
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.PatientIDF == patientId &&
                    x.IPDAdmissionDischargeIDP == registrationId);

            if (ipd == null)
                return null;

            var patient = await _context.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.PatientIDP == patientId);

            var doctor = await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.EmployeeIDP == ipd.PrimaryDocIDF);

            var classMaster = await _context.ClassMasters
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.ClassIDP == ipd.ClassIDF);


            return new PatientFeedbackDto
            {
                CRNumber = patient?.CRNumber,
                IPDRegistrationCode = ipd.IPDRegistrationCode,
                AdmissionDateTime = ipd.AdmissionDateTime,
                DischargeDate = ipd.DischargeDate,
                DoctorIDF = ipd.PrimaryDocIDF,
                DoctorName = doctor == null
                    ? "-"
                    : $"{doctor.FName} {doctor.MName} {doctor.LName}".Trim(),

                ClassName = classMaster?.ClassName,
                RegistrationType = 1,
                FeedbackForm = new FeedbackFormDto()
            };
        }


        private async Task<List<FeedBackPatientDetailDto>> GetFeedbackDetailsAsync(
    int masterId)
        {
            //Retrieve all saved feedback answers for the specified
            // feedback master record.
            return await _context.FeedBackPatientDetails
                .AsNoTracking()
                .Where(x =>
                    x.FeedBackPatientIDF == masterId)

                .Select(x => new FeedBackPatientDetailDto
                {
                    FeedbackCategoryIDF =
                        x.FeedbackCategoryIDF,
                    FeedbackQuestionIDF =
                        x.FeedbackQuestionIDF,
                    SelectedOptionIDF =
                        x.FeedbackOptionsTypeIDF,
                    // User-entered answer for text-based questions
                    TextAnswer =
                        x.Text,
                     // Selected answer for Yes/No or checkbox questions
                    IsSelected =
                        x.Answer,
                    Answer =
                        x.Answer,
                    // Question type (Text, Rating, Checkbox, etc.)
                    Type =
                        x.Type
                })
                .ToListAsync();
        }
        #endregion
        #region COMMON CATEGORY

        // =========================
        // Categories
        // =========================
        private async Task<List<FeedbackCategoryDto>> GetCategoriesAsync(
    int feedbackMapId,
    int masterId)
        {
            var groupId = _currentUser.HospitalGroupId;
            // Load all active feedback options (Rating, Excellent, Good, etc.)
            var allOptions = await _context.FeedbackOptionTypes
                .AsNoTracking()
                .Where(x =>
                    x.IsCurrentFormat &&
                    x.HospitalGroupIDF == groupId)
                .OrderBy(x => x.SrNo)
                .ToListAsync();


            var categories = await (
                from mapCat in _context.FeedbackMapCategoryDetails
                join cat in _context.FeedbackCategories
                    on mapCat.FeedbackCategoryIDF equals cat.FeedbackCategoryIDP
                where mapCat.FeedbackMapIDF == feedbackMapId
                orderby mapCat.SrNo
                select new FeedbackCategoryDto
                {
                    FeedbackCategoryId = cat.FeedbackCategoryIDP,
                    FeedbackMapCategoryId = mapCat.FeedbackMapCategoryIDP,
                    CategoryName = cat.CategoryName,
                    Type = cat.Type,
                    SrNo = mapCat.SrNo
                })
                .ToListAsync();


            var details = masterId == 0
                ? new List<FeedBackPatientDetail>()
                : await _context.FeedBackPatientDetails
                    .AsNoTracking()
                    .Where(x => x.FeedBackPatientIDF == masterId)
                    .ToListAsync();


            foreach (var cat in categories)
            {
                // ===========================
                //  Handle text-only categories (Remarks/Comments).
                // ===========================
                if (cat.Type == 2)
                {
                    var textValue = details
                        .Where(x =>
                            x.FeedbackCategoryIDF == cat.FeedbackCategoryId &&
                            x.Type == 2)
                        .Select(x => x.Text)
                        .FirstOrDefault() ?? string.Empty;


                    cat.Questions = new List<FeedbackQuestionDto>
            {
                new FeedbackQuestionDto
                {
                    FeedbackQuestionId = 0,
                    Question = cat.CategoryName,
                    Type = 2,
                    SrNo = 0,
                    TextAnswer = textValue,
                    Options = new List<FeedbackOptionDto>()
                }
            };

                    continue;
                }


                // ===========================
                // QUESTIONS
                // ===========================
                var questions = await GetQuestionsAsync(
                    feedbackMapId,
                    cat.FeedbackMapCategoryId,
                    allOptions);


                foreach (var q in questions)
                {
                    var qDetails = details
                        .Where(x =>
                            x.FeedbackCategoryIDF == cat.FeedbackCategoryId &&
                            x.FeedbackQuestionIDF == q.FeedbackQuestionId)
                        .ToList();
                    // ===========================
                    // YES / NO
                    // ===========================
                    if (q.Type == 0 || q.Type == 4)
                    {
                        q.IsSelected = qDetails.Any(x => x.Answer);
                    }
                    // ===========================
                    // OPTION BASED
                    // ===========================
                    else if (q.Type == 1 || q.Type == 3)
                    {
                        q.SelectedOptionIDF = qDetails
                            .Where(x => x.Answer)
                            .Select(x => x.FeedbackOptionsTypeIDF)
                            .FirstOrDefault();
                    }
                }
                cat.Questions = questions
                    .OrderBy(x => x.SrNo)
                    .ToList();
            }
            return categories
                .OrderBy(x => x.SrNo)
                .ToList();
        }
        #endregion


        #region COMMON QUESTIONS

        // =========================
        // Questions
        // =========================
        private async Task<List<FeedbackQuestionDto>> GetQuestionsAsync(
          int feedbackMapId,
          int feedbackMapCategoryId,
          List<FeedbackOptionType> allOptions)
        {
            // Retrieve all active questions mapped to the specified
            // feedback category and feedback format.
            var questions = await (
                from mapQ in _context.FeedbackMapQuestionDetails
                join q in _context.FeedbackQuestions
                    on mapQ.FeedbackQuestionIDF equals q.FeedbackQuestionIDP
                where mapQ.FeedbackMapIDF == feedbackMapId
                      && mapQ.FeedbackMapCategoryIDF == feedbackMapCategoryId
                      && !q.NonActive
                orderby mapQ.SrNo
                select new FeedbackQuestionDto
                {
                    FeedbackQuestionId = q.FeedbackQuestionIDP,
                    Question = q.Question,
                    Type = q.Type,
                    SrNo = mapQ.SrNo,
                    SelectedOptionIDF = null,
                    TextAnswer = null,
                    IsSelected = null,
                    NumericAnswer = null,
                    Options = new List<FeedbackOptionDto>()
                })
                .ToListAsync();
            // Populate option lists for option-based questions
            foreach (var q in questions)
            {
                if (q.Type == 1 || q.Type == 3)
                {
                    q.Options = allOptions
                        .Where(x => x.TypeEnum == q.Type)
                        .OrderBy(x => x.SrNo)
                        .Select(x => new FeedbackOptionDto
                        {
                            FeedbackOptionTypeId = x.FeedbackOptionsTypeIDP,
                            Type = x.Type,
                            Code = x.Code,
                            Marks = x.Marks
                        })
                        .ToList();
                }
                else
                {
                    q.Options = new List<FeedbackOptionDto>();
                }
            }
            return questions;
        }
        #endregion

        #region SAVE FEEDBACK

        // =========================
        // SAVE FEEDBACK
        // =========================
        public async Task<int> SaveFeedbackAsync(SaveFeedbackRequestDto request)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                if (request?.Master == null)
                    throw new Exception("Invalid request");

                request.Details ??=
                    new List<FeedBackPatientDetailDto>();

                var feedbackMap = await _context.FeedbackMaps
                    .AsNoTracking()
                    .Where(x =>
                        x.OPDIPDFlag == request.Master.RegistrationType &&
                        x.IsCurrentFormat &&
                        x.HospitalGroupIDF == _currentUser.HospitalGroupId)
                    .OrderByDescending(x => x.VersionNumber)
                    .FirstOrDefaultAsync();

                if (feedbackMap == null)
                    throw new Exception("No active feedback map found");

                int hospitalId =
                    _currentUser.HospitalId;
                int userId =
                    _currentUser.UserId;

                // =========================
                // MASTER
                // =========================

                var master =
                    await _context.FeedBackPatientMasters
                    .FirstOrDefaultAsync(x =>
                        x.PatientIDF ==
                            request.Master.PatientIDF &&
                        x.RegistrationIDF ==
                            request.Master.RegistrationIDF &&
                        x.RegistrationType ==
                            request.Master.RegistrationType &&
                        x.HospitalIDF ==
                            hospitalId);

                if (master == null)
                {
                    master = new FeedBackPatientMaster
                    {
                        PatientIDF =
                            request.Master.PatientIDF,
                        RegistrationIDF =
                            request.Master.RegistrationIDF,
                        RegistrationType =
                            request.Master.RegistrationType,
                        RegistrationDateTime =
                            DateTime.Now,
                        DoctorIDF =
                            request.Master.DoctorIDF,
                        ReferenceDocIDF =
                            request.Master.ReferenceDocIDF,
                        FeedbackMapIDF =
                            feedbackMap.FeedbackMapIDP,
                        Remarks =
                            request.Master.Remarks ?? "",
                        HospitalIDF =
                            hospitalId,
                        UserIDF =
                            userId,
                        EntryDate =
                            DateTime.Now,
                        IPAddress =
                            _currentUser.IpAddress,
                        BrowserName =
                            _currentUser.Browser,
                        FeedBackRegCode =
                            await GenerateRegCode(
                                hospitalId,
                                request.Master.RegistrationType)
                    };
                    _context.FeedBackPatientMasters.Add(master);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    master.DoctorIDF = request.Master.DoctorIDF;
                    master.ReferenceDocIDF = request.Master.ReferenceDocIDF;
                    master.Remarks = request.Master.Remarks ?? master.Remarks;
                    master.EditUserIDF = userId;
                    master.EditEntryDateTime = DateTime.Now;
                    master.EditIPAddress = _currentUser.IpAddress;
                    master.EditBrowserName = _currentUser.Browser;
                    var oldDetails = _context.FeedBackPatientDetails
                        .Where(x => x.FeedBackPatientIDF == master.FeedBackPatientIDP);
                    _context.FeedBackPatientDetails.RemoveRange(oldDetails);
                    await _context.SaveChangesAsync();
                }


                // =========================
                // Retrieve all active feedback options
                // =========================

                var dbOptions =
                    await _context.FeedbackOptionTypes
                    .AsNoTracking()
                    .Where(x =>
                        x.IsCurrentFormat &&
                        x.HospitalGroupIDF ==
                        _currentUser.HospitalGroupId)
                    .ToListAsync();
                // =========================
                // SAVE DETAILS
                // =========================
                var details = request.Details
                    .Where(x => x != null)
                    .ToList();
                // IPD
                if (feedbackMap.OPDIPDFlag != 0)
                {
                    details = details
                        .GroupBy(x => new
                        {
                            x.FeedbackQuestionIDF,
                            x.Type
                        })
                        .Select(x => x.First())
                        .ToList();
                }
                foreach (var item in details)
                {
                    if (item == null)
                        continue;


                    // =========================
                    // TEXT AREA
                    // =========================
                    if (item.Type == 2)
                    {
                        _context.FeedBackPatientDetails.Add(
                            new FeedBackPatientDetail
                            {
                                FeedBackPatientIDF = master.FeedBackPatientIDP,
                                FeedbackCategoryIDF = item.FeedbackCategoryIDF ?? 0,
                                FeedbackQuestionIDF = null,
                                FeedbackOptionsTypeIDF = null,
                                Type = 2,
                                Answer = false,
                                Text = item.TextAnswer?.Trim() ?? ""
                            });

                        continue;
                    }
                    // =========================
                    // YES / NO
                    // =========================
                    if (item.Type == 0 || item.Type == 4)
                    {
                        _context.FeedBackPatientDetails.Add(
                            new FeedBackPatientDetail
                            {
                                FeedBackPatientIDF = master.FeedBackPatientIDP,
                                FeedbackCategoryIDF = item.FeedbackCategoryIDF ?? 0,
                                FeedbackQuestionIDF = item.FeedbackQuestionIDF,
                                FeedbackOptionsTypeIDF = null,
                                Type = item.Type,
                                Answer = item.IsSelected,
                                Text = ""
                            });

                        continue;
                    }
                    // =========================
                    // OPTION BASED
                    // =========================
                    if (item.Type == 1 || item.Type == 3)
                    {
                        if (item.FeedbackQuestionIDF == null)
                            continue;

                        // =========================
                        //  OPD stores the selected option
                        // =========================
                        if (feedbackMap.OPDIPDFlag == 0)
                        {
                            _context.FeedBackPatientDetails.Add(
                                new FeedBackPatientDetail
                                {
                                    FeedBackPatientIDF = master.FeedBackPatientIDP,
                                    FeedbackCategoryIDF = item.FeedbackCategoryIDF ?? 0,
                                    FeedbackQuestionIDF = item.FeedbackQuestionIDF,
                                    FeedbackOptionsTypeIDF = item.SelectedOptionIDF,
                                    Type = item.Type,
                                    Answer = item.SelectedOptionIDF.HasValue,
                                    Text = ""
                                });
                        }
                        // =========================
                        // IPD
                        // =========================
                        else
                        {
                            var options = dbOptions
                                .Where(x => x.TypeEnum == item.Type)
                                .GroupBy(x => x.FeedbackOptionsTypeIDP)
                                .Select(x => x.First())
                                .OrderBy(x => x.SrNo)
                                .ToList();

                            foreach (var option in options)
                            {
                                _context.FeedBackPatientDetails.Add(
                                    new FeedBackPatientDetail
                                    {
                                        FeedBackPatientIDF = master.FeedBackPatientIDP,
                                        FeedbackCategoryIDF =
                                            item.FeedbackCategoryIDF ?? 0,
                                        FeedbackQuestionIDF =
                                            item.FeedbackQuestionIDF,
                                        FeedbackOptionsTypeIDF =
                                            option.FeedbackOptionsTypeIDP,
                                        Type = item.Type,
                                        Answer =
                                            option.FeedbackOptionsTypeIDP ==
                                            item.SelectedOptionIDF,
                                        Text = ""
                                    });
                            }
                        }
                    }
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return master.FeedBackPatientIDP;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region REG CODE

        // =========================
        // GENERATE FEEDBACK REG CODE
        // =========================
        private async Task<string> GenerateRegCode(
            int hospitalId,
            byte registrationType)
        {
            int year = DateTime.Now.Year;


            var lastCode = await _context.FeedBackPatientMasters
                .AsNoTracking()
                .Where(x =>
                    x.HospitalIDF == hospitalId &&
                    x.RegistrationType == registrationType &&
                    x.EntryDate.Year == year)
                .OrderByDescending(x =>
                    x.FeedBackPatientIDP)
                .Select(x =>
                    x.FeedBackRegCode)
                .FirstOrDefaultAsync();



            if (string.IsNullOrWhiteSpace(lastCode))
                return $"{year}00001";



            if (!int.TryParse(
                    lastCode.Substring(4, 5),
                    out int number))
            {
                number = 0;
            }

            number++;
            return $"{year}{number:00000}";
        }
        #endregion

    }
}
