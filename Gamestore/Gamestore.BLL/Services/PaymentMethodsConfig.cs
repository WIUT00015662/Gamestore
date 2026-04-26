namespace Gamestore.BLL.Services;

public class PaymentMethodsConfig
{
    public const string SectionName = "PaymentMethods";

    public List<PaymentMethodConfig> Methods { get; set; } = new()
    {
        new PaymentMethodConfig
        {
            ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/4/41/LINE_logo.svg/960px-LINE_logo.svg.png?_=20220419085336",
            Title = "Bank",
            Description = "Pay via generated bank invoice.",
        },
        new PaymentMethodConfig
        {
            ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/5/5c/Visa_Inc._logo_%282021%E2%80%93present%29.svg/1280px-Visa_Inc._logo_%282021%E2%80%93present%29.svg.png",
            Title = "Visa",
            Description = "Pay using your Visa card.",
        },
    };
}

public class PaymentMethodConfig
{
    public required string ImageUrl { get; set; }

    public required string Title { get; set; }

    public required string Description { get; set; }
}
