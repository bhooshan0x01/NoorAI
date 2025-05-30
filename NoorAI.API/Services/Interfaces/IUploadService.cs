using NoorAI.API.DTOs;

namespace NoorAI.API.Services.Interfaces;

public interface IUploadService
{
    Task<InterviewResponse> UploadFilesAsync(IFormFile resume, IFormFile jobDescription);
}