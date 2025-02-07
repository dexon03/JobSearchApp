using JobSearchApp.Api.Setup;
using JobSearchApp.Core.MessageContracts;
using JobSearchApp.Core.Models.Identity;
using JobSearchApp.Data.Models;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Role = JobSearchApp.Data.Enums.Role;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.AddDependencies();
    
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "JobSearchApp.Api v1"));
}

SeedData.Initialize(app);
app.UseHttpsRedirection();

app.MapIdentityApi<User>();
app.MapPost("api/account/register", async (RegisterDto model, UserManager<User> userManager, IPublishEndpoint publishEndpoint) =>
{
    var newUser = new User
    {
        UserName = model.Email,
        Email = model.Email,
        FirstName = model.FirstName,
        LastName = model.LastName,
    };

    var result = await userManager.CreateAsync(newUser, model.Password);
    if (!result.Succeeded) 
        return Results.BadRequest(new { result.Errors });
    
    var user = await userManager.FindByEmailAsync(newUser.Email);
    await userManager.AddToRoleAsync(user!, model.Role.ToString());
    
    await publishEndpoint.Publish<UserCreatedEvent>(new 
    {
        UserId = user.Id,
        user.Email,
        user.FirstName,
        user.LastName,
        user.PhoneNumber,
        Role = model.Role.ToString()
    });
    
    return Results.Ok(new { Message = "Registration successful" });
});

app.UseAuthentication();
app.UseAuthorization();

app.Run();