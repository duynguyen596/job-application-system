using JobApplicationSystem.Application.Features.Companies.Dtos;

namespace JobApplicationSystem.Application.Features.Companies.Services;

public interface ICompanyService
{
    Task<CompanyDto> CreateCompanyAsync(CreateCompanyDto dto);
    Task<CompanyDto?> GetCompanyByIdAsync(int id);
    Task<List<CompanyDto>> GetAllCompaniesAsync();
}