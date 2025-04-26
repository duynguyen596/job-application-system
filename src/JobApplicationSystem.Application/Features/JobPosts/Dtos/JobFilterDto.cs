namespace JobApplicationSystem.Application.Features.JobPosts.Dtos;

public class JobFilterDto
{
    public string? Keyword { get; set; }
    public int? CompanyId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}