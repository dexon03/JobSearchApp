namespace JobSearchApp.Data.Models.Common;

public class Company
{
    public int Id { get; init; }
    public required string Name { get; set; } = null!;
    public required string Description { get; set; } = null!;
}