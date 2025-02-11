using JobSearchApp.Api.Endpoints;

namespace JobSearchApp.Api.Setup;

public static class EndpointsExtensions
{
    public static void UseEndpoints(this WebApplication app)
    {
        var endpoints = app.MapGroup("/api");
        
        //TODO: check user controller
        ChatEndpoints.Register(endpoints);
        RoleEndpoints.Register(endpoints);
        CompanyEndpoints.Register(endpoints);
        LocationEndpoints.Register(endpoints);
        ProfileEndpoints.Register(endpoints);
        SkillsEndpoints.Register(endpoints);
        CategoryEndpoints.Register(endpoints);
        StatisticEndpoints.Register(endpoints);
    } 
}