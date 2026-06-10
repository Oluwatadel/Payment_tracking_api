using System.Security.Claims;
using PaymentTracker.Services;

namespace PaymentTracker.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IJwtService jwtService)
        {
            var token = ExtractToken(context.Request.Headers);
            
            if (!string.IsNullOrEmpty(token))
            {
                var principal = jwtService.ValidateToken(token);
                if (principal != null)
                {
                    context.User = principal;
                }
            }

            await _next(context);
        }

        private static string? ExtractToken(IHeaderDictionary headers)
        {
            const string authorizationHeader = "Authorization";
            
            if (!headers.TryGetValue(authorizationHeader, out var headerValues))
                return null;

            var authorization = headerValues.ToString();
            if (string.IsNullOrEmpty(authorization))
                return null;

            const string scheme = "Bearer ";
            if (!authorization.StartsWith(scheme, StringComparison.OrdinalIgnoreCase))
                return null;

            return authorization.Substring(scheme.Length).Trim();
        }
    }
}
