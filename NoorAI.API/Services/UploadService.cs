using Microsoft.AspNetCore.Http;
using NoorAI.API.Models;
using NoorAI.API.Repositories.Interfaces;
using NoorAI.API.Services.Interfaces;
using System.Text;
using NoorAI.API.DTOs;

namespace NoorAI.API.Services;

public class UploadService(
    IInterviewRepository interviewRepository, 
    IOllamaService ollamaService,
    ResumeParserService resumeParserService) : IUploadService
{
    public async Task<InterviewResponse> UploadFilesAsync(IFormFile resume, IFormFile jobDescription)
    {
        if (resume == null || resume.Length == 0)
            throw new ArgumentException("No resume file uploaded");

        if (jobDescription == null || jobDescription.Length == 0)
            throw new ArgumentException("No job description file uploaded");

        // Parse user info from resume
        var (userName, userEmail) = await resumeParserService.ParseResumeInfo(resume);
        
        if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userEmail))
        {
            throw new ArgumentException("Could not extract user name or email from the resume. Please ensure the resume contains this information.");
        }

        // Convert resume to base64 string
        using var resumeMemoryStream = new MemoryStream();
        await resume.CopyToAsync(resumeMemoryStream);
        var resumeBytes = resumeMemoryStream.ToArray();
        var resumeBase64Content = Convert.ToBase64String(resumeBytes);

        // Convert job description to base64 string
        using var jobDescMemoryStream = new MemoryStream();
        await jobDescription.CopyToAsync(jobDescMemoryStream);
        var jobDescBytes = jobDescMemoryStream.ToArray();
        var jobDescBase64Content = Convert.ToBase64String(jobDescBytes);

        var interview = new Interview
        {
            UserName = userName,
            UserEmail = userEmail,
            ResumeContent = resumeBase64Content,
            JobDescription = jobDescBase64Content,
            Transcript = "Resume and job description uploaded.",
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
            var introduction = $"Thank you for sharing your resume and job description, {interview.UserName}. I'm NoorAI, your AI interview assistant. Let's begin the interview.";
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