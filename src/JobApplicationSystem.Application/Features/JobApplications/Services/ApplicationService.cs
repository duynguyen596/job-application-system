using AutoMapper;
using JobApplicationSystem.Application.Exceptions;
using JobApplicationSystem.Application.Features.JobApplications.Dtos;
using JobApplicationSystem.Application.Features.JobApplications.Services;
using JobApplicationSystem.Domain.Entities;
using JobApplicationSystem.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace JobApplicationSystem.Application.Features.JobApplications.Services;

public class ApplicationService : IApplicationService
{
    private readonly IJobApplicationRepository _appRepository;
    private readonly ICandidateRepository _candidateRepository;
    private readonly IJobPostRepository _jobPostRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ApplicationService> _logger;

    public ApplicationService(IJobApplicationRepository appRepository, ICandidateRepository candidateRepository,
        IJobPostRepository jobPostRepository, IUnitOfWork unitOfWork, IMapper mapper,
        ILogger<ApplicationService> logger)
    {
        _appRepository = appRepository;
        _candidateRepository = candidateRepository;
        _jobPostRepository = jobPostRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApplicationDto> SubmitApplicationAsync(string identityUserId, CreateApplicationDto dto)
    {
        var candidate = await _candidateRepository.GetByIdentityUserIdAsync(identityUserId);
        if (candidate == null) throw new NotFoundException($"No candidate profile found for user {identityUserId}.");
        if (!await _jobPostRepository.ExistsAsync(dto.JobPostId))
            throw new NotFoundException(nameof(JobPost), dto.JobPostId);
        if (await _appRepository.HasCandidateAppliedAsync(candidate.Id, dto.JobPostId))
            throw new DuplicateApplicationException(
                $"Candidate {candidate.Id} has already applied for Job Post {dto.JobPostId}.");

        var application = _mapper.Map<JobApplication>(dto);
        application.CandidateId = candidate.Id;
        application.AppliedAt = DateTime.UtcNow;
        await _appRepository.AddAsync(application);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation(
            "Application {ApplicationId} submitted by Candidate {CandidateId} for JobPost {JobPostId}.", application.Id,
            candidate.Id, dto.JobPostId);
        
        var createdApp = await _appRepository.GetByIdAsync(application.Id);
        if (createdApp != null)
        {
            createdApp.Candidate = candidate;
            createdApp.JobPost = await _jobPostRepository.GetByIdAsync(dto.JobPostId);
        }

        return _mapper.Map<ApplicationDto>(createdApp);
    }

    public async Task<List<ApplicationDto>> GetApplicationsForJobAsync(int jobId)
    {
        var applications = await _appRepository.GetByJobIdAsync(jobId);
        return _mapper.Map<List<ApplicationDto>>(applications);
    }

    public async Task<List<ApplicationDto>> GetApplicationsForCandidateAsync(string identityUserId)
    {
        var candidate = await _candidateRepository.GetByIdentityUserIdAsync(identityUserId);
        if (candidate == null) return new List<ApplicationDto>();
        var applications = await _appRepository.GetByCandidateIdAsync(candidate.Id);
        return _mapper.Map<List<ApplicationDto>>(applications);
    }

    public async Task<ApplicationDto?> GetApplicationByIdAsync(int id)
    {
        var application = await _appRepository.GetByIdAsync(id); // TODO: Decide required Includes needed here
        if (application != null)
        {
            application.Candidate = await _candidateRepository.GetByIdAsync(application.CandidateId);
            application.JobPost = await _jobPostRepository.GetByIdAsync(application.JobPostId);
        }

        return application == null ? null : _mapper.Map<ApplicationDto>(application);
    }
}