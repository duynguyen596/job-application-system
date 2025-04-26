using JobApplicationSystem.Application.Features.Companies.Dtos;
using JobApplicationSystem.Application.Features.Companies.Services;
using JobApplicationSystem.Application.Features.JobPosts.Dtos;
using JobApplicationSystem.Application.Features.JobPosts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobApplicationSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _companyService;
    private readonly IJobService _jobService;

    public CompaniesController(ICompanyService companyService, IJobService jobService)
    {
        _companyService = companyService;
        _jobService = jobService;
    }

    // POST /api/companies
    [HttpPost]
    [ProducesResponseType(typeof(CompanyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    // [Authorize(Roles = "Admin")] // Example: Only admins can create companies? Or public?
    public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyDto createCompanyDto)
    {
        var company = await _companyService.CreateCompanyAsync(createCompanyDto);
        // Consider returning CreatedAtAction if a GetById action exists
        return StatusCode(StatusCodes.Status201Created, company);
    }

    // GET /api/companies/{id} - Example
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CompanyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCompany(int id)
    {
        var company = await _companyService.GetCompanyByIdAsync(id);
        if (company == null) return NotFound();
        return Ok(company);
    }

    // GET /api/companies - Example
    [HttpGet]
    [ProducesResponseType(typeof(List<CompanyDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllCompanies()
    {
        var companies = await _companyService.GetAllCompaniesAsync();
        return Ok(companies);
    }

    // POST /api/companies/{id}/jobs
    [HttpPost("{id}/jobs")]
    [Authorize(Roles = "Company, Admin")] // Only Company or Admin can post
    [ProducesResponseType(typeof(JobPostDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateJobPostForCompany(int id, [FromBody] CreateJobPostDto createJobPostDto)
    {
        // TODO: Add Authorization check: Does the logged-in user BELONG to company 'id'?
        // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        // Need logic to link userId to company ownership/membership

        var jobPost = await _jobService.CreateJobPostAsync(id, createJobPostDto);
        // Consider CreatedAtAction pointing to a GetJobPostById endpoint
        return StatusCode(StatusCodes.Status201Created, jobPost);
    }
}