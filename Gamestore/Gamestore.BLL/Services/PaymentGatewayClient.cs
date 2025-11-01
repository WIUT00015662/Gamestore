using Gamestore.BLL.DTOs.Order;
using Microsoft.Extensions.Logging;

namespace Gamestore.BLL.Services;

public class PaymentGatewayClient(PaymentGatewaySettings settings, ILogger<PaymentGatewayClient> logger) : IPaymentGatewayClient
{
    private readonly PaymentGatewaySettings _settings = settings;
    private readonly ILogger<PaymentGatewayClient> _logger = logger;

    public Task<IBoxPaymentResponse?> PayIBoxAsync(Guid userId, Guid orderId, double sum, DateTime paymentDate)
    {
        if (!_settings.SimulationEnabled)
        {
            _logger.LogWarning("Payment simulation is disabled.");
            return Task.FromResult<IBoxPaymentResponse?>(null);
        }

        var success = Random.Shared.NextDouble() <= Clamp(_settings.IBoxSuccessRate);
        if (!success)
        {
            _logger.LogInformation("Simulated IBox payment failed for order {OrderId}", orderId);
            return Task.FromResult<IBoxPaymentResponse?>(null);
        }

        _logger.LogInformation("Simulated IBox payment succeeded for order {OrderId}", orderId);
        return Task.FromResult<IBoxPaymentResponse?>(new IBoxPaymentResponse
        {
            UserId = userId,
            OrderId = orderId,
            PaymentDate = paymentDate,
            Sum = sum,
        });
    }

    public Task<bool> PayVisaAsync(VisaPaymentModel model, double sum)
    {
        if (!_settings.SimulationEnabled)
        {
            _logger.LogWarning("Payment simulation is disabled.");
            return Task.FromResult(false);
        }

        var cardLooksValid = model.CardNumber.Length >= 12 && model.Cvv2 is >= 100 and <= 9999;
        var success = cardLooksValid && Random.Shared.NextDouble() <= Clamp(_settings.VisaSuccessRate);

        _logger.LogInformation(
            "Simulated Visa payment {Result} for holder {Holder} and amount {Amount}",
            success ? "succeeded" : "failed",
            model.Holder,
            sum);

        return Task.FromResult(success);
    }

    private static double Clamp(double value)
    {
        return Math.Max(0, Math.Min(1, value));
    }
}
