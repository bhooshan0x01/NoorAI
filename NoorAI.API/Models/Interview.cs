using System.ComponentModel.DataAnnotations;

namespace NoorAI.API.Models;

public class Interview
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string UserName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string UserEmail { get; set; } = string.Empty;
    
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

    // Navigation property for related entities if needed
    public List<InterviewQuestion> Questions { get; set; } = new();
}

public class InterviewQuestion
{
    [Key]
    public int Id { get; set; }
    
    public int InterviewId { get; set; }
    
    [Required]
    public string Question { get; set; } = string.Empty;
    
    [Required]
    public string Answer { get; set; } = string.Empty;
    
    public DateTime AskedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? AnsweredAt { get; set; }
    
    public Interview Interview { get; set; } = null!;
}

public enum InterviewStatus
{
    InProgress,
    Completed
}