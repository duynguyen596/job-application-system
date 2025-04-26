using AutoMapper;
using JobApplicationSystem.Application.Features.Candidates.Dtos;
using JobApplicationSystem.Application.Features.Companies.Dtos;
using JobApplicationSystem.Application.Features.JobApplications.Dtos;
using JobApplicationSystem.Application.Features.JobPosts.Dtos;
using JobApplicationSystem.Domain.Entities;

namespace JobApplicationSystem.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Company
        CreateMap<CreateCompanyDto, Company>();
        CreateMap<Company, CompanyDto>();

        // JobPost
        CreateMap<CreateJobPostDto, JobPost>();
        CreateMap<JobPost, JobPostDto>()
            .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company != null ? src.Company.Name : string.Empty));

        // Candidate
        CreateMap<CreateCandidateDto, Candidate>();
        CreateMap<Candidate, CandidateDto>();

        // JobApplication
        CreateMap<CreateApplicationDto, JobApplication>();
        CreateMap<JobApplication, ApplicationDto>()
            .ForMember(dest => dest.CandidateName, opt => opt.MapFrom(src => src.Candidate != null ? src.Candidate.FullName : string.Empty))
            .ForMember(dest => dest.JobTitle, opt => opt.MapFrom(src => src.JobPost != null ? src.JobPost.Title : string.Empty));
    }
}