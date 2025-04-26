using AutoMapper;
using JobApplicationSystem.Application.Exceptions;
using JobApplicationSystem.Application.Features.Candidates.Dtos;
using JobApplicationSystem.Application.Features.Candidates.Services;
using JobApplicationSystem.Domain.Entities;
using JobApplicationSystem.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace JobApplicationSystem.Application.Features.Candidates.Services;

public class CandidateService : ICandidateService
{
    private readonly ICandidateRepository _candidateRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CandidateService> _logger;

    public CandidateService(ICandidateRepository candidateRepository, IUnitOfWork unitOfWork, IMapper mapper,
        ILogger<CandidateService> logger)
    {
        _candidateRepository = candidateRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CandidateDto> CreateCandidateProfileAsync(string identityUserId, CreateCandidateDto dto)
    {
        if (await _candidateRepository.ExistsForIdentityUserAsync(identityUserId))
            throw new ValidationAppException($"Candidate profile already exists for user {identityUserId}.");
        var candidate = _mapper.Map<Candidate>(dto);
        candidate.IdentityUserId = identityUserId;
        await _candidateRepository.AddAsync(candidate);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Candidate profile {CandidateId} created for IdentityUser {IdentityUserId}.",
            candidate.Id, identityUserId);
        return _mapper.Map<CandidateDto>(candidate);
    }

    public async Task<CandidateDto?> GetCandidateByIdentityUserIdAsync(string identityUserId)
    {
        var candidate = await _candidateRepository.GetByIdentityUserIdAsync(identityUserId);
        return candidate == null ? null : _mapper.Map<CandidateDto>(candidate);
    }

    public async Task<CandidateDto?> GetCandidateByIdAsync(int id)
    {
        var candidate = await _candidateRepository.GetByIdAsync(id);
        return candidate == null ? null : _mapper.Map<CandidateDto>(candidate);
    }
}