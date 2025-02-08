using JobSearchApp.Core;
using JobSearchApp.Data;
using JobSearchApp.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Role = JobSearchApp.Data.Enums.Role;

namespace JobSearchApp.Api.Setup;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddDependencies(this WebApplicationBuilder builder)
    {
        AddIdentity(builder);
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddCore(builder.Configuration);
        
        builder.Host.UseSerilog((context, loggerConfig) => 
            loggerConfig.ReadFrom.Configuration(context.Configuration));

        builder.Services.AddDbContext<AppDbContext>(opt =>
            opt.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));
        
        builder.Services.AddSignalR();
        
        return builder;
    }

    private static void AddIdentity(IHostApplicationBuilder app)
    {
        app.Services.AddAuthorization();
        app.Services.AddAuthentication()
            .AddBearerToken(IdentityConstants.BearerScheme);
        
        app.Services.AddAuthorizationBuilder()
            .AddPolicy(Role.Admin.ToString(), builder => 
                builder.RequireRole(Role.Admin.ToString()))
            .AddPolicy(Role.Recruiter.ToString(), builder => 
                builder.RequireRole(Role.Recruiter.ToString()))
            .AddPolicy(Role.Candidate.ToString(), builder => 
                builder.RequireRole(Role.Candidate.ToString()));

        app.Services.AddIdentityCore<User>()
            .AddRoles<Data.Models.Role>()
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