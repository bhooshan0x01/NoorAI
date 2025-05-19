using Microsoft.AspNetCore.Http;
using NoorAI.API.Models;
using NoorAI.API.Repositories.Interfaces;
using NoorAI.API.Services.Interfaces;
using System.Text;
using NoorAI.API.DTOs;

namespace NoorAI.API.Services;

public class UploadService(IInterviewRepository interviewRepository, IOllamaService ollamaService) : IUploadService
{
    public async Task<InterviewResponse> UploadResumeAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("No file uploaded");

        // Convert file to base64 string
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var fileBytes = memoryStream.ToArray();
        var base64Content = Convert.ToBase64String(fileBytes);

        // Get the most recent job description from the database
        var jobDescription = await interviewRepository.GetFirstJobDescriptionAsync();
        if (string.IsNullOrEmpty(jobDescription))
            throw new InvalidOperationException("No job description found in the database. Please upload a job description first.");

        var interview = new Interview
        {
            ResumeContent = base64Content,
            JobDescription = jobDescription,
            Transcript = "Resume uploaded.",
            Status = InterviewStatus.InProgress
        };

        await interviewRepository.AddAsync(interview);
        await interviewRepository.SaveChangesAsync();

        // Start the interview with just the first question
        var question = await StartInterviewWithFirstQuestion(interview);

        return new InterviewResponse(interview.Id, question);
    }

    private async Task<string> StartInterviewWithFirstQuestion(Interview interview)
    {
        try
        {
            // Add introduction
            var introduction = "Thank you for sharing your resume. I'm NoorAI, your AI interview assistant. Let's begin the interview.";
            interview.Transcript += $"\nAI: {introduction}";
            await interviewRepository.SaveChangesAsync();

            // Generate and ask first question
            var question = await ollamaService.GenerateInterviewQuestion(
                interview.ResumeContent,
                interview.JobDescription,
                interview.Transcript);

            interview.Transcript += $"\nAI: {question}";
            await interviewRepository.SaveChangesAsync();

            return interview.Transcript;
        }
        catch (Exception ex)
        {
            interview.Transcript += $"\nError during interview process: {ex.Message}";
            interview.Status = InterviewStatus.Completed;
            await interviewRepository.SaveChangesAsync();
            throw;
        }
    }
}