using FluentValidation;
using JobSearchApp.Core.Models.Identity;

namespace JobSearchApp.Core.Validation.Identity;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty();
        RuleFor(x => x.LastName)
            .NotEmpty();
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .MaximumLength(15);
    }
}