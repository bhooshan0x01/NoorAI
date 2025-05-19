using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using NoorAI.API.Services.Interfaces;

namespace NoorAI.API.Services;

public class OpenAIService : IOpenAIService
{
    private readonly OpenAIClient _client;
    private readonly string _modelName;

    public OpenAIService(IConfiguration configuration)
    {
        var apiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("OpenAI:ApiKey");
        _client = new OpenAIClient(apiKey);
        _modelName = configuration["OpenAI:ModelName"] ?? "gpt-4";
    }

    public async Task<string> GenerateInterviewQuestion(string resumeContent, string jobDescription, string transcript)
    {
        var prompt = $@"Based on the following resume and job description, generate a relevant interview question. 
        Consider the previous conversation context to avoid repetition and maintain a natural flow.

        Resume:
        {resumeContent}

        Job Description:
        {jobDescription}

        Previous Conversation:
        {transcript}

        Generate a single, focused interview question that:
        1. Is relevant to the candidate's experience and the job requirements
        2. Helps assess their qualifications
        3. Is specific and requires a detailed response";

        var options = new ChatCompletionsOptions
        {
            Messages = { new ChatRequestSystemMessage(prompt) },
            Temperature = 0.7f,
            MaxTokens = 150
        };

        var response = await _client.GetChatCompletionsAsync(options);
        return response.Value.Choices[0].Message.Content;
    }

    public async Task<string> GenerateInterviewFeedback(string resumeContent, string jobDescription, string transcript)
    {
        var prompt = $@"Based on the following interview transcript, provide comprehensive feedback on the candidate's performance.
        Consider their resume and the job requirements in your analysis.

        Resume:
        {resumeContent}

        Job Description:
        {jobDescription}

        Interview Transcript:
        {transcript}

        Provide feedback that includes:
        1. Overall performance assessment
        2. Strengths demonstrated
        3. Areas for improvement
        4. Specific examples from their responses
        5. Recommendations for future interviews";

        var options = new ChatCompletionsOptions
        {
            Messages = { new ChatRequestSystemMessage(prompt) },
            Temperature = 0.7f,
            MaxTokens = 500
        };

        var response = await _client.GetChatCompletionsAsync(options);
        return response.Value.Choices[0].Message.Content;
    }
} 