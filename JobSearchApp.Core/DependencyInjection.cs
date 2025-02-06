using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace JobSearchApp.Core;

public static class DependencyInjection
{
    public static void AddCore(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        services.AddAutoMapper(assembly);
    }
}