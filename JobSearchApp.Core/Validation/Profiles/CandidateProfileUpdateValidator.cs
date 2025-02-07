using FluentValidation;
using JobSearchApp.Core.Models.Profiles;
using JobSearchApp.Data;
using Microsoft.EntityFrameworkCore;

namespace JobSearchApp.Core.Validation.Profiles;

public class CandidateProfileUpdateValidator : AbstractValidator<CandidateProfileUpdateDto>
{
    public CandidateProfileUpdateValidator(AppDbContext db)
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(x => x.Surname).NotEmpty().WithMessage("Surname is required");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Email is required");
        RuleFor(x => x.Skills).CustomAsync(async (skills, context, cancellation) =>
        {
            if (skills is not null && skills.Any())
            {
                var skillIds = skills.Select(s => s.Id).ToList();
                var existingSkillIds = await db.Skills
                    .Where(s => skillIds.Contains(s.Id))
                    .Select(s => s.Id)
                    .ToListAsync(cancellation);

                var missingSkills = skillIds.Except(existingSkillIds).ToList();

                if (missingSkills.Any())
                {
                    context.AddFailure("Skills",
                        $"The following skill IDs do not exist: {string.Join(", ", missingSkills)}");
                }
            }
        });
        RuleFor(x => x.Locations).CustomAsync(async (locations, context, cancellation) =>
        {
            if (locations is not null && locations.Any())
            {
                var locationIds = locations.Select(s => s.Id).ToList();
                var existingLocationIds = await db.Locations
                    .Where(s => locationIds.Contains(s.Id))
                    .Select(s => s.Id)
                    .ToListAsync(cancellation);

                var missingLocations = locationIds.Except(existingLocationIds).ToList();

                if (missingLocations.Any())
                {
                    context.AddFailure("Skills",
                        $"The following location IDs do not exist: {string.Join(", ", missingLocations)}");
                }
            }
        });
    }
}