using JobApplicationSystem.Domain.Cores;

namespace JobApplicationSystem.Domain.Entities;

public class JobPost: IEntity<int>
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime PostedAt { get; set; }
    public int CompanyId { get; set; }
    public Company Company { get; set; }
    
    public virtual ICollection<JobApplication> Applications { get; set; }

}