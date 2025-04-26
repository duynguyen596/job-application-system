using JobApplicationSystem.Domain.Cores;

namespace JobApplicationSystem.Domain.Entities;

public class JobApplication : IEntity<int>
{
    public int Id { get; set; }
    public int CandidateId { get; set; }
    public Candidate Candidate { get; set; }
    public int JobPostId { get; set; }
    public virtual JobPost? JobPost { get; set; }
    public string ResumeUrl { get; set; }
    public DateTime AppliedAt { get; set; }
    

}