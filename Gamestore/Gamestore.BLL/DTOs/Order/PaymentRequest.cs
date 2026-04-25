using System.ComponentModel.DataAnnotations;

namespace Gamestore.BLL.DTOs.Order;

public class PaymentRequest : IValidatableObject
{
    public PaymentMethodType? Method { get; set; }

    public VisaPaymentModel? Model { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Method is null)
        {
            yield return new ValidationResult("Payment method is required.", [nameof(Method)]);
            yield break;
        }

        if (Method is PaymentMethodType.Visa && Model is null)
        {
            yield return new ValidationResult("Visa payment model is required when method is Visa.", [nameof(Model)]);
        }
    }
}
