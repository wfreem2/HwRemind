using HwRemind.Extensions;
using Microsoft.Extensions.Caching.Distributed;

namespace HwRemind.Middleware
{
    public class JWTRevokedMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDistributedCache _cache;

        public JWTRevokedMiddleware(RequestDelegate next, IDistributedCache cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.GetJWT();
            
            var isRevoked = !string.IsNullOrEmpty(await _cache.GetStringAsync(token));

            if (isRevoked)
            {
                context.Response.StatusCode = 401;
                return;
            }

            await _next(context);
        }
    }
}
