using JobSearchApp.Data.Enums;
using JobSearchApp.Data.Models.Profiles.Common;
using Pgvector;

namespace JobSearchApp.Data.Models.Profiles;

public class CandidateProfile : Profile<CandidateProfile>
{
    public Experience WorkExperience { get; set; } = Experience.NoExperience;
    public double DesiredSalary { get; set; } = 0;
    public AttendanceMode Attendance { get; set; } = AttendanceMode.Remote;
    public Vector? Embedding { get; set; }
    public virtual ICollection<ProfileSkills> ProfileSkills { get; set; } = [];
    public virtual ICollection<LocationProfile> LocationProfiles { get; set; } = [];
}