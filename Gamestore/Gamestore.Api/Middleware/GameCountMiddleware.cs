using Gamestore.BLL.Services;

namespace Gamestore.Api.Middleware;

public class GameCountMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context, IGameService gameService)
    {
        context.Response.OnStarting(async () =>
        {
            var totalGames = await gameService.GetGamesCountAsync();
            context.Response.Headers.Append("x-total-numbers-of-games", totalGames.ToString());
        });
        await _next(context);
    }
}
