namespace JobSearchApp.Core.Models.Vacancies;

public class VacancyGetAllDto
{
    public int Id { get; set; }
    public string Title { get; set; }= null!;
    public string Description { get; set; }= null!;
    public double Salary { get; set; }
    public string Experience { get; set; }= null!;
    public string AttendanceMode { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string CompanyName { get; set; } = null!;
    public bool IsActive { get; set; }
    public IEnumerable<LocationDto> Locations { get; set; } = [];
}