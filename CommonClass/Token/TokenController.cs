using DoctorMobileApp.CommonClass.Services.Interface;
using DoctorMobileApp.DTOs.PatientFeedbackDto;
using DoctorMobileApp.WebService.PatientFeedback.Interface;
using Microsoft.AspNetCore.Mvc;

namespace DoctorMobileApp.commonclass.CommonController
{
    [ApiController]
    [Route("api/[controller]")]
    public class TokenController : ControllerBase
    {
        private readonly IFeedbackTokenService _feedbackTokenService;
        private readonly ICurrentUserService _currentUser;

        public TokenController(
     IFeedbackTokenService feedbackTokenService,
     ICurrentUserService currentUser)
        {
            _feedbackTokenService = feedbackTokenService;
            _currentUser = currentUser;
        }
        #region Generate Token


        [HttpPost("GenerateToken")]
        public async Task<IActionResult> GenerateToken(
            [FromBody] GenerateFeedbackTokenRequestDto request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Invalid request."
                    });
                }

                int hospitalId = _currentUser.HospitalId;

                var token = await _feedbackTokenService.GenerateTokenAsync(
                    request.PatientId,
                    request.RegistrationId,
                    request.opdIpdFlag,
                    hospitalId);

                // Generate feedback URL dynamically
                var feedbackUrl = Url.Action(
                    action: "OpdFeedback",
                    controller: "Home",
                    values: new { token = token.Token },
                    protocol: Request.Scheme);

                var response = new GenerateFeedbackTokenResponseDto
                {
                    Success = true,
                    Message = "Token generated successfully.",
                    Token = token.Token,
                    Url = feedbackUrl!
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new
                    {
                        Success = false,
                        Message = ex.Message
                    });
            }
        }

        #endregion

     

        #region Resolve Token

        [HttpGet("ResolveToken")]
        public async Task<IActionResult> ResolveToken(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Token is required."
                    });
                }
                var result = await _feedbackTokenService.ResolveTokenAsync(token);
                if (result == null)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = "Invalid or expired token."
                    });
                }
                return Ok(new
                {
                    Success = true,
                    PatientId = result.PatientIDF,
                    RegistrationId = result.RegistrationIDF,
                    OpdIpdFlag = result.RegistrationType
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new
                    {
                        Success = false,
                        Message = ex.Message
                    });
            }
        }

        #endregion
       
    }
}