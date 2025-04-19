using JobSearchApp.Data.Models.Vacancies;

namespace JobSearchApp.Data.Models.Common;

public class VacancyUser
{
     public int Id { get; set; }
     public int UserId { get; set; }
     public int VacancyId { get; set; }
     public DateTime CreatedAt { get; set; }
     
     public virtual User User { get; set; } = null!;
     public virtual Vacancy Vacancy { get; set; } = null!;
}