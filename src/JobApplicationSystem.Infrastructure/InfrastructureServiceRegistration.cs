using System;
using JobApplicationSystem.Domain.Interfaces;
using JobApplicationSystem.Infrastructure.Persistence;
using JobApplicationSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JobApplicationSystem.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // --- Database Context ---
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        // services.AddDbContextFactory<AppDbContext>((serviceProvider, options) =>
        // {
        //     var factoryConfiguration = serviceProvider.GetRequiredService<IConfiguration>();
        //     var factoryConnectionString = factoryConfiguration.GetConnectionString("DefaultConnection");
        //
        //     options.UseSqlite(factoryConnectionString);
        //
        // }, ServiceLifetime.Singleton);

        // --- Repositories & Unit of Work ---
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddTransient<ICompanyRepository, CompanyRepository>();
        services.AddTransient<IJobPostRepository, JobPostRepository>();
        services.AddTransient<ICandidateRepository, CandidateRepository>();
        services.AddTransient<IJobApplicationRepository, JobApplicationRepository>();

        return services;
    }
}