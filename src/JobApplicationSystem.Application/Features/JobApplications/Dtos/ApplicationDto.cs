namespace JobApplicationSystem.Application.Features.JobApplications.Dtos;

public class ApplicationDto
{
    public int Id { get; set; }
    public int CandidateId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public int JobPostId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string ResumeUrl { get; set; } = string.Empty;
    public DateTime AppliedAt { get; set; }
}