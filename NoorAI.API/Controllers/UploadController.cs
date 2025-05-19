using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NoorAI.API.DTOs;
using NoorAI.API.Services.Interfaces;

namespace NoorAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadController(IUploadService uploadService) : ControllerBase
{
    [HttpPost]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> Upload([FromForm] UploadRequestDto request)
    {
        try
        {
            if (!IsPdfFile(request.Resume) || !IsPdfFile(request.JobDescription))
            {
                return BadRequest(new { error = "Both files must be in PDF format" });
            }

            var response = await uploadService.UploadFilesAsync(request.Resume, request.JobDescription);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private static bool IsPdfFile(IFormFile file)
    {
        return file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase);
    }
}