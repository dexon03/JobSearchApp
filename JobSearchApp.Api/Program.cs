using JobSearchApp.Api.Setup;
using JobSearchApp.Data.Models;
using Microsoft.AspNetCore.Identity;
using RegisterRequest = JobSearchApp.Api.RegisterRequest;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.AddDependencies();
    
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "JobSearchApp.Api v1"));
}


app.UseHttpsRedirection();

app.MapIdentityApi<User>();
app.MapPost("api/account/register", async (RegisterRequest model,HttpContext ctx, UserManager<User> userManager) =>
{
    var newUser = new User
    {
        UserName = model.Email,
        Email = model.Email,
        FirstName = model.FirstName,
        LastName = model.LastName,
        // Set other properties
    };

    var result = await userManager.CreateAsync(newUser, model.Password);
    var user = await userManager.FindByEmailAsync(model.Email);
    if (result.Succeeded)
    {
        // Your registration success logic
        return Results.Ok(new { Message = "Registration successful" });
    }

    // If registration fails, return errors
    return Results.BadRequest(new { Errors = result.Errors });
});
app.UseAuthentication();
app.UseAuthorization();

app.Run();

namespace JobSearchApp.Api
{
    public class RegisterRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}