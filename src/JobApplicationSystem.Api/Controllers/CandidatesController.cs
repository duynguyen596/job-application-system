using JobApplicationSystem.Application.Features.Candidates.Dtos;
using JobApplicationSystem.Application.Features.Candidates.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobApplicationSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CandidatesController : ControllerBase
{
    private readonly ICandidateService _candidateService;

    public CandidatesController(ICandidateService candidateService)
    {
        _candidateService = candidateService;
    }

    [HttpPost]
    [Authorize] 
    [ProducesResponseType(typeof(CandidateDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] 
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    
    public async Task<IActionResult> CreateCandidateProfile([FromBody] CreateCandidateDto createCandidateDto)
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(identityUserId))
        {  
            return Unauthorized("User identity not found in token.");
        }
        var candidateDto = await _candidateService.CreateCandidateProfileAsync(identityUserId, createCandidateDto);
        
        return StatusCode(StatusCodes.Status201Created, candidateDto);
    }
    
    [HttpGet("me")]
    [Authorize] // User must be authenticated
    [ProducesResponseType(typeof(CandidateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyProfile()
    {
        var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(identityUserId))
        {
            return Unauthorized("User identity not found in token.");
        }

        var candidate = await _candidateService.GetCandidateByIdentityUserIdAsync(identityUserId);
        if (candidate == null)
        {
            return NotFound("Candidate profile not found for the current user.");
        }
        return Ok(candidate);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CandidateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCandidateById(int id)
    {
        var candidate = await _candidateService.GetCandidateByIdAsync(id);
        if (candidate == null)
        {
            return NotFound();
        }
        return Ok(candidate);
    }
}