using Microsoft.AspNetCore.Http;

namespace NoorAI.API.DTOs;

public class UploadRequestDto
{
    public IFormFile Resume { get; set; } = null!;
    public IFormFile JobDescription { get; set; } = null!;
}