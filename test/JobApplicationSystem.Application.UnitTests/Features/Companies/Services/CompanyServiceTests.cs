using AutoMapper;
using FluentAssertions;
using JobApplicationSystem.Application.Features.Companies.Dtos;
using JobApplicationSystem.Application.Features.Companies.Services;
using JobApplicationSystem.Domain.Entities;
using JobApplicationSystem.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace JobApplicationSystem.Application.UnitTests.Features.Companies.Services;

public class CompanyServiceTests
{
    // --- Mocks ---
    private readonly Mock<ICompanyRepository> _mockCompanyRepo;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<CompanyService>> _mockLogger;

    // --- System Under Test (SUT) ---
    private readonly CompanyService _sut; // The actual service instance we are testing

    public CompanyServiceTests()
    {
        // Initialize mocks
        _mockCompanyRepo = new Mock<ICompanyRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<CompanyService>>();
        
        _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(1);
        
        _sut = new CompanyService(
            _mockCompanyRepo.Object,
            _mockUnitOfWork.Object,
            _mockMapper.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task CreateCompanyAsync_WhenCalledWithValidDto_ShouldAddCompanyAndSaveChanges()
    {
        // Arrange
        var createDto = new CreateCompanyDto { Name = "Test Corp" };
        var companyEntity = new Company { Id = 0, Name = createDto.Name }; 
        var savedCompanyEntity = new Company { Id = 1, Name = createDto.Name }; 
        var companyDto = new CompanyDto { Id = 1, Name = createDto.Name }; 

        // Setup Mapper mock: Map CreateCompanyDto to Company entity
        _mockMapper.Setup(m => m.Map<Company>(createDto)).Returns(companyEntity);
        _mockCompanyRepo.Setup(repo => repo.AddAsync(companyEntity, It.IsAny<CancellationToken>()))
                        .Callback<Company, CancellationToken>((c, ct) => c.Id = 1); 
        _mockMapper.Setup(m => m.Map<CompanyDto>(It.Is<Company>(c => c.Id == 1))).Returns(companyDto);

        // Act
        var result = await _sut.CreateCompanyAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(companyDto);
        
        _mockCompanyRepo.Verify(repo => repo.AddAsync(
            It.Is<Company>(c => c.Name == createDto.Name && c.Id == 1), 
            It.IsAny<CancellationToken>()),
            Times.Once);

        // Verify that UnitOfWork's SaveChangesAsync was called exactly once
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCompanyByIdAsync_WhenCompanyExists_ShouldReturnCompanyDto()
    {
         // Arrange
         int companyId = 1;
         var companyEntity = new Company { Id = companyId, Name = "Test Corp" };
         var companyDto = new CompanyDto { Id = companyId, Name = "Test Corp" };
         
         _mockCompanyRepo.Setup(repo => repo.GetByIdAsync(companyId, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(companyEntity);
          _mockMapper.Setup(m => m.Map<CompanyDto>(companyEntity)).Returns(companyDto);

         // Act
         var result = await _sut.GetCompanyByIdAsync(companyId);

         // Assert
         result.Should().NotBeNull();
         result.Should().BeEquivalentTo(companyDto);
         _mockCompanyRepo.Verify(repo => repo.GetByIdAsync(companyId, It.IsAny<CancellationToken>()), Times.Once);
    }

     [Fact]
    public async Task GetCompanyByIdAsync_WhenCompanyDoesNotExist_ShouldReturnNull()
    {
         // Arrange
         int companyId = 99;
         _mockCompanyRepo.Setup(repo => repo.GetByIdAsync(companyId, It.IsAny<CancellationToken>()))
                        .ReturnsAsync((Company?)null);

         // Act
         var result = await _sut.GetCompanyByIdAsync(companyId);

         // Assert
         result.Should().BeNull();
         _mockCompanyRepo.Verify(repo => repo.GetByIdAsync(companyId, It.IsAny<CancellationToken>()), Times.Once);
          // Mapper should not be called if entity is null
         _mockMapper.Verify(m => m.Map<CompanyDto>(It.IsAny<Company>()), Times.Never);
    }
}