using JobApplicationSystem.Domain.Cores;

namespace JobApplicationSystem.Domain.Entities;

public class Candidate : IEntity<int>
{
    public int Id { get; set; }
    public string IdentityUserId { get; set; } = string.Empty;
    public string FullName { get; set; }
    public string Email { get; set; }
    public ICollection<JobApplication> Applications { get; set; }
}