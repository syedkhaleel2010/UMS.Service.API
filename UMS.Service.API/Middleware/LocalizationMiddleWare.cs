using Microsoft.AspNetCore.Localization;
using System.Globalization;

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
            var culture = "en";

            // Check query string first (e.g., ?culture=ar)
            var cultureQuery = context.Request.Query["culture"].ToString();
            if (!string.IsNullOrEmpty(cultureQuery) && SupportedLanguages.Contains(cultureQuery))
            {
                culture = cultureQuery;
            }
            else
            {
                // Check header (e.g., language: ar)
                var langHeader = context.Request.Headers["language"].ToString();
                if (!string.IsNullOrEmpty(langHeader) && SupportedLanguages.Contains(langHeader))
                {
                    culture = langHeader;
                }
            }

            var ci = new CultureInfo(culture);

            // Set the culture for the current request using Thread.CurrentThread
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
