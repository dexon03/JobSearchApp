using FluentValidation;
using JobSearchApp.Core.Models.Profiles;

namespace JobSearchApp.Core.Validation.Vacancies;

public class SkillUpdateValidator : AbstractValidator<SkillUpdateDto>
{
    public SkillUpdateValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required");
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required");
    }
}