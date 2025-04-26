// tests/JobApplicationSystem.Application.UnitTests/Features/JobApplications/Services/ApplicationServiceTests.cs
using AutoMapper;
using FluentAssertions;
using JobApplicationSystem.Application.Exceptions;
using JobApplicationSystem.Application.Features.JobApplications.Dtos;
using JobApplicationSystem.Application.Features.JobApplications.Services;
using JobApplicationSystem.Domain.Entities;
using JobApplicationSystem.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace JobApplicationSystem.Application.UnitTests.Features.JobApplications.Services;

public class ApplicationServiceTests
{
    private readonly Mock<IJobApplicationRepository> _mockAppRepo;
    private readonly Mock<ICandidateRepository> _mockCandidateRepo;
    private readonly Mock<IJobPostRepository> _mockJobPostRepo;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<ApplicationService>> _mockLogger;
    private readonly ApplicationService _sut;

    public ApplicationServiceTests()
    {
        _mockAppRepo = new Mock<IJobApplicationRepository>();
        _mockCandidateRepo = new Mock<ICandidateRepository>();
        _mockJobPostRepo = new Mock<IJobPostRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ApplicationService>>();

        _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(1);

        _sut = new ApplicationService(
            _mockAppRepo.Object,
            _mockCandidateRepo.Object,
            _mockJobPostRepo.Object,
            _mockUnitOfWork.Object,
            _mockMapper.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task SubmitApplicationAsync_WhenValid_ShouldAddApplicationAndSaveChanges()
    {
        // Arrange
        string identityUserId = "user123";
        int candidateId = 1;
        int jobPostId = 10;
        var createDto = new CreateApplicationDto { JobPostId = jobPostId, ResumeUrl = "http://resume.com/my.pdf" };
        var candidateEntity = new Candidate { Id = candidateId, IdentityUserId = identityUserId, FullName = "Test User" };
        var applicationEntity = new JobApplication { CandidateId = candidateId, JobPostId = jobPostId, ResumeUrl = createDto.ResumeUrl };
        var savedApplicationEntity = new JobApplication { Id = 5, CandidateId = candidateId, JobPostId = jobPostId, ResumeUrl = createDto.ResumeUrl, AppliedAt = DateTime.UtcNow, Candidate = candidateEntity }; // Simulate saved state
        var applicationDto = new ApplicationDto { Id = 5, CandidateId = candidateId, JobPostId = jobPostId, ResumeUrl = createDto.ResumeUrl, AppliedAt = savedApplicationEntity.AppliedAt, CandidateName = candidateEntity.FullName };

        // Mock finding the candidate profile
        _mockCandidateRepo.Setup(repo => repo.GetByIdentityUserIdAsync(identityUserId, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(candidateEntity);
        // Mock checking if job post exists
        _mockJobPostRepo.Setup(repo => repo.ExistsAsync(jobPostId, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(true);
        // Mock checking for duplicate application
        _mockAppRepo.Setup(repo => repo.HasCandidateAppliedAsync(candidateId, jobPostId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false); // No duplicate
        // Mock mapping Create DTO -> Entity
        _mockMapper.Setup(m => m.Map<JobApplication>(createDto)).Returns(applicationEntity);
        // Mock saving the application (optional: simulate ID generation)
        _mockAppRepo.Setup(repo => repo.AddAsync(applicationEntity, It.IsAny<CancellationToken>()))
                    .Callback<JobApplication, CancellationToken>((app, ct) => app.Id = 5); 
        // Mock fetching the saved application for final mapping (if needed by service implementation)
        _mockAppRepo.Setup(repo => repo.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(savedApplicationEntity); 
        // Mock final mapping Entity -> DTO
        _mockMapper.Setup(m => m.Map<ApplicationDto>(It.Is<JobApplication>(a => a.Id == 5))).Returns(applicationDto);

        // Act
        var result = await _sut.SubmitApplicationAsync(identityUserId, createDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(applicationDto);

        _mockCandidateRepo.Verify(repo => repo.GetByIdentityUserIdAsync(identityUserId, It.IsAny<CancellationToken>()), Times.Once);
        _mockJobPostRepo.Verify(repo => repo.ExistsAsync(jobPostId, It.IsAny<CancellationToken>()), Times.Once);
        _mockAppRepo.Verify(repo => repo.HasCandidateAppliedAsync(candidateId, jobPostId, It.IsAny<CancellationToken>()), Times.Once);
        _mockAppRepo.Verify(repo => repo.AddAsync(It.Is<JobApplication>(a => a.CandidateId == candidateId && a.JobPostId == jobPostId), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubmitApplicationAsync_WhenCandidateProfileNotFound_ShouldThrowNotFoundException()
    {
         // Arrange
         string identityUserId = "user_does_not_exist";
         var createDto = new CreateApplicationDto { JobPostId = 1, ResumeUrl = "url" };
         _mockCandidateRepo.Setup(repo => repo.GetByIdentityUserIdAsync(identityUserId, It.IsAny<CancellationToken>()))
                         .ReturnsAsync((Candidate?)null);

         // Act
         Func<Task> act = async () => await _sut.SubmitApplicationAsync(identityUserId, createDto);

         // Assert
         await act.Should().ThrowAsync<NotFoundException>()
             .WithMessage($"No candidate profile found for user {identityUserId}."); 
         _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never); 
    }

    [Fact]
    public async Task SubmitApplicationAsync_WhenJobPostNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        string identityUserId = "user123";
        int candidateId = 1;
        int jobPostId = 999; // Non-existent job post
        var createDto = new CreateApplicationDto { JobPostId = jobPostId, ResumeUrl = "url" };
        var candidateEntity = new Candidate { Id = candidateId, IdentityUserId = identityUserId };

        _mockCandidateRepo.Setup(repo => repo.GetByIdentityUserIdAsync(identityUserId, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(candidateEntity);
        _mockJobPostRepo.Setup(repo => repo.ExistsAsync(jobPostId, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(false); 

        // Act
        Func<Task> act = async () => await _sut.SubmitApplicationAsync(identityUserId, createDto);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
                 .WithMessage($"Entity \"JobPost\" ({jobPostId}) was not found.");
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SubmitApplicationAsync_WhenApplicationIsDuplicate_ShouldThrowDuplicateApplicationException()
    {
        // Arrange
         string identityUserId = "user123";
         int candidateId = 1;
         int jobPostId = 10;
         var createDto = new CreateApplicationDto { JobPostId = jobPostId, ResumeUrl = "url" };
         var candidateEntity = new Candidate { Id = candidateId, IdentityUserId = identityUserId };

         _mockCandidateRepo.Setup(repo => repo.GetByIdentityUserIdAsync(identityUserId, It.IsAny<CancellationToken>())).ReturnsAsync(candidateEntity);
         _mockJobPostRepo.Setup(repo => repo.ExistsAsync(jobPostId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
         // Simulate duplicate found
         _mockAppRepo.Setup(repo => repo.HasCandidateAppliedAsync(candidateId, jobPostId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _sut.SubmitApplicationAsync(identityUserId, createDto);

        // Assert
        await act.Should().ThrowAsync<DuplicateApplicationException>();
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    
}