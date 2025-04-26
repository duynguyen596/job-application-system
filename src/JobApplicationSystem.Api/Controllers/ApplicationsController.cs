using JobApplicationSystem.Application.Features.JobApplications.Dtos;
using JobApplicationSystem.Application.Features.JobApplications.Services;
using JobApplicationSystem.Application.Features.Candidates.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobApplicationSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")] 
[Authorize]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _applicationService;
    private readonly ICandidateService _candidateService;
    private readonly ILogger<ApplicationsController> _logger;

    public ApplicationsController(IApplicationService applicationService, ICandidateService candidateService,
        ILogger<ApplicationsController> logger)
    {
        _applicationService = applicationService;
        _candidateService = candidateService;
        _logger = logger;
    }

    // POST /api/applications
    [HttpPost]
    [Authorize(Roles = "Candidate")]
    [ProducesResponseType(typeof(ApplicationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] 
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)] 
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SubmitApplication([FromBody] CreateApplicationDto createApplicationDto)
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(identityUserId))
        {
            return Unauthorized("User identity not found in token.");
        }
        
        var applicationDto = await _applicationService.SubmitApplicationAsync(identityUserId, createApplicationDto);
        
        return StatusCode(StatusCodes.Status201Created, applicationDto);
    }

    [HttpGet("my")]
    [Authorize(Roles = "Candidate")] 
    [ProducesResponseType(typeof(List<ApplicationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyApplications()
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(identityUserId))
        {
            return Unauthorized("User identity not found in token.");
        }

        var applications = await _applicationService.GetApplicationsForCandidateAsync(identityUserId);
        return Ok(applications);
    }

 
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApplicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetApplicationById(int id)
    {
        var application = await _applicationService.GetApplicationByIdAsync(id);
        if (application == null)
        {
            return NotFound();
        }

        // --- Authorization Check ---
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        // 1. Is the user an Admin?
        if (userRoles.Contains("Admin"))
        {
            return Ok(application);
        }

        // 2. Is the user the candidate who submitted it?
        if (userRoles.Contains("Candidate"))
        {
            var candidateProfile = await _candidateService.GetCandidateByIdentityUserIdAsync(identityUserId!);
            if (candidateProfile != null && application.CandidateId == candidateProfile.Id)
            {
                return Ok(application);
            }
        }
        _logger.LogWarning("User {UserId} forbidden to access Application {ApplicationId}.", identityUserId, id);
        return Forbid();
    }
}