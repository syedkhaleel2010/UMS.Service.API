using System.Globalization;
using Microsoft.AspNetCore.Localization;

namespace UMS.Service.API.Middleware
{
    public class LocalizationMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly string[] SupportedLanguages = { "en", "ar" };

        public LocalizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
           
            var langHeader = context.Request.Headers["language"].ToString();

           
            var culture = "en";

            if (!string.IsNullOrEmpty(langHeader) &&
                SupportedLanguages.Contains(langHeader))
            {
                culture = langHeader;
            }

            var ci = new CultureInfo(culture);

            // Apply culture for current request
            CultureInfo.CurrentCulture = ci;
            CultureInfo.CurrentUICulture = ci;

            await _next(context);
        }
    }

    
    public static class LocalizationMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomLocalization(this IApplicationBuilder app)
        {
            return app.UseMiddleware<LocalizationMiddleware>();
        }
    }
}
