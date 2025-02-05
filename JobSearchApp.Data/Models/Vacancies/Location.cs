namespace JobSearchApp.Data.Models.Vacancies;

public class Location
{
    public int Id { get; set; }
    public string Country { get; set; }
    public string City { get; set; }

    public virtual ICollection<LocationVacancy> LocationVacancy { get; set; } = [];
}