using Gamestore.BLL.DTOs.Deals;
using Gamestore.BLL.Services;
using Gamestore.Domain.Entities;
using Gamestore.Domain.Repositories;

namespace Gamestore.Api.Services;

public class DiscountPollingHostedService(
    IServiceProvider serviceProvider,
    ILogger<DiscountPollingHostedService> logger) : BackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<DiscountPollingHostedService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Discount polling hosted service starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var config = await GetConfigurationAsync();
                var delayMilliseconds = config.TimeWindowMinutes * 60 * 1000;

                _logger.LogInformation("Waiting {Minutes} minutes before next discount polling run", config.TimeWindowMinutes);
                await Task.Delay(delayMilliseconds, stoppingToken);

                if (!stoppingToken.IsCancellationRequested && config.IsActive)
                {
                    await PollDiscountsAsync();
                }
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogInformation(ex, "Discount polling service cancelled");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in discount polling hosted service");
                // Wait a bit before retrying
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

    private async Task<DiscountConfiguration> GetConfigurationAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var configService = scope.ServiceProvider.GetRequiredService<IDiscountConfigurationService>();
        return await configService.GetActiveConfigurationAsync();
    }

    private async Task PollDiscountsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var configService = scope.ServiceProvider.GetRequiredService<IDiscountConfigurationService>();
        var processingService = scope.ServiceProvider.GetRequiredService<IDiscountProcessingService>();
        var notificationService = scope.ServiceProvider.GetRequiredService<IDiscountNotificationService>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        try
        {
            var config = await configService.GetActiveConfigurationAsync();
            _logger.LogInformation("Starting discount polling run");

            var pollingRun = await processingService.ProcessDiscountsAsync(config);

            if (pollingRun.Status == "Completed")
            {
                // Step 5: Send email notifications
                var snapshots = (await unitOfWork.GameDiscountSnapshots.FindAsync(
                    x => x.PollingRunId == pollingRun.Id && x.IsNewDiscount)).ToList();

                // Group by game and select highest discount
                var groupedByGame = snapshots
                    .GroupBy(x => x.GameId)
                    .SelectMany(g => g.OrderByDescending(x => x.DiscountPercent).Take(1))
                    .OrderByDescending(x => x.DiscountPercent)
                    .ToList();

                if (groupedByGame.Count > 0)
                {
                    var emailSubscriptions = await unitOfWork.EmailSubscriptions.FindAsync(x => x.IsActive);
                    var recipients = emailSubscriptions.Select(x => x.Email).ToList();

                    if (recipients.Count > 0)
                    {
                        var responses = groupedByGame.Select(s => new DiscountedGameResponse
                        {
                            GameId = s.GameId,
                            GameName = s.GameName,
                            Vendor = s.Vendor,
                            PurchaseUrl = s.PurchaseUrl,
                            OriginalPrice = s.OriginalPrice,
                            DiscountedPrice = s.DiscountedPrice,
                            DiscountPercent = s.DiscountPercent,
                        });

                        await notificationService.NotifyUsersAsync(recipients, responses);
                        _logger.LogInformation("Sent discount notifications to {RecipientCount} subscribers", recipients.Count);
                    }
                }

                _logger.LogInformation("Discount polling run completed successfully");
            }
            else if (pollingRun.Status == "Failed")
            {
                _logger.LogError("Discount polling run failed: {Error}", pollingRun.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute discount polling");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Discount polling hosted service stopping");
        await base.StopAsync(cancellationToken);
    }
}
