namespace Gamestore.BLL.Services;

public interface IDiscountSimulationService
{
    decimal GenerateDiscountPercent();

    bool ShouldApplyDiscount(decimal probability);

    bool ShouldRevertDiscount(decimal probability);

    decimal GenerateDiscountPercent(decimal minPercent, decimal maxPercent);
}
