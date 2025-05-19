namespace NoorAI.API.DTOs;

public class UploadRequestDto
{
    public required Guid UserId { get; set; }
    public required IFormFile Resume { get; set; }
    public required IFormFile JobDescription { get; set; }

}