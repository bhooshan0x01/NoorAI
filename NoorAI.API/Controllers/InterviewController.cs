using Microsoft.AspNetCore.Mvc;
using NoorAI.API.DTOs;
using NoorAI.API.Services.Interfaces;

namespace NoorAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InterviewController(IInterviewService interviewService) : ControllerBase
{
    [HttpPost("start")]
    public async Task<ActionResult<InterviewResponse>> StartInterview([FromBody] StartInterviewRequest request)
    {
        try
        {
            var response = await interviewService.StartInterview(request.ResumeContent, request.JobDescription);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("respond")]
    public async Task<ActionResult<InterviewResponse>> RespondToQuestion([FromBody] InterviewResponseRequest request)
    {
        try
        {
            var response = await interviewService.GetNextQuestion(request.InterviewId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("end")]
    public async Task<ActionResult<InterviewFeedbackResponse>> EndInterview([FromBody] EndInterviewRequest request)
    {
        try
        {
            var response = await interviewService.EndInterview(request.InterviewId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("job-description")]
    public async Task<ActionResult<InterviewDetailsResponse>> UpdateJobDescription([FromBody] UpdateJobDescriptionRequest request)
    {
        try
        {
            var response = await interviewService.UpdateJobDescription(request.InterviewId, request.JobDescription);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InterviewDetailsResponse>>> GetAllInterviews()
    {
        try
        {
            var response = await interviewService.GetAllInterviews();
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
} 