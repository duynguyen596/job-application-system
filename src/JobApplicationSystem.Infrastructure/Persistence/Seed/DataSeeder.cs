using JobApplicationSystem.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // Add logging

namespace JobApplicationSystem.Infrastructure.Persistence.Seed;

public class DataSeeder
{
    private readonly AppDbContext _dbContext;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<DataSeeder> _logger;

    // Inject necessary services
    public DataSeeder(
        AppDbContext dbContext,
        RoleManager<IdentityRole> roleManager,
        UserManager<IdentityUser> userManager,
        ILogger<DataSeeder> logger)
    {
        _dbContext = dbContext;
        _roleManager = roleManager;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Executes the database seeding process.
    /// </summary>
    public async Task SeedAsync()
    {
        _logger.LogInformation("Starting database seeding process...");

        await SeedRolesAsync();
        await SeedAdminUserAsync(); // Seed admin user before potentially linking entities
        await SeedCompaniesAsync();
        await SeedJobPostsAsync();
        // Add other seeding methods here

        _logger.LogInformation("Database seeding process completed.");
    }

    private async Task SeedRolesAsync()
    {
        _logger.LogInformation("Seeding Roles...");
        string[] roleNames = { "Candidate", "Company", "Admin" };

        foreach (var roleName in roleNames)
        {
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
                if (result.Succeeded)
                {
                    _logger.LogInformation("Role '{RoleName}' created successfully.", roleName);
                }
                else
                {
                    _logger.LogError("Error creating role '{RoleName}': {Errors}", roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                 _logger.LogInformation("Role '{RoleName}' already exists.", roleName);
            }
        }
    }

    private async Task SeedAdminUserAsync()
    {
        _logger.LogInformation("Seeding Admin User...");
        string adminEmail = "admin@jobapp.local"; // Use a non-public domain for examples
        string adminPass = "AdminPass123!";       // Use a strong password, consider configuration

        var adminUser = await _userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true, // Set to true to bypass email confirmation for seed user
                LockoutEnabled = false // Example: disable lockout for seed admin
            };

            var result = await _userManager.CreateAsync(adminUser, adminPass);
            if (result.Succeeded)
            {
                _logger.LogInformation("Admin user '{AdminEmail}' created successfully.", adminEmail);
                // Assign Admin role
                var roleResult = await _userManager.AddToRoleAsync(adminUser, "Admin");
                if (roleResult.Succeeded)
                {
                     _logger.LogInformation("Assigned 'Admin' role to user '{AdminEmail}'.", adminEmail);
                }
                 else
                {
                    _logger.LogError("Error assigning 'Admin' role to user '{AdminEmail}': {Errors}", adminEmail, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                 _logger.LogError("Error creating admin user '{AdminEmail}': {Errors}", adminEmail, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
         else
        {
            _logger.LogInformation("Admin user '{AdminEmail}' already exists.", adminEmail);
            // Optional: Ensure admin user has Admin role even if they exist
            if (!await _userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                var roleResult = await _userManager.AddToRoleAsync(adminUser, "Admin");
                 if (roleResult.Succeeded) { _logger.LogInformation("Assigned 'Admin' role to existing user '{AdminEmail}'.", adminEmail); }
            }
        }
    }

    private async Task SeedCompaniesAsync()
    {
        _logger.LogInformation("Seeding Companies...");
        if (!await _dbContext.Companies.AnyAsync())
        {
            var companies = new List<Company>
            {
                new() { Name = "Tech Solutions Inc." },
                new() { Name = "Green Energy Co." },
                new() { Name = "Health Innovations Ltd." }
            };
            await _dbContext.Companies.AddRangeAsync(companies);
            await _dbContext.SaveChangesAsync(); // Save changes after adding companies
             _logger.LogInformation("Added {Count} new companies.", companies.Count);
        }
        else
        {
            _logger.LogInformation("Companies already exist. Skipping seeding.");
        }
    }

     private async Task SeedJobPostsAsync()
    {
        _logger.LogInformation("Seeding Job Posts...");
        // Ensure companies exist first
        var company1 = await _dbContext.Companies.FirstOrDefaultAsync(c => c.Name == "Tech Solutions Inc.");
        var company2 = await _dbContext.Companies.FirstOrDefaultAsync(c => c.Name == "Green Energy Co.");

        if (company1 != null && company2 != null && !await _dbContext.JobPosts.AnyAsync())
        {
             var jobPosts = new List<JobPost>
            {
                new()
                {
                    Title = ".NET Backend Developer",
                    Description = "Seeking an experienced .NET developer to build scalable backend services.",
                    PostedAt = new DateTime(2025, 01, 15, 10, 0, 0, DateTimeKind.Utc),
                    CompanyId = company1.Id // Use ID from fetched company
                },
                 new()
                {
                    Title = "Frontend Developer (React)",
                    Description = "Join our team to build modern user interfaces with React.",
                    PostedAt = new DateTime(2025, 01, 20, 11, 30, 0, DateTimeKind.Utc),
                    CompanyId = company1.Id
                },
                 new()
                {
                    Title = "Solar Panel Technician",
                    Description = "Install and maintain solar panel systems.",
                    PostedAt = new DateTime(2025, 02, 01, 9, 0, 0, DateTimeKind.Utc),
                    CompanyId = company2.Id
                }
            };
            await _dbContext.JobPosts.AddRangeAsync(jobPosts);
            await _dbContext.SaveChangesAsync(); // Save changes after adding job posts
             _logger.LogInformation("Added {Count} new job posts.", jobPosts.Count);
        }
        else
        {
            if (company1 == null || company2 == null) {
                 _logger.LogWarning("Could not seed Job Posts because required companies were not found.");
            } else {
                 _logger.LogInformation("Job Posts already exist or required companies not found. Skipping seeding.");
            }
        }
    }
}