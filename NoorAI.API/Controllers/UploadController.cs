using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NoorAI.API.Services.Interfaces;

namespace NoorAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadController(IUploadService uploadService) : ControllerBase
{
    [HttpPost]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> Upload([FromForm] IFormFile file)
    {
        try
        {
            var response = await uploadService.UploadResumeAsync(file);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}