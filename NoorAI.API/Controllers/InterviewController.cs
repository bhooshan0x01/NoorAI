using Microsoft.AspNetCore.Mvc;
using NoorAI.API.DTOs;
using NoorAI.API.Services.Interfaces;

namespace NoorAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InterviewController(IInterviewService interviewService) : ControllerBase
{
    
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
    
    [HttpGet("summaries")]
    public async Task<ActionResult<IEnumerable<InterviewSummaryResponse>>> GetInterviewSummaries()
    {
        try
        {
            var response = await interviewService.GetInterviewSummaries();
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}/full")]
    public async Task<ActionResult<InterviewSummaryResponse>> GetInterviewFullDetails(int id)
    {
        try
        {
            var response = await interviewService.GetInterviewFullDetails(id);
            if (response == null)
                return NotFound(new { error = "Interview not found" });
                
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
} 