using FluentValidation;
using JobSearchApp.Core.Models.Vacancies;

namespace JobSearchApp.Core.Validation.Vacancies;

public class CompanyCreateValidator : AbstractValidator<CompanyCreateDto>
{
    public CompanyCreateValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Description).NotEmpty();
    }
}