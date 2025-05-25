namespace JobSearchApp.Core.Models.Identity;

public class UsersResponse
{
    public UserDto[] Items { get; set; }  = null!;
    public int TotalCount { get; set; }
}