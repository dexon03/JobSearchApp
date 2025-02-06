namespace JobSearchApp.Core.Models.Vacancies;

public record SkillDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
};