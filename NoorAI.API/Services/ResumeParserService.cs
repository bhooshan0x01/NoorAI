using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Microsoft.AspNetCore.Http;

namespace NoorAI.API.Services;

public class ResumeParserService
{
    public async Task<(string Name, string Email)> ParseResumeInfo(IFormFile resumeFile)
    {
        if (resumeFile == null || resumeFile.Length == 0)
            throw new ArgumentException("No resume file provided");

        // Read the PDF content
        await using var stream = resumeFile.OpenReadStream();
        using var pdfReader = new PdfReader(stream);
        using var pdfDocument = new PdfDocument(pdfReader);
        
        var text = new System.Text.StringBuilder();
        
        // Extract text from all pages
        for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
        {
            var page = pdfDocument.GetPage(i);
            var strategy = new SimpleTextExtractionStrategy();
            var currentText = PdfTextExtractor.GetTextFromPage(page, strategy);
            text.Append(currentText);
        }

        var resumeText = text.ToString();

        // Extract email using regex
        var emailPattern = @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}";
        var emailMatch = Regex.Match(resumeText, emailPattern);
        var email = emailMatch.Success ? emailMatch.Value : string.Empty;

        // Extract name (assuming it's at the top of the resume)
        var name = ExtractName(resumeText);

        return (name, email);
    }

    private string ExtractName(string resumeText)
    {
        // Common patterns for names in resumes
        var namePatterns = new[]
        {
            // Pattern 1: Full name in all caps at the start
            @"^([A-Z][A-Z\s]+)(?=\n|$)",
            
            // Pattern 2: First name followed by last name
            @"^([A-Z][a-z]+\s+[A-Z][a-z]+)(?=\n|$)",
            
            // Pattern 3: Name followed by common resume headers
            @"^([A-Z][a-z]+\s+[A-Z][a-z]+)(?=\s*(?:Resume|CV|Curriculum Vitae|Professional Summary|Experience))",
            
            // Pattern 4: Name in title case
            @"^([A-Z][a-z]+(?:\s+[A-Z][a-z]+)+)(?=\n|$)"
        };

        foreach (var pattern in namePatterns)
        {
            var match = Regex.Match(resumeText, pattern, RegexOptions.Multiline);
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }
        }

        // If no pattern matches, try to get the first line that looks like a name
        var lines = resumeText.Split('\n');
        foreach (var line in lines.Take(5)) // Check first 5 lines
        {
            var trimmedLine = line.Trim();
            if (Regex.IsMatch(trimmedLine, @"^[A-Z][a-z]+(?:\s+[A-Z][a-z]+)+$"))
            {
                return trimmedLine;
            }
        }

        return string.Empty;
    }
} 