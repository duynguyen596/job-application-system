using JobApplicationSystem.Application.Features.Candidates.Dtos;

namespace JobApplicationSystem.Application.Features.Candidates.Services;

public interface ICandidateService
{
    Task<CandidateDto> CreateCandidateProfileAsync(string identityUserId, CreateCandidateDto dto);
    Task<CandidateDto?> GetCandidateByIdentityUserIdAsync(string identityUserId);
    Task<CandidateDto?> GetCandidateByIdAsync(int id);
}