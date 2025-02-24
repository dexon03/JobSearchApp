using Microsoft.AspNetCore.Identity;

namespace JobSearchApp.Data.Models;

public class Role : IdentityRole<int>
{ 
    public virtual ICollection<AspNetUserRole> UserRole { get; set; } = null!;
}