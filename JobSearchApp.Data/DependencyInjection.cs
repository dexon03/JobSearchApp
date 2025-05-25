using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace JobSearchApp.Data;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(opt =>
            opt.UseNpgsql(configuration.GetConnectionString("PostgresConnection"), o => o.UseVector())
                .EnableSensitiveDataLogging()
                .LogTo(Log.Logger.Information, LogLevel.Information));
        services.AddScoped<IAppDbContext, AppDbContext>();
    }
}