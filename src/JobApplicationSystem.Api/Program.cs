using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using FluentValidation; 
using FluentValidation.AspNetCore;
using JobApplicationSystem.Api.Middleware;
using JobApplicationSystem.Application;
using JobApplicationSystem.Application.Features.Companies.Dtos;
using JobApplicationSystem.Application.Mappings;
using JobApplicationSystem.Infrastructure; 
using JobApplicationSystem.Infrastructure.Persistence;
using JobApplicationSystem.Infrastructure.Persistence.Seed;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration; // Get configuration

// --- Configure Services ---

// 1. Logging (Default configuration is often sufficient)
builder.Services.AddLogging();

// 2. Database Context Configuration
var connectionString = configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString, optionsBuilder =>
    {
        optionsBuilder.MigrationsAssembly("JobApplicationSystem.Api");
    }) 
);

// 3. Identity Configuration
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        // Configure identity options if needed
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false; // Example: Simplify for testing
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.SignIn.RequireConfirmedAccount = false; // Set true for production with email confirmation
        options.User.RequireUniqueEmail = true; // Enforce unique emails at Identity level
    })
    .AddEntityFrameworkStores<AppDbContext>() // Use the configured AppDbContext
    .AddDefaultTokenProviders(); // For password reset, email confirmation etc.

// 4. JWT Authentication Configuration
// Ensure JwtSettings are in appsettings.json or user secrets
var jwtKey = configuration["JwtSettings:Key"];
var jwtIssuer = configuration["JwtSettings:Issuer"];
var jwtAudience = configuration["JwtSettings:Audience"];

if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
{
    throw new InvalidOperationException("JWT settings (Key, Issuer, Audience) are missing in configuration.");
}

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = builder.Environment.IsProduction(); // Require HTTPS in production
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidAudience = jwtAudience,
            ValidIssuer = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero // Remove default tolerance
        };
    });

// 5. AutoMapper Configuration
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly); // Finds profiles in Application layer

// 6. FluentValidation Configuration
builder.Services.AddFluentValidationAutoValidation() // Enables auto validation in controllers
    .AddValidatorsFromAssemblyContaining<CreateCompanyDtoValidator>(); // Finds validators in Application layer

// 7. Dependency Injection for Repositories, UnitOfWork, and Services
builder.Services.AddInfrastructureServices(configuration);
builder.Services.AddApplicationServices();

// builder.Services.AddScoped<IAuthService, AuthService>(); // Example if you abstracted JWT generation

// 8. Controllers Configuration
builder.Services.AddControllers();

// 9. API Explorer and Swagger Configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Job Application API", Version = "v1" });

    // Configure Swagger to use JWT Authentication
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http, // Use Http for Bearer token
        Scheme = "Bearer", // Case-insensitive "bearer" scheme name
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description =
            "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer" // Must match the Id in AddSecurityDefinition
                }
            },
            new string[] { } // No specific scopes required for Bearer token
        }
    });
});

// 10. Custom Middleware Registration (AddScoped if it has scoped dependencies)
builder.Services.AddScoped<ExceptionHandlingMiddleware>();

// --- Build the Application Host ---
var app = builder.Build();


// --- Apply Migrations and Seed Data ---
try
{
    // Create a scope to resolve scoped services for seeding
    using var scope = app.Services.CreateScope();
    var serviceProvider = scope.ServiceProvider;
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("Applying database migrations (if any)...");
    var dbContext = serviceProvider.GetRequiredService<AppDbContext>();

    // Optional: Automatically apply pending migrations. Use cautiously in production.
    // Consider external migration tools/scripts for production deployments.
    // await dbContext.Database.MigrateAsync();
    // logger.LogInformation("Database migrations applied successfully.");

    // Seed the database using the DataSeeder class
    logger.LogInformation("Attempting to seed database data...");
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var seederLogger = serviceProvider.GetRequiredService<ILogger<DataSeeder>>();

    var seeder = new DataSeeder(dbContext, roleManager, userManager, seederLogger);
    await seeder.SeedAsync(); // Execute the seeding process

    logger.LogInformation("Database seeding completed.");
}
catch (Exception ex)
{
    // Log errors during migration or seeding
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex,
        "An error occurred during database migration or seeding. The application might not start correctly.");
    // Depending on the severity, you might want to prevent the app from starting fully
    // throw; // Uncomment to stop application startup on seeding/migration error
}


// --- Configure the HTTP Request Pipeline ---

// 1. Exception Handling Middleware (place early)
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 2. Development-specific features (Swagger)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Job Application API V1");
        // Optional: Configure Swagger UI settings here
    });
}

// 3. Security Headers, CORS, etc. (Add as needed)
// app.UseCors("AllowSpecificOrigin"); // Example CORS

// 4. HTTPS Redirection
app.UseHttpsRedirection();

// 5. Static Files (if serving any, e.g., from wwwroot)
// app.UseStaticFiles();

// 6. Routing
app.UseRouting(); // Marks the position where routing decisions are made

// 7. Authentication Middleware (IMPORTANT: Before Authorization)
app.UseAuthentication();

// 8. Authorization Middleware
app.UseAuthorization();

// 9. Endpoint Mapping
app.MapControllers(); // Maps attribute-routed controllers

// --- Run the Application ---
app.Run();