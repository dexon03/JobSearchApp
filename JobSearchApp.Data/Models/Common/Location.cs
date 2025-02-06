using JobSearchApp.Data.Models.Profiles;
using JobSearchApp.Data.Models.Vacancies;

namespace JobSearchApp.Data.Models.Common;

public class Location
{
    public int Id { get; set; }
    public string Country { get; set; }
    public string City { get; set; }

    public virtual ICollection<LocationVacancy> LocationVacancy { get; set; } = [];
    public virtual ICollection<LocationProfile> LocationProfiles { get; set; } = [];
}