using System.Text;
using JobSearchApp.Data;
using JobSearchApp.Data.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace JobSearchApp.Api.Setup;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddDependencies(this IHostApplicationBuilder app)
    {
        AddIdentity(app);

        app.Services.AddDbContext<AppDbContext>(opt =>
            opt.UseNpgsql(app.Configuration.GetConnectionString("PostgresConnection")));
        
        return app;
    }

    private static void AddIdentity(IHostApplicationBuilder app)
    {
        app.Services.AddAuthorization();
        app.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddBearerToken(IdentityConstants.BearerScheme);
        app.Services.AddAuthorizationBuilder();

        app.Services.AddIdentityCore<User>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddApiEndpoints();

        app.Services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequiredLength = 6;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
        });
    }
}