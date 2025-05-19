using NoorAI.API.Models;

namespace NoorAI.API.DTOs;

public record StartInterviewRequest(
    string ResumeContent,
    string JobDescription,
    string UserName,
    string UserEmail
);

public record InterviewResponse(
    int Id,
    string Question,
    string? Feedback = null
);

public record InterviewResponseRequest(
    int InterviewId
);

public record EndInterviewRequest(
    int InterviewId
);

public record InterviewFeedbackResponse(
    int InterviewId,
    string Feedback,
    string Transcript,
    string UserName,
    string UserEmail,
    DateTime CreatedAt,
    DateTime? CompletedAt
);

public record UpdateJobDescriptionRequest(
    int InterviewId,
    string JobDescription
);

public record InterviewDetailsResponse(
    int Id,
    string ResumeContent,
    string JobDescription,
    string Transcript,
    string? Feedback,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    InterviewStatus Status
);

public record InterviewSummaryResponse(
    int Id,
    string UserName,
    string UserEmail,
    string JobDescription,
    string Transcript,
    string? Feedback,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    InterviewStatus Status,
    int QuestionCount,
    TimeSpan? Duration
); 