using JobSearchApp.Data.Models.Chats;
using JobSearchApp.Data.Models.Common;
using JobSearchApp.Data.Models.Profiles;
using JobSearchApp.Data.Models.Vacancies;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JobSearchApp.Data;

public interface IAppDbContext
{
    public DbSet<Vacancy> Vacancies { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Skill> Skills { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Company> Companies { get; set; }
    
    public DbSet<LocationVacancy> LocationVacancy { get; set; }
    public DbSet<VacancySkill> VacancySkill { get; set; }
    public DbSet<VacancyUser> VacancyUsers { get; set; } 

    public DbSet<CandidateProfile> CandidateProfile { get; set; }
    public DbSet<RecruiterProfile> RecruiterProfile { get; set; }
    public DbSet<ProfileSkills> ProfileSkills { get; set; }
    public DbSet<LocationProfile> LocationProfile { get; set; }
    
    public DbSet<Message> Messages { get; set; }
    public DbSet<Chat> Chats { get; set; }
    
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    DbSet<T> Set<T>() where T : class;
}