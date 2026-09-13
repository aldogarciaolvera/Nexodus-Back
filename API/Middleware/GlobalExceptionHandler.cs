using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nexodus_Back.Core.Exceptions;

namespace Nexodus_Back.API.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = new ProblemDetails
        {
            Instance = httpContext.Request.Path
        };

        if (exception is ValidationException validationException)
        {
            problemDetails.Title = "Validation Error";
            problemDetails.Status = StatusCodes.Status400BadRequest;
            problemDetails.Detail = validationException.Message;
            problemDetails.Extensions["errors"] = validationException.Errors;
        }
        else if (exception is NotFoundException notFoundException)
        {
            problemDetails.Title = "Resource Not Found";
            problemDetails.Status = StatusCodes.Status404NotFound;
            problemDetails.Detail = notFoundException.Message;
        }
        else if (exception is ConflictException conflictException)
        {
            problemDetails.Title = "Conflict";
            problemDetails.Status = StatusCodes.Status409Conflict;
            problemDetails.Detail = conflictException.Message;
        }
        else if (exception is UnauthorizedException unauthorizedException)
        {
            problemDetails.Title = "Unauthorized";
            problemDetails.Status = StatusCodes.Status401Unauthorized;
            problemDetails.Detail = unauthorizedException.Message;
        }
        else
        {
            problemDetails.Title = "Internal Server Error";
            problemDetails.Status = StatusCodes.Status500InternalServerError;
            problemDetails.Detail = "An unexpected error occurred.";
        }

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response
            .WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
