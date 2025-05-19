using NoorAI.API.DTOs;

namespace NoorAI.API.Services.Interfaces;

public interface IInterviewService
{
    Task<InterviewResponse> StartInterview(string resumeContent, string jobDescription);
    Task<InterviewResponse> GetNextQuestion(int interviewId);
    Task<InterviewFeedbackResponse> EndInterview(int interviewId);
    Task<InterviewDetailsResponse> UpdateJobDescription(int interviewId, string jobDescription);
    Task<IEnumerable<InterviewDetailsResponse>> GetAllInterviews();
} 