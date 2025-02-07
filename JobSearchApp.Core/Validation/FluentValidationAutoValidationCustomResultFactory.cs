using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Results;

namespace JobSearchApp.Core.Validation;

public class FluentValidationAutoValidationCustomResultFactory : IFluentValidationAutoValidationResultFactory
{
    public IResult CreateResult(EndpointFilterInvocationContext context, ValidationResult validationResult)
    {
        return Results.UnprocessableEntity(validationResult.Errors);
    }
}