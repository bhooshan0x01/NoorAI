namespace NoorAI.API.Services.Interfaces;
 
public interface IOpenAIService
{
    Task<string> GenerateInterviewQuestion(string resumeContent, string jobDescription, string transcript);
    Task<string> GenerateInterviewFeedback(string resumeContent, string jobDescription, string transcript);
} 