using NoorAI.API.DTOs;
using NoorAI.API.Models;
using NoorAI.API.Repositories.Interfaces;
using NoorAI.API.Services.Interfaces;

namespace NoorAI.API.Services;

public class InterviewService(IInterviewRepository interviewRepository, IOllamaService ollamaService)
    : IInterviewService
{
    private const int MaxQuestions = 6;

    public async Task<InterviewResponse> StartInterview(string resumeContent, string jobDescription, string userName, string userEmail)
    {
        var interview = new Interview
        {
            UserName = userName,
            UserEmail = userEmail,
            ResumeContent = resumeContent,
            JobDescription = jobDescription,
            Transcript = "Interview started.",
            Status = InterviewStatus.InProgress
        };

        await interviewRepository.AddAsync(interview);
        await interviewRepository.SaveChangesAsync();

        var question = await ollamaService.GenerateInterviewQuestion(
            resumeContent,
            jobDescription,
            interview.Transcript);

        interview.Transcript += $"\nAI: {question}";
        await interviewRepository.SaveChangesAsync();

        return new InterviewResponse(interview.Id, question);
    }

    public async Task<InterviewResponse> GetNextQuestion(int interviewId)
    {
        var interview = await interviewRepository.GetByIdAsync(interviewId);
        if (interview == null)
            throw new ArgumentException("Interview not found");

        if (interview.Status == InterviewStatus.Completed)
            throw new InvalidOperationException("Interview is already completed");

        // Count the number of questions asked so far
        var questionCount = interview.Transcript.Split("\nAI:").Length - 1;

        if (questionCount >= MaxQuestions)
        {
            // Add closing message to transcript
            var closingMessage = $"Interview automatically ended after {MaxQuestions} questions at {DateTime.UtcNow:g} UTC.";
            interview.Transcript += $"\nSystem: {closingMessage}";

            // Generate comprehensive feedback
            var feedback = await ollamaService.GenerateInterviewFeedback(
                interview.ResumeContent,
                interview.JobDescription,
                interview.Transcript);

            // Store the feedback and update interview status
            interview.Feedback = feedback;
            interview.Status = InterviewStatus.Completed;
            interview.CompletedAt = DateTime.UtcNow;
            await interviewRepository.SaveChangesAsync();

            return new InterviewResponse(interview.Id, "Interview completed. Thank you for your time!", feedback);
        }

        var question = await ollamaService.GenerateInterviewQuestion(
            interview.ResumeContent,
            interview.JobDescription,
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

        // Add closing message to transcript
        var closingMessage = $"Interview ended by {interview.UserName} at {DateTime.UtcNow:g} UTC.";
        interview.Transcript += $"\nSystem: {closingMessage}";

        // Generate comprehensive feedback
        var feedback = await ollamaService.GenerateInterviewFeedback(
            interview.ResumeContent,
            interview.JobDescription,
            interview.Transcript);

        // Store the feedback and update interview status
        interview.Feedback = feedback;
        interview.Status = InterviewStatus.Completed;
        interview.CompletedAt = DateTime.UtcNow;

        // Save all changes
        await interviewRepository.SaveChangesAsync();

        // Return the complete feedback response
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

    public async Task<InterviewDetailsResponse> UpdateJobDescription(int interviewId, string jobDescription)
    {
        var interview = await interviewRepository.GetByIdAsync(interviewId);
        if (interview == null)
            throw new ArgumentException("Interview not found");

        if (interview.Status == InterviewStatus.Completed)
            throw new InvalidOperationException("Cannot update job description for completed interview");

        interview.JobDescription = jobDescription;
        await interviewRepository.SaveChangesAsync();

        return new InterviewDetailsResponse(
            interview.Id,
            interview.ResumeContent,
            interview.JobDescription,
            interview.Transcript,
            interview.Feedback,
            interview.CreatedAt,
            interview.CompletedAt,
            interview.Status
        );
    }

    public async Task<IEnumerable<InterviewDetailsResponse>> GetAllInterviews()
    {
        var interviews = await interviewRepository.GetAllAsync();
        return interviews.Select(interview => new InterviewDetailsResponse(
            interview.Id,
            interview.ResumeContent,
            interview.JobDescription,
            interview.Transcript,
            interview.Feedback,
            interview.CreatedAt,
            interview.CompletedAt,
            interview.Status
        ));
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