using Gamestore.BLL.DTOs.Order;
using Gamestore.BLL.Mapping;
using Gamestore.Domain.Entities;
using Gamestore.Domain.Exceptions;
using Gamestore.Domain.Repositories;
using QuestPDF.Fluent;

namespace Gamestore.BLL.Services;

public class OrderService(
    IUnitOfWork unitOfWork,
    IPaymentGatewayClient paymentGatewayClient,
    OrderSettings orderSettings) : IOrderService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IPaymentGatewayClient _paymentGatewayClient = paymentGatewayClient;
    private readonly OrderSettings _orderSettings = orderSettings;

    public async Task AddGameToCartAsync(string gameKey)
    {
        var game = await _unitOfWork.Games.GetByKeyAsync(gameKey)
            ?? throw new EntityNotFoundException(nameof(Game), gameKey);

        var order = await GetOrCreateOpenOrderAsync();
        AddOrIncreaseOrderGame(order, game);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveGameFromCartAsync(string gameKey)
    {
        var order = await _unitOfWork.Orders.GetOpenOrderByCustomerIdAsync(_orderSettings.CustomerId)
            ?? throw new EntityNotFoundException(nameof(Order), "Open cart was not found.");

        var orderGame = order.OrderGames.FirstOrDefault(og => og.Product is not null && og.Product.Key == gameKey)
            ?? throw new EntityNotFoundException(nameof(Game), gameKey);

        order.OrderGames.Remove(orderGame);

        if (order.OrderGames.Count == 0)
        {
            _unitOfWork.Orders.Delete(order);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AddGameToOrderAsync(Guid orderId, string gameKey)
    {
        var order = await _unitOfWork.Orders.GetByIdWithDetailsAsync(orderId)
            ?? throw new EntityNotFoundException(nameof(Order), orderId);

        EnsureOrderIsOpen(order);

        var game = await _unitOfWork.Games.GetByKeyAsync(gameKey)
            ?? throw new EntityNotFoundException(nameof(Game), gameKey);

        AddOrIncreaseOrderGame(order, game);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateOrderDetailQuantityAsync(Guid detailId, int count)
    {
        if (count <= 0)
        {
            throw new ArgumentException("Count must be greater than zero.");
        }

        var order = await FindOrderByDetailIdAsync(detailId)
            ?? throw new EntityNotFoundException(nameof(OrderGame), detailId);

        EnsureOrderIsOpen(order);

        var detail = order.OrderGames.First(og => og.ProductId == detailId);

        if (detail.Product is not null && count > detail.Product.UnitInStock)
        {
            throw new ArgumentException("Cannot add more games than available in stock.");
        }

        detail.Quantity = count;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteOrderDetailAsync(Guid detailId)
    {
        var order = await FindOrderByDetailIdAsync(detailId)
            ?? throw new EntityNotFoundException(nameof(OrderGame), detailId);

        EnsureOrderIsOpen(order);

        var detail = order.OrderGames.First(og => og.ProductId == detailId);
        order.OrderGames.Remove(detail);

        if (order.OrderGames.Count == 0)
        {
            _unitOfWork.Orders.Delete(order);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ShipOrderAsync(Guid orderId)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId)
            ?? throw new EntityNotFoundException(nameof(Order), orderId);

        if (order.Status != OrderStatus.Paid)
        {
            throw new ArgumentException("Only paid order can be shipped.");
        }

        order.Status = OrderStatus.Shipped;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<OrderResponse>> GetOrdersAsync()
    {
        var orders = await _unitOfWork.Orders.GetByStatusesAsync(OrderStatus.Paid, OrderStatus.Cancelled, OrderStatus.Shipped);
        var threshold = DateTime.UtcNow.AddDays(-30);
        return orders.Where(o => o.Date is not null && o.Date <= threshold).ToOrderResponse();
    }

    public async Task<OrderResponse> GetOrderByIdAsync(Guid orderId)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId)
            ?? throw new EntityNotFoundException(nameof(Order), orderId);

        return order.ToOrderResponse();
    }

    public async Task<IEnumerable<OrderGameResponse>> GetOrderDetailsAsync(Guid orderId)
    {
        var order = await _unitOfWork.Orders.GetByIdWithDetailsAsync(orderId)
            ?? throw new EntityNotFoundException(nameof(Order), orderId);

        return order.OrderGames.ToOrderGameResponse();
    }

    public async Task<IEnumerable<OrderGameResponse>> GetCartAsync()
    {
        var order = await _unitOfWork.Orders.GetOpenOrderByCustomerIdAsync(_orderSettings.CustomerId);
        return order?.OrderGames.ToOrderGameResponse() ?? [];
    }

    public PaymentMethodsResponse GetPaymentMethods()
    {
        return new PaymentMethodsResponse
        {
            PaymentMethods =
            [
                new PaymentMethodResponse
                {
                    ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/4/41/LINE_logo.svg/960px-LINE_logo.svg.png?_=20220419085336",
                    Title = "Bank",
                    Description = "Pay via generated bank invoice.",
                },
                new PaymentMethodResponse
                {
                    ImageUrl = "https://encrypted-tbn1.gstatic.com/images?q=tbn:ANd9GcSzcJU0fSXRm3i6CGPBmhF5q6DR25GB7f6aXid3XUGWlWw-B581MA_qxEP6dY7R",
                    Title = "IBox terminal",
                    Description = "Pay in a terminal after invoice generation.",
                },
                new PaymentMethodResponse
                {
                    ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/5/5c/Visa_Inc._logo_%282021%E2%80%93present%29.svg/1280px-Visa_Inc._logo_%282021%E2%80%93present%29.svg.png",
                    Title = "Visa",
                    Description = "Pay using your Visa card.",
                },
            ],
        };
    }

    public async Task<BankInvoiceResponse> PayByBankAsync()
    {
        var order = await StartCheckoutAsync();
        var sum = GetOrderSum(order);
        var createdAt = DateTime.UtcNow;
        var validUntil = createdAt.AddDays(_orderSettings.BankInvoiceValidityDays);

        order.Status = OrderStatus.Paid;
        order.Date = createdAt;
        await _unitOfWork.SaveChangesAsync();

        return new BankInvoiceResponse
        {
            Content = CreateInvoicePdf(_orderSettings.CustomerId, order.Id, createdAt, validUntil, sum),
            FileName = $"invoice-{order.Id}.pdf",
        };
    }

    public async Task<IBoxPaymentResponse> PayByIBoxAsync()
    {
        var order = await StartCheckoutAsync();
        var sum = GetOrderSum(order);
        var paymentDate = DateTime.UtcNow;

        for (var attempt = 0; attempt < _orderSettings.PaymentRetryCount; attempt++)
        {
            if (await _paymentGatewayClient.PayIBoxAsync(_orderSettings.CustomerId, order.Id, sum, paymentDate) is { } response)
            {
                order.Status = OrderStatus.Paid;
                order.Date = paymentDate;
                await _unitOfWork.SaveChangesAsync();
                return response;
            }
        }

        order.Status = OrderStatus.Cancelled;
        order.Date = paymentDate;
        await _unitOfWork.SaveChangesAsync();

        throw new ArgumentException("IBox payment failed.");
    }

    public async Task PayByVisaAsync(VisaPaymentModel model)
    {
        var order = await StartCheckoutAsync();
        var sum = GetOrderSum(order);
        var paymentDate = DateTime.UtcNow;

        for (var attempt = 0; attempt < _orderSettings.PaymentRetryCount; attempt++)
        {
            var success = await _paymentGatewayClient.PayVisaAsync(model, sum);
            if (success)
            {
                order.Status = OrderStatus.Paid;
                order.Date = paymentDate;
                await _unitOfWork.SaveChangesAsync();
                return;
            }
        }

        order.Status = OrderStatus.Cancelled;
        order.Date = paymentDate;
        await _unitOfWork.SaveChangesAsync();

        throw new ArgumentException("Visa payment failed.");
    }

    private static void EnsureOrderIsOpen(Order order)
    {
        if (order.Status != OrderStatus.Open)
        {
            throw new ArgumentException("Only open orders can be modified.");
        }
    }

    private static void AddOrIncreaseOrderGame(Order order, Game game)
    {
        var detail = order.OrderGames.FirstOrDefault(og => og.ProductId == game.Id);
        if (detail is null)
        {
            if (game.UnitInStock < 1)
            {
                throw new ArgumentException("Game is out of stock.");
            }

            order.OrderGames.Add(new OrderGame
            {
                OrderId = order.Id,
                ProductId = game.Id,
                Product = game,
                Quantity = 1,
                Price = game.Price,
                Discount = game.Discount,
            });

            return;
        }

        if (detail.Quantity >= game.UnitInStock)
        {
            throw new ArgumentException("Cannot add more games than available in stock.");
        }

        detail.Quantity++;
    }

    private async Task<Order?> FindOrderByDetailIdAsync(Guid detailId)
    {
        var allOrders = await _unitOfWork.Orders.GetByStatusesAsync(
            OrderStatus.Open,
            OrderStatus.Checkout,
            OrderStatus.Paid,
            OrderStatus.Cancelled,
            OrderStatus.Shipped);

        var owner = allOrders.FirstOrDefault(o => o.OrderGames.Any(og => og.ProductId == detailId));
        return owner is null ? null : await _unitOfWork.Orders.GetByIdWithDetailsAsync(owner.Id);
    }

    private async Task<Order> StartCheckoutAsync()
    {
        var order = await _unitOfWork.Orders.GetOpenOrderByCustomerIdAsync(_orderSettings.CustomerId)
            ?? throw new EntityNotFoundException(nameof(Order), "Open cart was not found.");

        if (order.OrderGames.Count == 0)
        {
            throw new ArgumentException("Cart is empty.");
        }

        order.Status = OrderStatus.Checkout;
        await _unitOfWork.SaveChangesAsync();
        return order;
    }

    private async Task<Order> GetOrCreateOpenOrderAsync()
    {
        var order = await _unitOfWork.Orders.GetOpenOrderByCustomerIdAsync(_orderSettings.CustomerId);
        if (order is not null)
        {
            return order;
        }

        order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = _orderSettings.CustomerId,
            Status = OrderStatus.Open,
        };

        await _unitOfWork.Orders.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();
        return order;
    }

    private static double GetOrderSum(Order order)
    {
        return order.OrderGames.Sum(item => item.Price * item.Quantity * (100 - (item.Discount ?? 0)) / 100.0d);
    }

    private static byte[] CreateInvoicePdf(Guid customerId, Guid orderId, DateTime createdAt, DateTime validUntil, double sum)
    {
        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Margin(32);
                page.Content().Column(column =>
                {
                    column.Spacing(8);
                    column.Item().Text("Bank Invoice").FontSize(20).SemiBold();
                    column.Item().Text($"User ID: {customerId}");
                    column.Item().Text($"Order ID: {orderId}");
                    column.Item().Text($"Creation date: {createdAt:O}");
                    column.Item().Text($"Valid until: {validUntil:O}");
                    column.Item().Text($"Sum: {sum:0.00}").SemiBold();
                });
            });
        }).GeneratePdf();
    }
}
