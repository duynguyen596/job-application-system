using FluentValidation;

namespace JobApplicationSystem.Application.Features.JobPosts.Dtos;

public class CreateJobPostDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class CreateJobPostDtoValidator : AbstractValidator<CreateJobPostDto>
{
    public CreateJobPostDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).NotEmpty();
    }
}