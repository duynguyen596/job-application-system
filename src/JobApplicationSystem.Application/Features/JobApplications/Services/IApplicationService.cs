using JobApplicationSystem.Application.Features.JobApplications.Dtos;

namespace JobApplicationSystem.Application.Features.JobApplications.Services;

public interface IApplicationService
{
    Task<ApplicationDto> SubmitApplicationAsync(string identityUserId, CreateApplicationDto dto);
    Task<List<ApplicationDto>> GetApplicationsForJobAsync(int jobId);
    Task<List<ApplicationDto>> GetApplicationsForCandidateAsync(string identityUserId);
    Task<ApplicationDto?> GetApplicationByIdAsync(int id);
}