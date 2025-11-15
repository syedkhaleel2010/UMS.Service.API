using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;
using UMS.Service.API.Common;

namespace UMS.Service.API.Middleware
{
    public class CustomAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

        public async Task HandleAsync(
            RequestDelegate next,
            HttpContext context,
            AuthorizationPolicy policy,
            PolicyAuthorizationResult authorizeResult)
        {
            // If authorization succeeded, continue with the request pipeline
            if (authorizeResult.Succeeded)
            {
                await next(context);
                return;
            }

            // Handle authorization failure
            if (authorizeResult.Challenged)
            {
                // 401 Unauthorized - Authentication required
                await HandleUnauthorizedAsync(context, "Authentication required. Please provide a valid token.");
                return;
            }

            if (authorizeResult.Forbidden)
            {
                // 403 Forbidden - User is authenticated but doesn't have required permissions
                var message = GetForbiddenMessage(context, policy);
                await HandleForbiddenAsync(context, message);
                return;
            }

            // Fallback to default handler
            await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }

        private static string GetForbiddenMessage(HttpContext context, AuthorizationPolicy policy)
        {
            // Check if it's a role-based authorization
            var roleRequirement = policy.Requirements
                .OfType<RolesAuthorizationRequirement>()
                .FirstOrDefault();

            if (roleRequirement != null && roleRequirement.AllowedRoles.Any())
            {
                var roles = string.Join(" or ", roleRequirement.AllowedRoles);
                return $"Access denied. This operation requires one of the following roles: {roles}.";
            }

           
            // Generic forbidden message
            return "Access denied. You do not have permission to access this resource.";
        }

        private static async Task HandleUnauthorizedAsync(HttpContext context, string message)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/json";

            var errorResponse = ServiceResponse.ErrorResponse(message);

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            var jsonResponse = JsonSerializer.Serialize(errorResponse, options);
            await context.Response.WriteAsync(jsonResponse);
        }

        private static async Task HandleForbiddenAsync(HttpContext context, string message)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            context.Response.ContentType = "application/json";

            var errorResponse = ServiceResponse.ErrorResponse(message);

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            var jsonResponse = JsonSerializer.Serialize(errorResponse, options);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}

