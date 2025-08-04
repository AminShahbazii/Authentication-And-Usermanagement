using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace AuthenticationApp.Middlewares
{
    public class GlobalException(RequestDelegate next, IHostEnvironment env)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }


        public (HttpStatusCode statusCode, string message) MapExceptionResponse(Exception exception)
        {
            return exception switch
            {
                ValidationException validationException => (HttpStatusCode.BadRequest, validationException.Message),
                UnauthorizedAccessException _ => (HttpStatusCode.Unauthorized, "Unauthorized access. please log in"),
                KeyNotFoundException _ => (HttpStatusCode.NotFound, "The requested resource was not found."),
                _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again later.")
            };
        }


        public async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, message) = MapExceptionResponse(exception);

            var response = new ProblemDetails
            {
                Title = statusCode.ToString(),
                Status = (int)statusCode,
                Detail = message
            };

            if (env.IsDevelopment())
            {
                response.Extensions["stackTrace"] = exception.StackTrace;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var json = JsonSerializer.Serialize(response);

            await context.Response.WriteAsync(json);
        }
    }
}
