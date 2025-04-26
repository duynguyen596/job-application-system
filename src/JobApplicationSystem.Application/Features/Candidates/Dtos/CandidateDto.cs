namespace JobApplicationSystem.Application.Features.Candidates.Dtos;

public class CandidateDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string IdentityUserId { get; set; } = string.Empty;
}