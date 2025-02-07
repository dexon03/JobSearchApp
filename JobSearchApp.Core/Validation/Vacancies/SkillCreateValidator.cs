using FluentValidation;
using JobSearchApp.Core.Models.Vacancies;

namespace JobSearchApp.Core.Validation.Vacancies;

public class SkillCreateValidator : AbstractValidator<SkillCreateDto>
{
    public SkillCreateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required");
    }
    
}