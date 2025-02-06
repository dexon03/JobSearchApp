using JobSearchApp.Data.Models.Common;

namespace JobSearchApp.Data.Models.Profiles;

public class ProfileSkills
{
    public int ProfileId { get; set; }
    public int SkillId { get; set; }
    public virtual CandidateProfile Profile { get; set; } = null!;
    public virtual Skill Skill { get; set; } = null!;
}