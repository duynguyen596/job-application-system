using JobApplicationSystem.Domain.Entities;
using JobApplicationSystem.Domain.Interfaces;
using JobApplicationSystem.Infrastructure.Core;
using JobApplicationSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationSystem.Infrastructure.Repositories;

/// <summary>
/// Repository for JobPost entity operations.
/// Implements specific filtering logic in addition to base CRUD.
/// </summary>
public class JobPostRepository : BaseRepository<JobPost, int>, IJobPostRepository
{
    // Constructor passes the DbContext up to the base class
    public JobPostRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    /// <summary>
    /// Gets a filtered list of job posts, including their associated company.
    /// </summary>
    public async Task<List<JobPost>> GetFilteredJobsAsync(
        string? keyword,
        int? companyId,
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Include(jp => jp.Company)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            string lowerKeyword = keyword.ToLower();
            query = query.Where(jp => jp.Title.ToLower().Contains(lowerKeyword) ||
                                      jp.Description.ToLower().Contains(lowerKeyword));
        }

        if (companyId.HasValue)
        {
            query = query.Where(jp => jp.CompanyId == companyId.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(jp => jp.PostedAt >= startDate);
        }

        if (endDate.HasValue)
        {
            query = query.Where(jp => jp.PostedAt < endDate);
        }

        query = query.OrderByDescending(jp => jp.PostedAt);
        return await query.ToListAsync(cancellationToken);
    }
}