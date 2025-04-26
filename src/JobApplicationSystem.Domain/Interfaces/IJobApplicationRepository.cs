using JobApplicationSystem.Domain.Cores;
using JobApplicationSystem.Domain.Entities;

namespace JobApplicationSystem.Domain.Interfaces;

public interface IJobApplicationRepository : IBaseRepository<JobApplication, int>
{
    Task<List<JobApplication>> GetByJobIdAsync(int jobPostId, CancellationToken cancellationToken = default);
    Task<List<JobApplication>> GetByCandidateIdAsync(int candidateId, CancellationToken cancellationToken = default);
    Task<bool> HasCandidateAppliedAsync(int candidateId, int jobPostId, CancellationToken cancellationToken = default);
}