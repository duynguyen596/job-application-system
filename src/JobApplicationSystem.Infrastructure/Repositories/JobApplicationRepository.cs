using JobApplicationSystem.Domain.Entities;
using JobApplicationSystem.Domain.Interfaces;
using JobApplicationSystem.Infrastructure.Core;
using JobApplicationSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationSystem.Infrastructure.Repositories;

/// <summary>
/// Repository for JobApplication entity operations.
/// Implements specific querying logic for applications by Job or Candidate.
/// </summary>
public class JobApplicationRepository : BaseRepository<JobApplication, int>, IJobApplicationRepository
{
    // Constructor passes the DbContext up to the base class
    public JobApplicationRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    /// <summary>
    /// Gets all applications for a specific job post, including Candidate details.
    /// </summary>
    public async Task<List<JobApplication>> GetByJobIdAsync(int jobPostId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(ja => ja.JobPostId == jobPostId)
            .Include(ja => ja.Candidate) // Include candidate details
            .AsNoTracking() // Read-only query
            .OrderByDescending(ja => ja.AppliedAt) // Optional ordering
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets all applications submitted by a specific candidate, including Job Post and Company details.
    /// </summary>
    public async Task<List<JobApplication>> GetByCandidateIdAsync(int candidateId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(ja => ja.CandidateId == candidateId)
            .Include(ja => ja.JobPost) // Include Job Post details
                .ThenInclude(jp => jp.Company) // Then include Company details from Job Post
            .AsNoTracking() // Read-only query
            .OrderByDescending(ja => ja.AppliedAt) // Optional ordering
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Checks if a specific candidate has already applied for a specific job.
    /// </summary>
    public async Task<bool> HasCandidateAppliedAsync(int candidateId, int jobPostId, CancellationToken cancellationToken = default)
    {
        // Use AnyAsync for an efficient existence check based on the composite index/key
        return await _dbSet.AnyAsync(ja => ja.CandidateId == candidateId && ja.JobPostId == jobPostId, cancellationToken);
    }
}