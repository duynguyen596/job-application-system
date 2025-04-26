using FluentValidation;

namespace JobApplicationSystem.Application.Features.Companies.Dtos;

public class CreateCompanyDto
{
    public string Name { get; set; } = string.Empty;
}

public class CreateCompanyDtoValidator : AbstractValidator<CreateCompanyDto>
{
    public CreateCompanyDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}