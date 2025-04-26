using JobApplicationSystem.Application.Features.JobPosts.Dtos;

namespace JobApplicationSystem.Application.Features.JobPosts.Services;

public interface IJobService
{
    Task<JobPostDto> CreateJobPostAsync(int companyId, CreateJobPostDto dto);
    Task<List<JobPostDto>> GetJobsAsync(JobFilterDto filters);
    Task<JobPostDto?> GetJobByIdAsync(int id);
}