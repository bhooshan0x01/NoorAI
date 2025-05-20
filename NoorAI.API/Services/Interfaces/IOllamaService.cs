namespace NoorAI.API.Services.Interfaces;

public interface IOllamaService
{
    Task<string> GenerateInterviewQuestion(string resumeContent, string jobDescription, string transcript, bool isFirstQuestion = false);
    Task<string> GenerateInterviewFeedback(string resumeContent, string jobDescription, string transcript);
}
