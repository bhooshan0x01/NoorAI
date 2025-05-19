using NoorAI.API.DTOs;

namespace NoorAI.API.Services.Interfaces;

public interface IInterviewService
{
    Task<InterviewResponse> GetNextQuestion(int interviewId);
    Task<InterviewFeedbackResponse> EndInterview(int interviewId);
    Task<IEnumerable<InterviewSummaryResponse>> GetInterviewSummaries();
    Task<InterviewSummaryResponse?> GetInterviewFullDetails(int id);
} 