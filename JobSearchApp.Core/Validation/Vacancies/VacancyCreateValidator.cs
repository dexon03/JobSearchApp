using FluentValidation;
using JobSearchApp.Core.Models.Vacancies;
using JobSearchApp.Data;

namespace JobSearchApp.Core.Validation.Vacancies;

public class VacancyCreateValidator : AbstractValidator<VacancyCreateDto>
{
    public VacancyCreateValidator(AppDbContext db)
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Description is required");
        RuleFor(x => x.Salary).NotEmpty().GreaterThan(0).WithMessage("Salary is required");
        RuleFor(x => x.CategoryId).NotEmpty().WithMessage("Category is required").Custom((id, context) =>
        {
            if (!db.Categories.Any(c => c.Id == id))
            {
                context.AddFailure("Category not found");
            }
        });
        RuleFor(x => x.CompanyId).NotEmpty().WithMessage("Company is required").Custom((id, context) =>
        {
            if (!db.Categories.Any(c => c.Id == id))
            {
                context.AddFailure("Company not found");
            }
        });
        RuleFor(x => x.RecruiterId).NotEmpty();
    }
}