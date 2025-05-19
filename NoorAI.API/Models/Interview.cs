using System.ComponentModel.DataAnnotations;

namespace NoorAI.API.Models;

public class Interview
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string ResumeContent { get; set; } = string.Empty;
    
    [Required]
    public string JobDescription { get; set; } = string.Empty;
    
    [Required]
    public string Transcript { get; set; } = string.Empty;
    
    public string? Feedback { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? CompletedAt { get; set; }
    
    public InterviewStatus Status { get; set; } = InterviewStatus.InProgress;
}

public enum InterviewStatus
{
    InProgress,
    Completed
}