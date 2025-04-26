using JobApplicationSystem.Domain.Entities;
using JobApplicationSystem.Domain.Interfaces;
using JobApplicationSystem.Infrastructure.Core;
using JobApplicationSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationSystem.Infrastructure.Repositories;

/// <summary>
/// Repository for Candidate entity operations.
/// Includes methods to find candidates by Identity User ID.
/// </summary>
public class CandidateRepository : BaseRepository<Candidate, int>, ICandidateRepository
{
    // Constructor passes the DbContext up to the base class
    public CandidateRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    /// <summary>
    /// Gets a candidate profile using the associated Identity User ID.
    /// </summary>
    public async Task<Candidate?> GetByIdentityUserIdAsync(string identityUserId, CancellationToken cancellationToken = default)
    {
        // Use FirstOrDefaultAsync to find the candidate matching the IdentityUserId
        return await _dbSet
            .AsNoTracking() // Use AsNoTracking if primarily for reading
            .FirstOrDefaultAsync(c => c.IdentityUserId == identityUserId, cancellationToken);
    }
    
    /// <summary>
    /// Checks if a candidate profile exists for a given Identity User ID.
    /// </summary>
    public async Task<bool> ExistsForIdentityUserAsync(string identityUserId, CancellationToken cancellationToken = default)
    {
        // Use AnyAsync for an efficient existence check
        return await _dbSet.AnyAsync(c => c.IdentityUserId == identityUserId, cancellationToken);
    }
}