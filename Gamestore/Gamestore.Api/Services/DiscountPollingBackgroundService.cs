using Gamestore.BLL.Services;

namespace Gamestore.Api.Services;

public class DiscountPollingBackgroundService(
    IServiceScopeFactory serviceScopeFactory,
    DiscountPollingOptions options,
    ILogger<DiscountPollingBackgroundService> logger) : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly DiscountPollingOptions _options = options;
    private readonly ILogger<DiscountPollingBackgroundService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromMinutes(Math.Max(1, _options.IntervalMinutes));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var dealsService = scope.ServiceProvider.GetRequiredService<IGameDealsService>();
                await dealsService.PollDiscountsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Discount polling failed");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }
}
