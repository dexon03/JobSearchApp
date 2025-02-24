using Microsoft.AspNetCore.Identity;

namespace JobSearchApp.Data.Models;

public class AspNetUserRole: IdentityUserRole<int>
{
    public virtual Role Role { get; set; }
}