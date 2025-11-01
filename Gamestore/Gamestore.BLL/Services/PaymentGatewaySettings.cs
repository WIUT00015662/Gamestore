namespace Gamestore.BLL.Services;

public class PaymentGatewaySettings
{
    public bool SimulationEnabled { get; set; } = true;

    public double IBoxSuccessRate { get; set; } = 0.85;

    public double VisaSuccessRate { get; set; } = 0.9;
}
