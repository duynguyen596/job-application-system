using FluentValidation;

namespace JobApplicationSystem.Application.Features.JobApplications.Dtos;

public class CreateApplicationDto
{
    public int JobPostId { get; set; }
    public string ResumeUrl { get; set; } = string.Empty;
}

public class CreateApplicationDtoValidator : AbstractValidator<CreateApplicationDto>
{
    public CreateApplicationDtoValidator()
    {
        RuleFor(x => x.JobPostId).GreaterThan(0);
        RuleFor(x => x.ResumeUrl).NotEmpty().Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("'{PropertyName}' must be a valid URL.");
    }
}