using Microsoft.Net.Http.Headers;

namespace Gamestore.Api.Middleware;

/// <summary>
/// Middleware that adds cache control headers to GET responses.
/// </summary>
public class CacheControlMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">HTTP context.</param>
    /// <returns>A task representing the operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Method == HttpMethods.Get)
        {
            context.Response.GetTypedHeaders().CacheControl =
                new CacheControlHeaderValue
                {
                    Public = true,
                    MaxAge = TimeSpan.FromMinutes(1),
                };
        }

        await _next(context);
    }
}
