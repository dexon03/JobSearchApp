using System.Reflection;
using FluentValidation;
using JobSearchApp.Core.Contracts.Chats;
using JobSearchApp.Core.Contracts.Common;
using JobSearchApp.Core.Contracts.Profiles;
using JobSearchApp.Core.Contracts.Vacancies;
using JobSearchApp.Core.Services;
using JobSearchApp.Core.Services.Chats;
using JobSearchApp.Core.Services.Profiles;
using JobSearchApp.Core.Services.Vacancies;
using JobSearchApp.Core.Validation;
using MassTransit;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

namespace JobSearchApp.Core;

public static class DependencyInjection
{
    public static void AddCore(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = Assembly.GetExecutingAssembly();
        services.AddAutoMapper(assembly);
        services.AddValidatorsFromAssembly(assembly);
        services.AddFluentValidationAutoValidation(opt =>
        {
            opt.OverrideDefaultResultFactoryWith<FluentValidationAutoValidationCustomResultFactory>();
        });

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.UsingRabbitMq((context, configurator) =>
            {
                configurator.Host(configuration.GetConnectionString("RabbitMq"));

                configurator.ConfigureEndpoints(context);
            });
            x.AddConsumers(assembly);
        });

        services.AddFusionCache()
            .WithDefaultEntryOptions(new FusionCacheEntryOptions { Duration = TimeSpan.FromMinutes(5) })
            .WithSerializer(new FusionCacheSystemTextJsonSerializer())
            .WithDistributedCache(
                new RedisCache(new RedisCacheOptions
                {
                    Configuration = configuration.GetConnectionString("Redis")
                }));

        AddServices(services);
    }

    private static void AddServices(IServiceCollection services)
    {
        // Vacancy
        services.AddScoped<IVacancyService, VacancyService>();
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<ISkillService, SkillService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ILocationService, LocationService>();
        services.AddScoped<IStatisticService, StatisticService>();

        //Profile
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IPdfService, PdfService>();

        //Chat
        services.AddScoped<IChatService, ChatService>();

        //Common
        services.AddScoped<IEmbeddingService, EmbeddingService>();
    }
}