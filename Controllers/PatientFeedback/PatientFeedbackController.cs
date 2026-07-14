using DoctorMobileApp.DTOs.PatientFeedbackDto;
using DoctorMobileApp.WebService.patientFeedback.Interface;
using DoctorMobileApp.WebService.PatientFeedback.Interface;
using Microsoft.AspNetCore.Mvc;

namespace DoctorMobileApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientFeedbackController : ControllerBase
    {
        private readonly IPatientFeedbackService _patientFeedbackService;
        private readonly IFeedbackTokenService _feedbackTokenService;

        public PatientFeedbackController(
            IPatientFeedbackService patientFeedbackService,
            IFeedbackTokenService feedbackTokenService)
        {
            _patientFeedbackService = patientFeedbackService;
            _feedbackTokenService = feedbackTokenService;
        }


        [HttpGet("feedback")]
        public async Task<IActionResult> GetFeedback(
            [FromQuery] int patientId,
            [FromQuery] int registrationId,
            [FromQuery] byte opdIpdFlag)
        {
            try
            {
                if (patientId <= 0 || registrationId <= 0)
                    return BadRequest("Invalid patientId or registrationId");


                if (opdIpdFlag!= 0 && opdIpdFlag!= 1)
                    return BadRequest("opdIpdFlagmust be 0(OPD) or 1(IPD)");


                var result = await _patientFeedbackService.GetFeedbackAsync(
                    patientId,
                    registrationId,
                    opdIpdFlag);


                if (result == null)
                    return NotFound();


                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }


        #region Save Feedback

        [HttpPost("save")]
        public async Task<IActionResult> SaveFeedback([FromBody] SaveFeedbackRequestDto request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Invalid request.");

                if (string.IsNullOrWhiteSpace(request.Token))
                    return BadRequest("Token is required.");

                if (request.Master == null)
                    return BadRequest("Master data is required.");

                if (request.Details == null || request.Details.Count == 0)
                    return BadRequest("Feedback details are required.");

                // DEBUG
                Console.WriteLine("====================================");
                Console.WriteLine($"DETAIL COUNT : {request.Details.Count}");

                foreach (var item in request.Details)
                {
                    Console.WriteLine(
                        $"Question={item.FeedbackQuestionIDF} | " +
                        $"Option={item.SelectedOptionIDF} | " +
                        $"Type={item.Type} | " +
                        $"Selected={item.IsSelected}");
                }

                // Resolve token
                var tokenInfo = await _feedbackTokenService.ResolveTokenAsync(request.Token);

                if (tokenInfo == null)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Invalid or expired feedback link."
                    });
                }

                // Never trust browser values
                request.Master.PatientIDF = tokenInfo.PatientIDF;
                request.Master.RegistrationIDF = tokenInfo.RegistrationIDF;
                request.Master.RegistrationType = tokenInfo.RegistrationType;

                var feedbackPatientId =
                    await _patientFeedbackService.SaveFeedbackAsync(request);

                await _feedbackTokenService.MarkTokenUsedAsync(request.Token);

                return Ok(new
                {
                    Success = true,
                    FeedbackPatientID = feedbackPatientId,
                    Message = "Feedback saved successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new
                    {
                        Success = false,
                        Message = ex.ToString()
                    });
            }
        }

       
        #endregion
    }
}