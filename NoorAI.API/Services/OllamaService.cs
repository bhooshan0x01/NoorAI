using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using NoorAI.API.Services.Interfaces;

namespace NoorAI.API.Services;

public class OllamaService(IConfiguration configuration) : IOllamaService
{
    private readonly HttpClient _httpClient = new();
    private readonly string _modelName = configuration["Ollama:ModelName"] ?? "deepseek-r1:8b";
    private readonly string _baseUrl = configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";

    public async Task<string> GenerateInterviewQuestion(string resumeContent, string jobDescription, string transcript)
    {
        
        var prompt = $"""
                      Based on the following resume and job description, generate a relevant interview question. 
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
                      3. Is specific and requires a detailed response
                      4. Do not include any thinking process or <think> tags in the response
                      5. Only provide the question directly
                      6. MUST be different from any previous questions in the transcript
                      7. Should focus on a different aspect of the job requirements than previous questions
                      8. Should be based on specific requirements from the job description that haven't been covered yet
                      9. Should avoid asking about the same skills or experiences that were already discussed
                      10. Should only give one question at a time
                      11. All questions should starts with a capital letter and end with a period.
                      Question:
                      """;

        var request = new
        {
            model = _modelName,
            prompt = prompt,
            stream = false
        };

        var response = await _httpClient.PostAsync(
            $"{_baseUrl}/api/generate",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
        );
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var ollamaResponse = JsonSerializer.Deserialize<OllamaResponse>(responseContent);

        if (ollamaResponse?.Response == null)
        {
            return "Could not generate a question at this time.";
        }

        // Clean the response by removing the <think> section if present
        var cleanedResponse = ollamaResponse.Response;
        var thinkStartIndex = cleanedResponse.IndexOf("<think>", StringComparison.OrdinalIgnoreCase);
        if (thinkStartIndex >= 0)
        {
            var thinkEndIndex = cleanedResponse.IndexOf("</think>", thinkStartIndex, StringComparison.OrdinalIgnoreCase);
            if (thinkEndIndex >= 0)
            {
                cleanedResponse = cleanedResponse[(thinkEndIndex + 8)..].Trim();
            }
        }

        return cleanedResponse;
    }

    public async Task<string> GenerateInterviewFeedback(string resumeContent, string jobDescription, string transcript)
    {
        var prompt = $"""
                      Based on the following interview transcript, provide comprehensive feedback on the candidate's performance.
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
                              5. Recommendations for future interviews

                              Feedback:
                      """;

        var request = new
        {
            model = _modelName,
            prompt = prompt,
            stream = false
        };

        var response = await _httpClient.PostAsync(
            $"{_baseUrl}/api/generate",
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
        );

        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var ollamaResponse = JsonSerializer.Deserialize<OllamaResponse>(responseContent);

        // Clean the response by removing the <think> section if present
        var cleanedResponse = ollamaResponse.Response;
        var thinkStartIndex = cleanedResponse.IndexOf("<think>", StringComparison.OrdinalIgnoreCase);
        if (thinkStartIndex >= 0)
        {
            var thinkEndIndex = cleanedResponse.IndexOf("</think>", thinkStartIndex, StringComparison.OrdinalIgnoreCase);
            if (thinkEndIndex >= 0)
            {
                cleanedResponse = cleanedResponse[(thinkEndIndex + 8)..].Trim();
            }
        }

        return cleanedResponse;
    }

    private class OllamaResponse
    {
        [JsonPropertyName("response")]
        public string Response { get; set; } = string.Empty;
    }
} 