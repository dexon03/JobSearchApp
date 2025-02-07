using JobSearchApp.Data;
using JobSearchApp.Data.Models;
using Microsoft.AspNetCore.Identity;
using Role = JobSearchApp.Data.Enums.Role;

namespace JobSearchApp.Api.Setup;

public class SeedData
{
    public static void Initialize(WebApplication app)
    {
        using var serviceScope = app.Services.CreateScope();
        
        var services = serviceScope.ServiceProvider;
        var context = services.GetService<AppDbContext>();
        var userManager = services.GetService<UserManager<User>>();
        var user = new User
        {
            FirstName = "Admin",
            LastName = "Admin",
            Email = "admin@example.com",
            NormalizedEmail = "ADMIN@EXAMPLE.COM",
            UserName = "admin@example.com",
            NormalizedUserName = "ADMIN",
            PhoneNumber = "+111111111111",
            EmailConfirmed = true,
            PhoneNumberConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString("D")
        };


        if (!context.Users.Any(u => u.UserName == user.UserName))
        {
            var password = new PasswordHasher<User>();
            var hashed = password.HashPassword(user, "admin");
            user.PasswordHash = hashed;

            var result = userManager.CreateAsync(user);
            userManager.AddToRoleAsync(user, Role.Admin.ToString());
        }

        context.SaveChangesAsync();
    }
}