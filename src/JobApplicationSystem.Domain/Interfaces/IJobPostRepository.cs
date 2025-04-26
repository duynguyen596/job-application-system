using JobApplicationSystem.Domain.Cores;
using JobApplicationSystem.Domain.Entities;

namespace JobApplicationSystem.Domain.Interfaces;

public interface IJobPostRepository : IBaseRepository<JobPost, int>
{
    Task<List<JobPost>> GetFilteredJobsAsync(string? keyword, int? companyId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
}