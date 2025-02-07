using FluentValidation;
using JobSearchApp.Core.Models.Vacancies;

namespace JobSearchApp.Core.Validation.Vacancies;

public class CategoryUpdateValidator : AbstractValidator<CategoryUpdateDto>
{
    public CategoryUpdateValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
    }
}