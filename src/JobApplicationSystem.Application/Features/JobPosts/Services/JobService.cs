using AutoMapper;
using JobApplicationSystem.Application.Exceptions;
using JobApplicationSystem.Application.Features.JobPosts.Dtos;
using JobApplicationSystem.Application.Features.JobPosts.Services;
using JobApplicationSystem.Domain.Entities;
using JobApplicationSystem.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace JobApplicationSystem.Application.Features.JobPosts.Services;

public class JobService : IJobService
{
    private readonly IJobPostRepository _jobPostRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<JobService> _logger;

    public JobService(IJobPostRepository jobPostRepository, ICompanyRepository companyRepository,
        IUnitOfWork unitOfWork, IMapper mapper, ILogger<JobService> logger)
    {
        _jobPostRepository = jobPostRepository;
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<JobPostDto> CreateJobPostAsync(int companyId, CreateJobPostDto dto)
    {
        if (!await _companyRepository.ExistsAsync(companyId)) throw new NotFoundException(nameof(Company), companyId);
        var jobPost = _mapper.Map<JobPost>(dto);
        jobPost.CompanyId = companyId;
        jobPost.PostedAt = DateTime.UtcNow;
        await _jobPostRepository.AddAsync(jobPost);
        await _unitOfWork.SaveChangesAsync();
        var createdDto = _mapper.Map<JobPostDto>(jobPost);
        // Manually map CompanyName if needed, as the initial entity might not have it loaded
        var company = await _companyRepository.GetByIdAsync(companyId);
        createdDto.CompanyName = company?.Name ?? string.Empty;
        _logger.LogInformation("JobPost {JobPostId} created for Company {CompanyId}.", jobPost.Id, companyId);
        return createdDto;
    }

    public async Task<List<JobPostDto>> GetJobsAsync(JobFilterDto filters)
    {
        var jobs = await _jobPostRepository.GetFilteredJobsAsync(filters.Keyword, filters.CompanyId, filters.StartDate,
            filters.EndDate);
        return _mapper.Map<List<JobPostDto>>(jobs);
    }

    public async Task<JobPostDto?> GetJobByIdAsync(int id)
    {
        var job = await _jobPostRepository.GetByIdAsync(id);
        return job == null ? null : _mapper.Map<JobPostDto>(job);
    }
}