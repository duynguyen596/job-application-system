using JobApplicationSystem.Application.Features.JobPosts.Dtos;
using JobApplicationSystem.Application.Features.JobPosts.Services;
using JobApplicationSystem.Application.Features.JobApplications.Dtos;
using JobApplicationSystem.Application.Features.JobApplications.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobApplicationSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")] 
public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;
    private readonly IApplicationService _applicationService; 

    public JobsController(IJobService jobService, IApplicationService applicationService)
    {
        _jobService = jobService;
        _applicationService = applicationService;
    }
    
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<JobPostDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetJobs([FromQuery] JobFilterDto filters)
    {
        var jobs = await _jobService.GetJobsAsync(filters);
        return Ok(jobs);
    }

    
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(JobPostDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetJobById(int id)
    {
        var job = await _jobService.GetJobByIdAsync(id);
        if (job == null)
        {
            return NotFound();
        }

        return Ok(job);
    }

    // GET /api/jobs/{id}/applications
    [HttpGet("{id}/applications")]
    [Authorize(Roles = "Company, Admin")]
    [ProducesResponseType(typeof(List<ApplicationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)] // If job post itself not found
    public async Task<IActionResult> GetJobApplications(int id)
    {
        var applications = await _applicationService.GetApplicationsForJobAsync(id);
        return Ok(applications);
    }
}