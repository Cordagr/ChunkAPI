using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Bucket
{
    public class TokenBucketRateLimiterMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TokenBucket _bucket;

        public TokenBucketRateLimiterMiddleware(RequestDelegate next, TokenBucket bucket)
        {
            _next = next;
            _bucket = bucket;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (_bucket.TryConsume())
            {
                await _next(context);
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers["Retry-After"] = "1"; // Retry after 1 second
                await context.Response.WriteAsync("Rate limit exceeded. Try again later.");
            }
        }
    }
}
