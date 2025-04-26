using JobApplicationSystem.Domain.Entities;
using JobApplicationSystem.Domain.Interfaces;
using JobApplicationSystem.Infrastructure.Core;
using JobApplicationSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationSystem.Infrastructure.Repositories;

public class CompanyRepository: BaseRepository<Company, int>, ICompanyRepository
{
    public CompanyRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}