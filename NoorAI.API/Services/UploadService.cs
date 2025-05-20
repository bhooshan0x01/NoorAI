using NoorAI.API.Models;
using NoorAI.API.Repositories.Interfaces;
using NoorAI.API.Services.Interfaces;
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

        // Parse resume and job description to get full text content
        var (userName, userEmail, resumeContent) = await resumeParserService.ParseResume(resume);
        var jobDescriptionContent = await resumeParserService.ParseJobDescription(jobDescription);

        if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userEmail))
        {
            throw new ArgumentException("Could not extract user name or email from the resume. Please ensure the resume contains this information.");
        }

        // Convert files to base64 string for db
        using var resumeMemoryStream = new MemoryStream();
        await resume.CopyToAsync(resumeMemoryStream);
        var resumeBase64Content = Convert.ToBase64String(resumeMemoryStream.ToArray());

        using var jobDescMemoryStream = new MemoryStream();
        await jobDescription.CopyToAsync(jobDescMemoryStream);
        var jobDescBase64Content = Convert.ToBase64String(jobDescMemoryStream.ToArray());

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

        var question = await StartInterviewWithFirstQuestion(interview, resumeContent, jobDescriptionContent);

        return new InterviewResponse(interview.Id, question);
    }

    private async Task<string> StartInterviewWithFirstQuestion(Interview interview, string resumeContent, string jobDescriptionContent)
    {
        try
        {
            var introduction = $"Thank you for sharing your resume and job description, {interview.UserName}. I'm NoorAI, your AI interview assistant. Let's begin the interview.";
            interview.Transcript += $"\nAI: {introduction}";
            await interviewRepository.SaveChangesAsync();

            var question = await ollamaService.GenerateInterviewQuestion(
                resumeContent, 
                jobDescriptionContent, 
                interview.Transcript,
                isFirstQuestion: true);

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