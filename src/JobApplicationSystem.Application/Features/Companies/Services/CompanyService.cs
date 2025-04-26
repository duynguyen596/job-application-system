using AutoMapper;
using JobApplicationSystem.Application.Exceptions;
using JobApplicationSystem.Application.Features.Companies.Dtos;
using JobApplicationSystem.Application.Features.Companies.Services;
using JobApplicationSystem.Domain.Entities;
using JobApplicationSystem.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace JobApplicationSystem.Application.Features.Companies.Services;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CompanyService> _logger;

    public CompanyService(ICompanyRepository companyRepository, IUnitOfWork unitOfWork, IMapper mapper,
        ILogger<CompanyService> logger)
    {
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CompanyDto> CreateCompanyAsync(CreateCompanyDto dto)
    {
        var company = _mapper.Map<Company>(dto);
        await _companyRepository.AddAsync(company);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Company {CompanyId} created.", company.Id);
        return _mapper.Map<CompanyDto>(company);
    }

    public async Task<CompanyDto?> GetCompanyByIdAsync(int id)
    {
        var company = await _companyRepository.GetByIdAsync(id);
        return company == null ? null : _mapper.Map<CompanyDto>(company);
    }

    public async Task<List<CompanyDto>> GetAllCompaniesAsync()
    {
        var companies = await _companyRepository.GetAllAsync();
        return _mapper.Map<List<CompanyDto>>(companies);
    }
}