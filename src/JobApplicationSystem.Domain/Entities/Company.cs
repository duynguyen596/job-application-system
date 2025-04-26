using JobApplicationSystem.Domain.Cores;

namespace JobApplicationSystem.Domain.Entities;

public class Company : IEntity<int>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<JobPost> JobPosts { get; set; }

}