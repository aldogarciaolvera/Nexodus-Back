using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using DomainValidationException = Nexodus_Back.Core.Exceptions.ValidationException;

namespace Nexodus_Back.API.Filters;

/// <summary>
/// Endpoint filter that intercepts incoming DTOs and runs FluentValidation before reaching endpoint handlers.
/// Throws a controlled ValidationException intercepted by GlobalExceptionHandler.
/// </summary>
/// <typeparam name="T">The type of the request payload to validate.</typeparam>
public class ValidationFilter<T> : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var argument = context.Arguments.OfType<T>().FirstOrDefault();
        if (argument is not null)
        {
            var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();
            if (validator is not null)
            {
                var validationResult = await validator.ValidateAsync(argument, context.HttpContext.RequestAborted);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                    throw new DomainValidationException(errors);
                }
            }
        }

        return await next(context);
    }
}
