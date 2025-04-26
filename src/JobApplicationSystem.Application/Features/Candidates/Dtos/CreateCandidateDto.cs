using FluentValidation;

namespace JobApplicationSystem.Application.Features.Candidates.Dtos;

public class CreateCandidateDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class CreateCandidateDtoValidator : AbstractValidator<CreateCandidateDto>
{
    public CreateCandidateDtoValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(100);
    }
}