using JobSearchApp.Data.Models.Common;

namespace JobSearchApp.Data.Models.Profiles;

public class LocationProfile
{
    public int ProfileId { get; set; }
    public int LocationId { get; set; }
    public virtual CandidateProfile Profile { get; set; } = null!;
    public virtual Location Location { get; set; } = null!;
}