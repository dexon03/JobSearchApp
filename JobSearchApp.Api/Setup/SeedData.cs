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
            Email = "admin@admin.com",
            NormalizedEmail = "ADMIN@admin.COM",
            UserName = "admin@admin.com",
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

            userManager.CreateAsync(user).Wait();
            userManager.AddToRoleAsync(user, Role.Admin.ToString());
        }

        context.SaveChangesAsync();
    }
}