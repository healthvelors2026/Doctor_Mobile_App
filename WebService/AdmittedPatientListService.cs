using DoctorMobileApp.CommonClass;
using DoctorMobileApp.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using static DoctorMobileApp.Models.KioskModel;

namespace DoctorMobileApp.WebService
{
    public class AdmittedPatientListService
    {
        private readonly IDbConnectionFactory _dbHelper;
        private readonly IConfiguration _configuration;
        public AdmittedPatientListService(IDbConnectionFactory db, IConfiguration configuration)
        {
            _dbHelper = db;
            _configuration = configuration;
        }

        public async Task<AdmittedPatientListRequest> GetWardListAsync(int hospitalidf)
        {
            var response = new AdmittedPatientListRequest
            {
                WardList = new List<WardList>(),

            };
            var wardParams = new[]
            {
                    new SqlParameter("@HospitalIDF", hospitalidf)
            };
            var wardTask = await _dbHelper.QueryAsync<WardList>(
                "API_GetWardName",
                CommandType.StoredProcedure,
                wardParams);
            response.WardList = wardTask;
            return response;
        }
        public async Task<AdmittedPatientListRequest> GetDoctorListAsync(int hospitalidf)
        {
            var response = new AdmittedPatientListRequest

            {
                DoctorList = new List<DoctorList>(),
            };
            var doctorParams = new[]
            {
                 new SqlParameter("@HospitalIDF", hospitalidf)
            };
            var doctorTask = await _dbHelper.QueryAsync<DoctorList>("API_DoctorList", CommandType.StoredProcedure, doctorParams);
            response.DoctorList = doctorTask;
            return response;
        }
        public async Task<AdmittedPatientListRequest> GetAdmittedPatientListAsync(AdmittedPatienttRequest request, int hospitalidf)
        {
            var response = new AdmittedPatientListRequest
            {
                WardIDF = request.WardIDF,
                HospitalIDF = hospitalidf,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                WardList = new List<WardList>(),
                DoctorList = new List<DoctorList>(),
                AdmittedPatientList = new List<AdmittedPatientList>()
            };
            var totalRecordsParam = new SqlParameter("@TotalRecords", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            var totalPagesParam = new SqlParameter("@TotalPages", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            var patientParams = new[]
            {
                        new SqlParameter("@WardIDF", request.WardIDF),
                        new SqlParameter("@Filter", request.Filter),
                        new SqlParameter("@PageNumber", request.PageNumber),
                        new SqlParameter("@PageSize", request.PageSize),
                        totalRecordsParam,
                        totalPagesParam,
                        new SqlParameter("@HospitalIDF", hospitalidf),
            };
            response.AdmittedPatientList = await _dbHelper.QueryAsync<AdmittedPatientList>(
            "API_SP_GetWardAdmissionPatientList",
             CommandType.StoredProcedure,
             patientParams);
            response.TotalRecords = totalRecordsParam.Value != DBNull.Value
            ? Convert.ToInt32(totalRecordsParam.Value) : 0;
            response.TotalPages = totalPagesParam.Value != DBNull.Value
           ? Convert.ToInt32(totalPagesParam.Value) : 0;
            return response;
        }
        private static string GetAge(DateTime dateOfBirth, DateTime referenceDate)
        {
            int years = referenceDate.Year - dateOfBirth.Year;
            int months = referenceDate.Month - dateOfBirth.Month;
            int days = referenceDate.Day - dateOfBirth.Day;

            if (days < 0)
            {
                months--;
                days += DateTime.DaysInMonth(referenceDate.AddMonths(-1).Year,
                                             referenceDate.AddMonths(-1).Month);
            }

            if (months < 0)
            {
                years--;
                months += 12;
            }

            if (years > 0)
                return $"{years} Year{(years > 1 ? "s" : "")}";

            if (months > 0)
                return $"{months} Month{(months > 1 ? "s" : "")}";

            return $"{days} Day{(days > 1 ? "s" : "")}";
        }
        public async Task<BedTransferResponse> GetBedTransferAsync(BedTransferRequest request, int hospitalidf)
        {
            var response = new BedTransferResponse();

            var patientParams = new[]
            {
                new SqlParameter("@HospitalIDP", hospitalidf),
                new SqlParameter("@PatientIDF", request.PatientID)
            };
            var patient = (await _dbHelper.QueryAsync<BedTransferPatientDBModel>(
                "DoctorApp_API_GetBedTransferPatientDetail", CommandType.StoredProcedure, patientParams)).FirstOrDefault();

            if (patient == null)
                return response;

            response.AdmissionID = patient.IPDAdmissionDischargeIDP;
            response.IPDRegistrationCode = patient.IPDRegistrationCode;
            response.PatientName = $"{patient.FName} {patient.MName} {patient.LName}".Trim();
            response.AdmissionDate = patient.AdmissionDateTime.ToString("dd/MM/yyyy hh:mm tt");
            response.ClassName = patient.ClassName;
            switch (patient.Gender)
            {
                case 0:
                    response.Gender = "Female";
                    break;

                case 1:
                    response.Gender = "Male";
                    break;

                case 2:
                    response.Gender = "Trans";
                    break;

                default:
                    response.Gender = string.Empty;
                    break;
            }
            response.ConsultDoctor = $"{patient.EmpFName} {patient.EmpMName} {patient.EmpLName}".Trim();
            response.Age = GetAge(patient.DateOfBirth, patient.AdmissionDateTime);

            var historyParams = new[]
            {
                new SqlParameter("@PatientID", request.PatientID),
                new SqlParameter("@HospitalIDF", hospitalidf)
            };
            var history = await _dbHelper.QueryAsync<BedTransferHistoryModel>("SP_GetAdmissionDischargeWithDetailForBedTransfer",
                          CommandType.StoredProcedure,
                          historyParams);

            foreach (var item in history)
            {
                item.TransferBy = item.UserName ?? string.Empty;

                item.Mode = item.TransferType switch
                {
                    0 => "-",

                    1 => string.IsNullOrWhiteSpace(item.ReBedname)
                        ? "Transferred"
                        : $"Transferred From Bed {item.ReBedname}",

                    2 => string.IsNullOrWhiteSpace(item.ReBedname)
                        ? "Moved"
                        : $"Moved From Bed {item.ReBedname}",

                    3 => string.IsNullOrWhiteSpace(item.ReBedname)
                        ? "Swapped"
                        : $"Swapped From Bed {item.ReBedname}",

                    _ => string.Empty
                };
            }
            response.BedTransferHistory = history;
            return response;
        }

        public async Task<List<AddmisionCheckResponse>> GetAddmisionCheckListAsync(AddmisionCheckRequest request, int hospitalIDF, int hospitalGroupIDF)
        {
            var parameters = new[]
            {
                new SqlParameter("@Type", request.CheckListType),
                new SqlParameter("@NonActive", request.NonActive),
                new SqlParameter("@AdmissionIDF", request.AdmissionIDF),
                new SqlParameter("@RegistrationIDF", request.RegistrationIDF),
                new SqlParameter("@RegistrationType", request.RegistrationType),
                new SqlParameter("@HospitalGroupIDF", hospitalGroupIDF),
                new SqlParameter("@HospitalIDF", hospitalIDF)
            };

            var result = await _dbHelper.QueryAsync<AddmisionCheckResponse>("GET_IPDCheckListQuestionDetail",
                CommandType.StoredProcedure,
                parameters);

            return result;
        }

        public async Task<BedTransferEditResponse?> GetBedTransferEditAsync(BedTransferEditRequest request, int hospitalIDF, int hospitalGroupIDF)
        {
            var patientParameters = new[]
            {
                new SqlParameter("@HospitalIDP", hospitalIDF),
                new SqlParameter("@PatientIDF", request.PatientIDF)
            };

            var patient = (await _dbHelper.QueryAsync<BedTransferPatientDBModel>(
                            "DoctorApp_API_GetBedTransferPatientDetail",
                            CommandType.StoredProcedure, patientParameters)).FirstOrDefault();
            if (patient == null)
            {
                return null;
            }
            var response = new BedTransferEditResponse
            {
                AdmissionID = patient.IPDAdmissionDischargeIDP,
                CurrentTrackingID = patient.IPDBedAmenityTrackingIDP,
                CurrentBedID = patient.BedIDP,
                CurrentBed = patient.BedName ?? string.Empty,
                CurrentWardID = patient.WardIDP,
                CurrentWard = patient.WardName ?? string.Empty,
                CurrentWardTypeID = patient.WardTypeIDF,
                FromDate = patient.FromDate,
                TransferDate = DateTime.Now,
                IsDayCare = patient.IsDayCare
            };

            var bedParameters = new[]
            {
                new SqlParameter("@HospitalIDF", hospitalIDF),
                new SqlParameter("@CurrentBedIDF", patient.BedIDP),
                new SqlParameter("@CurrentWardIDF", patient.WardIDP),
                new SqlParameter("@IsDayCare", patient.IsDayCare)
            };

            var availableBeds = await _dbHelper.QueryAsync<AvailableBedDBModel>("DoctorApp_API_GetAvailableBedsForTransfer",
                    CommandType.StoredProcedure,
                    bedParameters);

            response.AvailableBeds = availableBeds
                .Select(x => new BedDropdownModel
                {
                    BedID = x.BedIDP,
                    BedName = x.BedName ?? string.Empty,
                    WardID = x.WardIDP,
                    WardName = x.WardName ?? string.Empty
                })
                .ToList();
            var checklistParameters = new[]
            {
                new SqlParameter("@Type", (int)EnumIsCheckList.EnumAdmission),
                new SqlParameter("@NonActive", true),
                new SqlParameter("@AdmissionIDF",patient.IPDAdmissionDischargeIDP),
                new SqlParameter("@RegistrationIDF", 0),
                new SqlParameter("@RegistrationType",(int)EnumOPDIPDFlag.EnmIPD),
                new SqlParameter("@HospitalIDF", hospitalIDF),
                new SqlParameter("@HospitalGroupIDF", hospitalGroupIDF)
            };
            var admissionChecklist = await _dbHelper.QueryAsync<AddmisionCheckResponse>("GET_IPDCheckListQuestionDetail", CommandType.StoredProcedure, checklistParameters);
            response.AdmissionChecklist = admissionChecklist.Select(x => new CheckListQuestionModel
            {
                CheckListQuestionIDP = x.CheckListQuestionIDP,
                Question = x.Question ?? string.Empty,
                Value = x.Value,
                Remarks = x.Remarks ?? string.Empty,
                CategoryName = x.CategoryName ?? string.Empty
            })
                .ToList();

            var swapPatientParameters = new[]
            {
                new SqlParameter("@HospitalIDF", hospitalIDF),
                new SqlParameter("@CurrentAdmissionIDF",patient.IPDAdmissionDischargeIDP),
                new SqlParameter("@CurrentBedIDF", patient.BedIDP),
                new SqlParameter("@IsDayCare", patient.IsDayCare)

            };

            var swapPatients =
                await _dbHelper.QueryAsync<SwapPatientModel>(
                    "DoctorApp_API_GetSwapPatients",
                    CommandType.StoredProcedure,
                    swapPatientParameters);

            response.SwapPatients = swapPatients.ToList();
            return response;
        }
        public async Task<SaveBedTransferResponse> SaveBedTransferAsync(SaveBedTransferRequest request,int hospitalIDF,int userIDF,int employeeIDF)
        {
            if (request.AdmissionID <= 0)
            {
                return new SaveBedTransferResponse
                {
                    Success = false,
                    ResultCode = -1,
                    Message = "Valid admission ID is required."
                };
            }

            if (request.CurrentBedID <= 0)
            {
                return new SaveBedTransferResponse
                {
                    Success = false,
                    ResultCode = -1,
                    Message = "Valid current bed ID is required."
                };
            }

            if (request.ToBedID <= 0)
            {
                return new SaveBedTransferResponse
                {
                    Success = false,
                    ResultCode = -1,
                    Message = "Valid destination bed ID is required."
                };
            }

            if (request.CurrentBedID == request.ToBedID)
            {
                return new SaveBedTransferResponse
                {
                    Success = false,
                    ResultCode = -1,
                    Message = "Current bed and destination bed cannot be the same."
                };
            }

            if (request.TransferDate == default)
            {
                return new SaveBedTransferResponse
                {
                    Success = false,
                    ResultCode = -1,
                    Message = "Transfer date and time are required."
                };
            }

            if (!string.IsNullOrWhiteSpace(request.Remarks) && request.Remarks.Length > 250)
            {
                return new SaveBedTransferResponse
                {
                    Success = false,
                    ResultCode = -1,
                    Message = "Remarks cannot exceed 250 characters."
                };
            }

            if (request.ICUChargeType is < 0 or > 2)
            {
                return new SaveBedTransferResponse
                {
                    Success = false,
                    ResultCode = 11,
                    Message = "Invalid ICU charge type."
                };
            }

            if (hospitalIDF <= 0 || userIDF <= 0 || employeeIDF <= 0)
            {
                return new SaveBedTransferResponse
                {
                    Success = false,
                    ResultCode = -401,
                    Message = "Authenticated hospital, user, and employee information is required."
                };
            }

            var parameters = new[]
            {
        new SqlParameter("@HospitalIDF", hospitalIDF)
        {
            Value = hospitalIDF
        },
        new SqlParameter("@AdmissionIDF", SqlDbType.Int)
        {
            Value = request.AdmissionID
        },
        new SqlParameter("@CurrentBedIDF", SqlDbType.Int)
        {
            Value = request.CurrentBedID
        },
        new SqlParameter("@ToBedIDF", SqlDbType.Int)
        {
            Value = request.ToBedID
        },
        new SqlParameter("@TransferDateTime", SqlDbType.SmallDateTime)
        {
            Value = request.TransferDate
        },
        new SqlParameter("@Remarks", SqlDbType.NVarChar, 250)
        {
            Value = string.IsNullOrWhiteSpace(request.Remarks)
                ? DBNull.Value
                : request.Remarks.Trim()
        },
        new SqlParameter("@EmployeeIDF", employeeIDF)
        {
            Value = employeeIDF
        },
        new SqlParameter("@UserIDF", userIDF)
        {
            Value = userIDF
        },
        new SqlParameter("@IsIcuBed", SqlDbType.Bit)
        {
            Value = request.IsIcuBed
        },
        new SqlParameter("@ICUChargeType", SqlDbType.TinyInt)
        {
             Value = request.ICUChargeType ?? 0
        }
    };
            try
            {
                var result = await _dbHelper.QueryAsync<SaveBedTransferDBModel>("DoctorApp_API_SaveBedTransfer",CommandType.StoredProcedure,parameters);

                var dbResult = result.FirstOrDefault();

                if (dbResult == null)
                {
                    return new SaveBedTransferResponse
                    {
                        Success = false,
                        ResultCode = -500,
                        Message = "Bed transfer could not be completed."
                    };
                }

                return new SaveBedTransferResponse
                {
                    Success = dbResult.Success,
                    ResultCode = dbResult.ResultCode < 0 ? -500 : dbResult.ResultCode,
                    Message = dbResult.ResultCode < 0
                        ? "An unexpected error occurred while completing the bed transfer."
                        : dbResult.Message,
                    TrackingID = dbResult.TrackingID
                };
            }
            catch (Exception ex)
            {
                _dbHelper.LogError(ex, "DoctorApp_API_SaveBedTransfer");
                return new SaveBedTransferResponse
                {
                    Success = false,
                    ResultCode = -500,
                    Message = "An unexpected error occurred while completing the bed transfer."
                };
            }
        }
    }
}
//public async Task<RequestBedTransferResponse> RequestBedTransferAsync(RequestBedTransferRequest request,
//    int hospitalIDF,int hospitalGroupIDF,
//    int userIDF,
//    int employeeIDF)
//{
//    RequestBedTransferResponse Invalid(string message, int code = -1) => new()
//    {
//        Success = false,
//        ResultCode = code,
//        Message = message
//    };

//    if (request == null)
//    {
//        return Invalid("Request body is required.");
//    }

//    if (request.AdmissionID <= 0 || request.CurrentBedID <= 0 || request.ToBedID <= 0)
//    {
//        return Invalid("Valid admission, current bed, and destination bed IDs are required.");
//    }

//    if (request.CurrentBedID == request.ToBedID)
//    {
//        return Invalid("Current bed and destination bed cannot be the same.");
//    }

//    if (request.TransferDate == default)
//    {
//        return Invalid("Transfer date and time are required.");
//    }

//    if (!string.IsNullOrWhiteSpace(request.Remarks) && request.Remarks.Length > 250)
//    {
//        return Invalid("Remarks cannot exceed 250 characters.");
//    }

//    if (hospitalIDF <= 0 || hospitalGroupIDF <= 0 || userIDF <= 0 || employeeIDF <= 0)
//    {
//        return Invalid("Authenticated hospital, hospital group, user, and employee information is required.", -401);
//    }

//    if (request.AmenityIDs.Any(x => x <= 0))
//    {
//        return Invalid("Amenity IDs must be positive.");
//    }

//    if (request.AmenityIDs.Count != request.AmenityIDs.Distinct().Count())
//    {
//        return Invalid("Duplicate amenity IDs are not allowed.");
//    }

//    var amenityIDs = request.AmenityIDs.Count > 0
//        ? string.Join(",", request.AmenityIDs)
//        : null;

//    var parameters = new[]
//    {
//        new SqlParameter("@HospitalIDF", SqlDbType.Int) { Value = hospitalIDF },
//        new SqlParameter("@HospitalGroupIDF", SqlDbType.Int) { Value = hospitalGroupIDF },
//        new SqlParameter("@AdmissionIDF", SqlDbType.Int) { Value = request.AdmissionID },
//        new SqlParameter("@CurrentBedIDF", SqlDbType.Int) { Value = request.CurrentBedID },
//        new SqlParameter("@ToBedIDF", SqlDbType.Int) { Value = request.ToBedID },
//        new SqlParameter("@RequestDateTime", SqlDbType.SmallDateTime) { Value = request.TransferDate },
//        new SqlParameter("@Remarks", SqlDbType.NVarChar, 250)
//        {
//            Value = string.IsNullOrWhiteSpace(request.Remarks)
//                ? DBNull.Value
//                : request.Remarks.Trim()
//        },
//        new SqlParameter("@EmployeeIDF", SqlDbType.Int) { Value = employeeIDF },
//        new SqlParameter("@UserIDF", SqlDbType.Int) { Value = userIDF },
//        new SqlParameter("@AmenityIDs", SqlDbType.NVarChar, -1)
//        {
//            Value = string.IsNullOrWhiteSpace(amenityIDs)
//                ? DBNull.Value
//                : amenityIDs
//        }
//    };

//    try
//    {
//        var result = await _dbHelper.QueryAsync<RequestBedTransferDBModel>(
//            "DoctorApp_API_RequestBedTransfer",
//            CommandType.StoredProcedure,
//            parameters);

//        var dbResult = result.FirstOrDefault();
//        if (dbResult == null)
//        {
//            return Invalid("The bed transfer request could not be completed.", -500);
//        }

//        if (dbResult.ResultCode < 0 && dbResult.ResultCode != -401 && dbResult.ResultCode != -500)
//        {
//            return Invalid("The bed transfer request could not be completed.", -500);
//        }

//        return new RequestBedTransferResponse
//        {
//            Success = dbResult.Success,
//            ResultCode = dbResult.ResultCode,
//            Message = dbResult.Message,
//            BedStatusTrackingID = dbResult.BedStatusTrackingID,
//            AdmissionID = dbResult.AdmissionID,
//            FromBedID = dbResult.FromBedID,
//            ToBedID = dbResult.ToBedID
//        };
//    }
//    catch (Exception ex)
//    {
//        _dbHelper.LogError(ex, "DoctorApp_API_RequestBedTransfer");
//        return Invalid("An unexpected error occurred while creating the bed transfer request.", -500);
//    }
//}
