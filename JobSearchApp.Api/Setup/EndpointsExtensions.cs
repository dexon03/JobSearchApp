namespace JobSearchApp.Api.Setup;

public static class EndpointsExtensions
{
    public static void UseEndpoints(this WebApplication app)
    {
        var endpoints = app.MapGroup("/api");
        
    } 
}