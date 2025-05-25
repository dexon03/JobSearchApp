using JobSearchApp.Data.Enums;

namespace JobSearchApp.Core.Models.Profiles;

public class AiDescriptionRequest
{
    public string? Description { get; set; }
    public Experience Experience { get; set; }
    public string PositionTitle { get; set; }  = null!;
}