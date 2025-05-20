using NoorAI.API.DTOs;
using NoorAI.API.Models;
using NoorAI.API.Repositories.Interfaces;
using NoorAI.API.Services.Interfaces;

namespace NoorAI.API.Services;

public class InterviewService(IInterviewRepository interviewRepository, IOllamaService ollamaService, ResumeParserService resumeParserService)
    : IInterviewService
{
    private const int MaxQuestions = 6;

    private IFormFile? BytesToFile(byte[] bytes, string fileName)
    {
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "file", fileName);
    }

    private IFormFile? DecodeBase64ToFile(string base64Content, string fileName)
    {
        if (string.IsNullOrEmpty(base64Content)) return null;
        try
        {
            var bytes = Convert.FromBase64String(base64Content);
            return BytesToFile(bytes, fileName);
        }
        catch (FormatException)
        {
            return null;
        }
    }

    public async Task<InterviewResponse> GetNextQuestion(int interviewId)
    {
        var interview = await interviewRepository.GetByIdAsync(interviewId);
        if (interview == null)
            throw new ArgumentException("Interview not found");

        if (interview.Status == InterviewStatus.Completed)
            throw new InvalidOperationException("Interview is already completed");

        var questionCount = interview.Transcript.Split("\nAI:").Length - 1;

        if (questionCount >= MaxQuestions)
        {
            var closingMessage = $"Interview automatically ended after {MaxQuestions} questions at {DateTime.UtcNow:g} UTC.";
            interview.Transcript += $"\nSystem: {closingMessage}";

            var resumeFile = DecodeBase64ToFile(interview.ResumeContent, "resume.pdf");
            var jobDescriptionFile = DecodeBase64ToFile(interview.JobDescription, "job_description.pdf");

            if (resumeFile == null || jobDescriptionFile == null) {
                 throw new InvalidOperationException("Could not decode or process stored file content for feedback.");
            }
            
            var parsedResume = await resumeParserService.ParseResume(resumeFile);
            var parsedJobDescriptionText = await resumeParserService.ParseJobDescription(jobDescriptionFile);

            var feedback = await ollamaService.GenerateInterviewFeedback(
                parsedResume.FullText, 
                parsedJobDescriptionText,
                interview.Transcript);

            interview.Feedback = feedback;
            interview.Status = InterviewStatus.Completed;
            interview.CompletedAt = DateTime.UtcNow;
            await interviewRepository.SaveChangesAsync();

            return new InterviewResponse(interview.Id, "Interview completed. Thank you for your time!", feedback);
        }

        var resumeFileForQuestion = DecodeBase64ToFile(interview.ResumeContent, "resume.pdf");
        var jobDescriptionFileForQuestion = DecodeBase64ToFile(interview.JobDescription, "job_description.pdf");

        if (resumeFileForQuestion == null || jobDescriptionFileForQuestion == null) {
             throw new InvalidOperationException("Could not decode or process stored file content for question generation.");
        }

        var parsedResumeForQuestion = await resumeParserService.ParseResume(resumeFileForQuestion);
        var parsedJobDescriptionTextForQuestion = await resumeParserService.ParseJobDescription(jobDescriptionFileForQuestion);

        var question = await ollamaService.GenerateInterviewQuestion(
            parsedResumeForQuestion.FullText, 
            parsedJobDescriptionTextForQuestion,
            interview.Transcript);

        interview.Transcript += $"\nAI: {question}";
        await interviewRepository.SaveChangesAsync();

        return new InterviewResponse(interview.Id, question);
    }

    public async Task<InterviewFeedbackResponse> EndInterview(int interviewId)
    {
        var interview = await interviewRepository.GetByIdAsync(interviewId);
        if (interview == null)
            throw new ArgumentException("Interview not found");

        if (interview.Status == InterviewStatus.Completed)
            throw new InvalidOperationException("Interview is already completed");

        var closingMessage = $"Interview ended by {interview.UserName} at {DateTime.UtcNow:g} UTC.";
        interview.Transcript += $"\nSystem: {closingMessage}";

        var resumeFileForFeedback = DecodeBase64ToFile(interview.ResumeContent, "resume.pdf");
        var jobDescriptionFileForFeedback = DecodeBase64ToFile(interview.JobDescription, "job_description.pdf");

        if (resumeFileForFeedback == null || jobDescriptionFileForFeedback == null) {
             throw new InvalidOperationException("Could not decode or process stored file content for feedback.");
        }

        var parsedResumeForFeedback = await resumeParserService.ParseResume(resumeFileForFeedback);
        var parsedJobDescriptionTextForFeedback = await resumeParserService.ParseJobDescription(jobDescriptionFileForFeedback);

        var feedback = await ollamaService.GenerateInterviewFeedback(
            parsedResumeForFeedback.FullText,
            parsedJobDescriptionTextForFeedback,
            interview.Transcript);

        interview.Feedback = feedback;
        interview.Status = InterviewStatus.Completed;
        interview.CompletedAt = DateTime.UtcNow;

        await interviewRepository.SaveChangesAsync();

        return new InterviewFeedbackResponse(
            interview.Id,
            feedback,
            interview.Transcript,
            interview.UserName,
            interview.UserEmail,
            interview.CreatedAt,
            interview.CompletedAt
        );
    }

    public async Task<IEnumerable<InterviewSummaryResponse>> GetInterviewSummaries()
    {
        var interviews = await interviewRepository.GetAllAsync();
        return interviews.Select(interview => new InterviewSummaryResponse(
            interview.Id,
            interview.UserName,
            interview.UserEmail,
            interview.JobDescription,
            interview.Transcript,
            interview.Feedback,
            interview.CreatedAt,
            interview.CompletedAt,
            interview.Status,
            interview.Transcript.Split("\nAI:").Length - 1,
            interview.CompletedAt.HasValue ? interview.CompletedAt.Value - interview.CreatedAt : null
        ));
    }

    public async Task<InterviewSummaryResponse?> GetInterviewFullDetails(int id)
    {
        var interview = await interviewRepository.GetByIdAsync(id);
        if (interview == null)
            return null;

        return new InterviewSummaryResponse(
            interview.Id,
            interview.UserName,
            interview.UserEmail,
            interview.JobDescription,
            interview.Transcript,
            interview.Feedback,
            interview.CreatedAt,
            interview.CompletedAt,
            interview.Status,
            interview.Transcript.Split("\nAI:").Length - 1,
            interview.CompletedAt.HasValue ? interview.CompletedAt.Value - interview.CreatedAt : null
        );
    }
} 