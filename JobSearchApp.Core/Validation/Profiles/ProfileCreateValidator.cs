using FluentValidation;
using JobSearchApp.Core.Models.Profiles;
using JobSearchApp.Data;

namespace JobSearchApp.Core.Validation.Profiles;

public class ProfileCreateValidator : AbstractValidator<ProfileCreateDto>
{
    public ProfileCreateValidator(IAppDbContext db)
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(x => x.Surname).NotEmpty().WithMessage("Surname is required");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Email is required");
        RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("PhoneNumber is required");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Linked user is required");
    }
}