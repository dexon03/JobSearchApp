using JobSearchApp.Core;
using JobSearchApp.Data;
using JobSearchApp.Data.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
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
        builder.Services.AddSignalR();
        builder.Services.AddExceptionHandler<ExceptionHandler>();
        builder.Services.AddProblemDetails();

        builder.Host.UseSerilog((context, loggerConfig) =>
            loggerConfig.ReadFrom.Configuration(context.Configuration));

        builder.Services.AddSignalR();
        builder.Services.AddCors(opt =>
            opt.AddDefaultPolicy(c => c.AllowAnyMethod().AllowAnyOrigin().AllowAnyHeader()));

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
        
        app.Services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo("/keys"))
            .SetApplicationName("JobSearchApp"); 


    }
}