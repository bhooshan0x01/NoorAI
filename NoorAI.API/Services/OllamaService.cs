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
        if (thinkStartIndex < 0) return cleanedResponse;
        var thinkEndIndex = cleanedResponse.IndexOf("</think>", thinkStartIndex, StringComparison.OrdinalIgnoreCase);
        if (thinkEndIndex >= 0)
        {
            cleanedResponse = cleanedResponse[(thinkEndIndex + 8)..].Trim();
        }

        return cleanedResponse;
    }

    public async Task<string> GenerateInterviewFeedback(string resumeContent, string jobDescription, string transcript)
    {
        var prompt = $"""
                      Based on the following interview transcript, provide honest and critical feedback on the candidate's performance.
                      Be direct and specific about where their responses fell short of expectations or were incorrect.
                      Analyze their actual responses and provide concrete examples from the conversation.

                      Resume:
                      {resumeContent}

                      Job Description:
                      {jobDescription}

                      Interview Transcript:
                      {transcript}

                      Provide a structured feedback that includes:

                      1. Overall Performance Assessment:
                      - Honestly evaluate how well the candidate's responses aligned with the job requirements
                      - Be specific about where their answers were incorrect or insufficient
                      - Note any concerning patterns in their responses
                      - If responses were consistently off-target, clearly state this
                      - If technical knowledge was incorrect, point this out directly

                      2. Critical Analysis of Responses:
                      - For each question, evaluate:
                        * Whether the answer was technically correct
                        * If the response matched the job requirements
                        * If they demonstrated the required knowledge
                        * If they provided relevant examples
                      - Be direct about incorrect or inadequate responses
                      - Point out specific instances where they failed to meet expectations

                      3. Major Concerns:
                      - List specific areas where the candidate's responses were problematic
                      - Highlight instances where they demonstrated lack of required knowledge
                      - Point out responses that showed misunderstanding of key concepts
                      - Note any red flags in their answers

                      4. Areas for Improvement:
                      - Identify specific responses that were incorrect or inadequate
                      - Point out where they failed to demonstrate required skills
                      - Note instances where they didn't provide concrete examples
                      - Highlight where their experience didn't match job requirements

                      5. Job Fit Assessment:
                      - Honestly evaluate if their demonstrated skills match the job requirements
                      - Clearly state any significant gaps between their responses and job needs
                      - If they're not a good fit, explain why based on their actual responses
                      - Be specific about which required skills they failed to demonstrate

                      6. Recommendations:
                      - Provide specific suggestions for addressing each identified issue
                      - Include examples of correct answers they should have given
                      - Suggest concrete ways to improve their knowledge gaps
                      - If they're not a good fit, suggest what they need to learn or improve

                      Important Guidelines:
                      - Be honest and direct about poor performance
                      - Don't sugar-coat feedback when responses were incorrect
                      - Use specific examples from their responses to support your critique
                      - If they consistently failed to meet requirements, state this clearly
                      - Focus on actual responses given, not potential or hypothetical performance

                      Format the feedback in a clear, structured way with specific examples from the transcript.
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
        if (ollamaResponse?.Response == null)
        {
            return "Could not generate feedback at this time.";
        }
        var cleanedResponse = ollamaResponse.Response;
        var thinkStartIndex = cleanedResponse.IndexOf("<think>", StringComparison.OrdinalIgnoreCase);
        if (thinkStartIndex < 0) return cleanedResponse;
        var thinkEndIndex = cleanedResponse.IndexOf("</think>", thinkStartIndex, StringComparison.OrdinalIgnoreCase);
        if (thinkEndIndex >= 0)
        {
            cleanedResponse = cleanedResponse[(thinkEndIndex + 8)..].Trim();
        }

        return cleanedResponse;
    }

    private class OllamaResponse
    {
        [JsonPropertyName("response")]
        public string Response { get; init; } = string.Empty;
    }
} 