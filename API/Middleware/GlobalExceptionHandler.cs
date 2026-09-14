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
        var statusCode = StatusCodes.Status500InternalServerError;
        var errorContent = new Dictionary<string, object>
        {
            { "message", "An unexpected error occurred." },
            { "endpoint", httpContext.GetEndpoint()?.DisplayName ?? httpContext.Request.Path.Value ?? "Unknown" }
        };

        if (exception is ValidationException validationException)
        {
            statusCode = StatusCodes.Status400BadRequest;
            errorContent["message"] = validationException.Message;
            errorContent["errors"] = validationException.Errors;
        }
        else if (exception is NotFoundException notFoundException)
        {
            statusCode = StatusCodes.Status404NotFound;
            errorContent["message"] = notFoundException.Message;
        }
        else if (exception is ConflictException conflictException)
        {
            statusCode = StatusCodes.Status409Conflict;
            errorContent["message"] = conflictException.Message;
        }
        else if (exception is UnauthorizedException unauthorizedException)
        {
            statusCode = StatusCodes.Status401Unauthorized;
            errorContent["message"] = unauthorizedException.Message;
        }
        else 
        {
            // Opcionalmente podrías incluir el mensaje real de la excepción aquí si se desea en entorno de desarrollo.
            errorContent["message"] = exception.Message;
        }

        var apiResponse = Nexodus_Back.Application.DTOs.Common.ApiResponse<object>.Error(errorContent, statusCode);

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response
            .WriteAsJsonAsync(apiResponse, cancellationToken);

        return true;
    }
}
