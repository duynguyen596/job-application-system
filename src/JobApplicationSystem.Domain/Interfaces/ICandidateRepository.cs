using JobApplicationSystem.Domain.Cores;
using JobApplicationSystem.Domain.Entities;

namespace JobApplicationSystem.Domain.Interfaces;

public interface ICandidateRepository : IBaseRepository<Candidate, int>
{
    Task<Candidate?> GetByIdentityUserIdAsync(string identityUserId, CancellationToken cancellationToken = default);
    Task<bool> ExistsForIdentityUserAsync(string identityUserId, CancellationToken cancellationToken = default);
}