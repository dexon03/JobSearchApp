using Microsoft.AspNetCore.Identity;

namespace JobSearchApp.Data.Models;

public class AspNetUserRole: IdentityUserRole<int>
{
    public virtual int UserId { get; set; } = default!;

    public virtual int RoleId { get; set; } = default!;
    public virtual User User { get; set; }
    public virtual Role Role { get; set; }
}