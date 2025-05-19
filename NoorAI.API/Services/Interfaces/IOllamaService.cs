namespace NoorAI.API.Services.Interfaces;

public interface IOllamaService
{
    Task<string> GenerateInterviewQuestion(string resumeContent, string jobDescription, string transcript);
    Task<string> GenerateInterviewFeedback(string resumeContent, string jobDescription, string transcript);
}
