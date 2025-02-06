using JobSearchApp.Data.Models.Common;
using JobSearchApp.Data.Models.Profiles.Common;

namespace JobSearchApp.Data.Models.Profiles;

public class RecruiterProfile : Profile<RecruiterProfile>
{
    public int? CompanyId { get; set; }
    public Company? Company { get; set; }
}