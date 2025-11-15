using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Net;
using System.Text.Json;
using UMS.Service.API.Common;

namespace UMS.Service.API.Middleware
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "An unhandled exception occurred. Request Path: {Path}, Method: {Method}, QueryString: {QueryString}, User: {User}",
                    context.Request.Path, 
                    context.Request.Method,
                    context.Request.QueryString,
                    context.User?.Identity?.Name ?? "Anonymous");

                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var response = context.Response;

            // Get localizer from service provider
            var localizer = context.RequestServices.GetService<IStringLocalizer<SharedResource>>();

            ServiceResponse<object> errorResponse;

            switch (exception)
            {
                case ValidationException validationEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse = ServiceResponse.ErrorResponse(localizer?["ValidationFailed"]?.ToString() ?? "Validation failed.");
                    errorResponse.Errorlst = validationEx.Errors.Select(e => new ErrorMessage
                    {
                        Error = e.ErrorMessage,
                        Value = e.AttemptedValue?.ToString()
                    }).ToList();
                    break;

                case UnauthorizedAccessException:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    errorResponse = ServiceResponse.ErrorResponse(localizer?["UnauthorizedAccess"]?.ToString() ?? "Unauthorized access. Please check your credentials.");
                    break;

                case KeyNotFoundException:
                case ArgumentNullException argEx:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse = ServiceResponse.ErrorResponse(
                        string.IsNullOrWhiteSpace(exception.Message) 
                            ? (localizer?["ResourceNotFound"]?.ToString() ?? "The requested resource was not found.")
                            : exception.Message);
                    break;

                case ArgumentException:
                case InvalidOperationException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse = ServiceResponse.ErrorResponse(
                        string.IsNullOrWhiteSpace(exception.Message) 
                            ? (localizer?["InvalidRequest"]?.ToString() ?? "Invalid request.")
                            : exception.Message);
                    break;

                case BadHttpRequestException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse = ServiceResponse.ErrorResponse(localizer?["InvalidHttpRequest"]?.ToString() ?? "Invalid HTTP request.");
                    break;

                case TimeoutException:
                    response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                    errorResponse = ServiceResponse.ErrorResponse(localizer?["RequestTimedOut"]?.ToString() ?? "The request timed out. Please try again.");
                    break;

                case DbUpdateException dbEx:
                    // Handle database update exceptions (e.g., constraint violations, duplicate keys)
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    var dbMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                    
                    // Check for common database constraint violations
                    if (dbMessage.Contains("UNIQUE") || dbMessage.Contains("duplicate"))
                    {
                        errorResponse = ServiceResponse.ErrorResponse(localizer?["RecordAlreadyExists"]?.ToString() ?? "A record with this information already exists.");
                    }
                    else if (dbMessage.Contains("FOREIGN KEY") || dbMessage.Contains("constraint"))
                    {
                        errorResponse = ServiceResponse.ErrorResponse(localizer?["DatabaseConstraintViolation"]?.ToString() ?? "The operation violates a database constraint.");
                    }
                    else
                    {
                        errorResponse = ServiceResponse.ErrorResponse(localizer?["DatabaseError"]?.ToString() ?? "A database error occurred while processing your request.");
                    }
                    break;

                
                default:
                    // For production, don't expose internal exception details
                    var isDevelopment = context.RequestServices
                        .GetService<IWebHostEnvironment>()?.IsDevelopment() ?? false;

                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse = ServiceResponse.ErrorResponse(
                        isDevelopment 
                            ? exception.Message 
                            : (localizer?["ErrorOccurredProcessingRequest"]?.ToString() ?? "An error occurred while processing your request. Please try again later."));
                    
                    if (isDevelopment)
                    {
                        errorResponse.Errorlst = new List<ErrorMessage>
                        {
                            new ErrorMessage { Error = exception.Message, Value = exception.StackTrace }
                        };
                    }
                    break;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            var jsonResponse = JsonSerializer.Serialize(errorResponse, options);
            await response.WriteAsync(jsonResponse);
        }
    }

    public static class GlobalExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
        }
    }
}

