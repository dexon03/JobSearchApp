namespace JobSearchApp.Data.Models.Common;

public class Company
{
    public int Id { get; set; }
    public required string Name { get; set; } = null!;
    public required string Description { get; set; } = null!;
}