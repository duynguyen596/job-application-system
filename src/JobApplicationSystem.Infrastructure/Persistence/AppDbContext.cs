using JobApplicationSystem.Domain; // For IEntity<TId> if used
using JobApplicationSystem.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion; // For UTC DateTime conversion if needed

namespace JobApplicationSystem.Infrastructure.Persistence;

// Inherit from IdentityDbContext<IdentityUser> to include Identity tables
public class AppDbContext : IdentityDbContext<IdentityUser>
{
    // Define DbSets for your custom domain entities
    public DbSet<Company> Companies { get; set; } = null!; // Initialize with null forgiving operator
    public DbSet<JobPost> JobPosts { get; set; } = null!;
    public DbSet<Candidate> Candidates { get; set; } = null!;
    public DbSet<JobApplication> JobApplications { get; set; } = null!;

    // Constructor to receive DbContextOptions, passed to the base IdentityDbContext
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // IMPORTANT: Call base.OnModelCreating first to configure Identity schemas
        base.OnModelCreating(modelBuilder);

        // --- Company Configuration ---
        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("Companies");

            entity.HasKey(c => c.Id);

            entity.Property(c => c.Id)
                .ValueGeneratedOnAdd();

            entity.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            // Relationship: One Company has Many JobPosts
            entity.HasMany(c => c.JobPosts)
                .WithOne(jp => jp.Company)
                .HasForeignKey(jp => jp.CompanyId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        // --- JobPost Configuration ---
        modelBuilder.Entity<JobPost>(entity =>
        {
            entity.ToTable("JobPosts");

            entity.HasKey(jp => jp.Id);

            entity.Property(jp => jp.Id)
                .ValueGeneratedOnAdd();

            entity.Property(jp => jp.Title)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(jp => jp.Description)
                .IsRequired();

            entity.Property(jp => jp.PostedAt)
                .IsRequired();

            entity.Property(jp => jp.CompanyId).IsRequired();
        });

        // --- Candidate Configuration ---
        modelBuilder.Entity<Candidate>(entity =>
        {
            entity.ToTable("Candidates");
            entity.HasKey(ca => ca.Id);
            entity.Property(ca => ca.Id).ValueGeneratedOnAdd();
            entity.Property(ca => ca.FullName)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(ca => ca.Email)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(ca => ca.IdentityUserId)
                .IsRequired();
            entity.HasIndex(ca => ca.IdentityUserId)
                .IsUnique();
            // Relationship: One Candidate has Many JobApplications
            entity.HasMany(ca => ca.Applications)
                .WithOne(ja => ja.Candidate)
                .HasForeignKey(ja => ja.CandidateId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        // --- JobApplication Configuration ---
        modelBuilder.Entity<JobApplication>(entity =>
        {
            entity.ToTable("JobApplications");

            entity.HasKey(ja => ja.Id);

            entity.Property(ja => ja.Id)
                .ValueGeneratedOnAdd();

            entity.Property(ja => ja.CandidateId).IsRequired();
            entity.Property(ja => ja.JobPostId).IsRequired();

            entity.Property(ja => ja.ResumeUrl)
                .IsRequired();

            entity.Property(ja => ja.AppliedAt)
                .IsRequired();

            entity.HasOne(ja => ja.JobPost)
                .WithMany()
                .HasForeignKey(ja => ja.JobPostId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(ja => new { ja.CandidateId, ja.JobPostId })
                .IsUnique();
        });

        modelBuilder.Entity<IdentityRole>().HasData(
            new IdentityRole { Id = Guid.NewGuid().ToString(), Name = "Candidate", NormalizedName = "CANDIDATE" },
            new IdentityRole { Id = Guid.NewGuid().ToString(), Name = "Company", NormalizedName = "COMPANY" },
            new IdentityRole { Id = Guid.NewGuid().ToString(), Name = "Admin", NormalizedName = "ADMIN" }
        );
    }
}