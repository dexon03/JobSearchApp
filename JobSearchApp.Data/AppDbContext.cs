using JobSearchApp.Data.Models;
using JobSearchApp.Data.Models.Common;
using JobSearchApp.Data.Models.Profiles;
using JobSearchApp.Data.Models.Vacancies;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JobSearchApp.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<User, Role, int>(options)
{
    public DbSet<Vacancy> Vacancy { get; set; }
    public DbSet<Location> Location { get; set; }
    public DbSet<Skill> Skill { get; set; }
    public DbSet<Category> Category { get; set; }
    public DbSet<Company> Company { get; set; }
    public DbSet<LocationVacancy> LocationVacancy { get; set; }
    public DbSet<VacancySkill> VacancySkill { get; set; }

    public DbSet<CandidateProfile> CandidateProfile { get; set; }
    public DbSet<RecruiterProfile> RecruiterProfile { get; set; }
    public DbSet<ProfileSkills> ProfileSkills { get; set; }
    public DbSet<LocationProfile> LocationProfile { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        SeedVacanciesModels(builder);
        
        builder.Entity<ProfileSkills>().HasKey(ps => new { ps.SkillId, ps.ProfileId });
        builder.Entity<LocationProfile>().HasKey(lp => new { lp.LocationId, lp.ProfileId });

        builder.Entity<VacancySkill>(x =>
        {
            x.HasKey(vs => new { vs.SkillId, vs.VacancyId });
            x
                .HasOne(vs => vs.Skill)
                .WithMany(b => b.VacancySkill)
                .HasForeignKey(vs => vs.SkillId);
            x
                .HasOne(vs => vs.Vacancy)
                .WithMany(c => c.VacancySkill)
                .HasForeignKey(vs => vs.VacancyId);
        });

        builder.Entity<ProfileSkills>(x =>
        {
            x
                .HasOne(ps => ps.Skill)
                .WithMany(s => s.ProfileSkills)
                .HasForeignKey(ps => ps.SkillId);
            x
                .HasOne(ps => ps.Profile)
                .WithMany(p => p.ProfileSkills)
                .HasForeignKey(ps => ps.ProfileId);
        });

        builder.Entity<Vacancy>(x =>
        {
            x.HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId);

            x.HasOne(x => x.Company)
                .WithMany()
                .HasForeignKey(x => x.CompanyId);

            x.HasOne(x => x.Recruiter)
                .WithMany()
                .HasForeignKey(x => x.Company);
        });

        builder.Entity<LocationProfile>(x =>
        {
            x
                .HasOne(lp => lp.Location)
                .WithMany(l => l.LocationProfiles)
                .HasForeignKey(lp => lp.LocationId);
            x
                .HasOne(lp => lp.Profile)
                .WithMany(p => p.LocationProfiles)
                .HasForeignKey(lp => lp.ProfileId);
        });
    }

    private static void SeedVacanciesModels(ModelBuilder builder)
    {
        var locations = new[]
        {
            new Location { Id = 1, Country = "Ukraine", City = "Kyiv" },
            new Location { Id = 2, Country = "Ukraine", City = "Lviv" },
            new Location { Id = 3, Country = "Ukraine", City = "Kharkiv" },
            new Location { Id = 4, Country = "Ukraine", City = "Dnipro" },
            new Location { Id = 5, Country = "Ukraine", City = "Odesa" },
            new Location { Id = 6, Country = "Ukraine", City = "Zaporizhzhia" },
            new Location { Id = 7, Country = "Ukraine", City = "Vinnytsia" },
            new Location { Id = 8, Country = "Ukraine", City = "Khmelnytskyi" },
        };

        var skills = new[]
        {
            new Skill { Id = 1, Name = "C#" },
            new Skill { Id = 2, Name = "Java" },
            new Skill { Id = 3, Name = "Python" },
            new Skill { Id = 4, Name = "JavaScript" },
            new Skill { Id = 5, Name = "C++" },
            new Skill { Id = 6, Name = "PHP" },
            new Skill { Id = 7, Name = "Ruby" },
            new Skill { Id = 8, Name = "Swift" },
            new Skill { Id = 9, Name = "Go" },
            new Skill { Id = 10, Name = "Kotlin" },
            new Skill { Id = 11, Name = "TypeScript" },
            new Skill { Id = 12, Name = "Scala" },
        };

        var categories = new[]
        {
            new Category
            {
                Id = 1,
                Name = "Software Development",
            },
            new Category
            {
                Id = 2,
                Name = "Design",
            },
            new Category
            {
                Id = 3,
                Name = "Management",
            },
            new Category
            {
                Id = 4,
                Name = "Marketing",
            },
            new Category
            {
                Id = 5,
                Name = "Sales",
            },
            new Category
            {
                Id = 6,
                Name = "Finance",
            }
        };

        builder.Entity<Location>().HasData(locations);
        builder.Entity<Skill>().HasData(skills);
        builder.Entity<Category>().HasData(categories);
    }
};