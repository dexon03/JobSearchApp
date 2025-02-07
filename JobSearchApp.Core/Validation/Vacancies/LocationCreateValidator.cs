using FluentValidation;
using JobSearchApp.Core.Models.Vacancies;

namespace JobSearchApp.Core.Validation.Vacancies;

public class LocationCreateValidator : AbstractValidator<LocationCreateDto>
{
    public LocationCreateValidator()
    {
        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("City is required");
        RuleFor(x => x.Country)
            .NotEmpty()
            .WithMessage("Country is required");
    }
}