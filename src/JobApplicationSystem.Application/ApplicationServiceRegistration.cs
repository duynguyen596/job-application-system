using System.Reflection;
using FluentValidation;
using JobApplicationSystem.Application.Features.Candidates.Services; 
using JobApplicationSystem.Application.Features.Companies.Services;
using JobApplicationSystem.Application.Features.JobApplications.Services; 
using JobApplicationSystem.Application.Features.JobPosts.Services; 
using Microsoft.Extensions.DependencyInjection;

namespace JobApplicationSystem.Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
    
        // Note: The integration call `.AddFluentValidationAutoValidation()` still need
        // to be in the API project's Program.cs to link validation to the MVC pipeline
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // 2. AutoMapper Registratio
        // Finds all profiles inheriting from AutoMapper.Profile in this assembly
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IJobService, JobService>();
        services.AddScoped<ICandidateService, CandidateService>();
        services.AddScoped<IApplicationService, ApplicationService>();

        return services;
    }
}