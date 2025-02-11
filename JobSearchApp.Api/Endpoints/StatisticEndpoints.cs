using JobSearchApp.Core.Contracts.Vacancies;

namespace JobSearchApp.Api.Endpoints;

public static class StatisticEndpoints
{
    public static void Register(RouteGroupBuilder group)
    {
        var statisticGroup = group.MapGroup("/statistic");

        statisticGroup.MapGet("/", async (string? skillName, IStatisticService statisticService) =>
            {
                var encodedSkillName = System.Net.WebUtility.UrlEncode(skillName);
                return Results.Ok(await statisticService.GetStatisticAsync(encodedSkillName));
            })
            .WithName("GetStatistic")
            .WithOpenApi();

        statisticGroup.MapGet("/mocked",
                async (IStatisticService statisticService) =>
                {
                    return Results.Ok(await statisticService.GetMockedStatisticAsync());
                })
            .WithName("GetMockedStatistic")
            .WithOpenApi();
    }
}