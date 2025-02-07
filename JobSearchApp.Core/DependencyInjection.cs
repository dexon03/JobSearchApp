using System.Reflection;
using FluentValidation;
using JobSearchApp.Core.Validation;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

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
        });
    }
}