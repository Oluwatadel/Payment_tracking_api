using Microsoft.AspNetCore.Diagnostics;
using PaymentTracker.Exceptions;

namespace PaymentTracker
{
    public static class ExceptionMiddleware
    {
        public static void ConfigureExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(config =>
            {
                config.Run(async context =>
                {
                    var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                    context.Response.ContentType = "application/json";

                    context.Response.StatusCode = ex switch
                    {
                        System.ComponentModel.DataAnnotations.ValidationException => StatusCodes.Status400BadRequest,
                        AlreadyExistException => StatusCodes.Status409Conflict,
                        InvalidOperationException => StatusCodes.Status409Conflict,
                        NotFoundException => StatusCodes.Status404NotFound,
                        SaveOperationException => StatusCodes.Status500InternalServerError,
                        UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                        _ => StatusCodes.Status500InternalServerError
                    };
                    await context.Response.WriteAsJsonAsync(new
                    {
                        success = false,
                        message = ex?.Message,
                        statusCode = context.Response.StatusCode
                    });
                });
            });
        }
    }
}
