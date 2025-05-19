using Microsoft.AspNetCore.Http;
using NoorAI.API.DTOs;

namespace NoorAI.API.Services.Interfaces;

public interface IUploadService
{
    Task<InterviewResponse> UploadResumeAsync(IFormFile file);
}