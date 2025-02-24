namespace JobSearchApp.Core.Models.Identity;

public class UsersResponse
{
    public UserDto[] Items { get; set; }
    public int TotalCount { get; set; }
}