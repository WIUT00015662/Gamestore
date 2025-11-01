using Gamestore.BLL.DTOs.Order;
using Gamestore.BLL.Services;
using Gamestore.Domain.Entities;
using Gamestore.Domain.Exceptions;
using Gamestore.Domain.Repositories;
using Moq;

namespace GameStore.UnitTests.BLL.Services;

public class OrderServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGameRepository> _gameRepoMock;
    private readonly Mock<IOrderRepository> _orderRepoMock;
    private readonly Mock<IPaymentGatewayClient> _paymentGatewayClientMock;
    private readonly OrderSettings _orderSettings;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        _gameRepoMock = new Mock<IGameRepository>();
        _orderRepoMock = new Mock<IOrderRepository>();
        _paymentGatewayClientMock = new Mock<IPaymentGatewayClient>();

        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.Setup(u => u.Games).Returns(_gameRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Orders).Returns(_orderRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        _orderRepoMock.Setup(r => r.AddAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);

        _orderSettings = new OrderSettings
        {
            CustomerId = Guid.NewGuid(),
            BankInvoiceValidityDays = 3,
            PaymentRetryCount = 3,
        };

        _orderService = new OrderService(_unitOfWorkMock.Object, _paymentGatewayClientMock.Object, _orderSettings);
    }

    [Fact]
    public async Task AddGameToCartAsyncThrowsEntityNotFoundExceptionWhenGameNotFound()
    {
        _gameRepoMock.Setup(x => x.GetByKeyAsync("missing")).ReturnsAsync((Game?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => _orderService.AddGameToCartAsync("missing"));
    }

    [Fact]
    public async Task AddGameToCartAsyncCreatesOpenOrderAndAddsGameWhenOrderDoesNotExist()
    {
        var game = new Game { Id = Guid.NewGuid(), Name = "Test", Key = "test", Price = 100, UnitInStock = 5, Discount = 10 };
        _gameRepoMock.Setup(x => x.GetByKeyAsync(game.Key)).ReturnsAsync(game);
        _orderRepoMock.Setup(r => r.GetOpenOrderByCustomerIdAsync(_orderSettings.CustomerId)).ReturnsAsync((Order?)null);

        await _orderService.AddGameToCartAsync(game.Key);

        _orderRepoMock.Verify(r => r.AddAsync(It.Is<Order>(o => o.CustomerId == _orderSettings.CustomerId && o.Status == OrderStatus.Open)), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Exactly(2));
    }

    [Fact]
    public async Task AddGameToCartAsyncIncrementsQuantityWhenGameAlreadyInCart()
    {
        var game = new Game { Id = Guid.NewGuid(), Name = "Test", Key = "test", Price = 30, UnitInStock = 3, Discount = 0 };
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = _orderSettings.CustomerId,
            Status = OrderStatus.Open,
            OrderGames =
            [
                new OrderGame { ProductId = game.Id, Price = game.Price, Quantity = 1, Discount = game.Discount, Product = game },
            ],
        };

        _gameRepoMock.Setup(x => x.GetByKeyAsync(game.Key)).ReturnsAsync(game);
        _orderRepoMock.Setup(r => r.GetOpenOrderByCustomerIdAsync(_orderSettings.CustomerId)).ReturnsAsync(order);

        await _orderService.AddGameToCartAsync(game.Key);

        Assert.Equal(2, order.OrderGames.Single().Quantity);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveGameFromCartAsyncDeletesOrderWhenLastGameRemoved()
    {
        var game = new Game { Id = Guid.NewGuid(), Name = "Test", Key = "test" };
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = _orderSettings.CustomerId,
            Status = OrderStatus.Open,
            OrderGames =
            [
                new OrderGame { ProductId = game.Id, Product = game, Quantity = 1, Price = 15 },
            ],
        };

        _orderRepoMock.Setup(r => r.GetOpenOrderByCustomerIdAsync(_orderSettings.CustomerId)).ReturnsAsync(order);

        await _orderService.RemoveGameFromCartAsync(game.Key);

        Assert.Empty(order.OrderGames);
        _orderRepoMock.Verify(r => r.Delete(order), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetCartAsyncReturnsEmptyCollectionWhenNoOpenOrder()
    {
        _orderRepoMock.Setup(r => r.GetOpenOrderByCustomerIdAsync(_orderSettings.CustomerId)).ReturnsAsync((Order?)null);

        var result = await _orderService.GetCartAsync();

        Assert.Empty(result);
    }

    [Fact]
    public void GetPaymentMethodsReturnsThreeMethods()
    {
        var result = _orderService.GetPaymentMethods();

        Assert.Equal(3, result.PaymentMethods.Count());
        Assert.Contains(result.PaymentMethods, p => p.Title == "Bank");
        Assert.Contains(result.PaymentMethods, p => p.Title == "IBox terminal");
        Assert.Contains(result.PaymentMethods, p => p.Title == "Visa");
    }

    [Fact]
    public async Task PayByIBoxAsyncRetriesAndMarksOrderPaidWhenGatewayReturnsResponse()
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = _orderSettings.CustomerId,
            Status = OrderStatus.Open,
            OrderGames =
            [
                new OrderGame { ProductId = Guid.NewGuid(), Price = 50, Quantity = 2, Discount = 10 },
            ],
        };

        var paymentResponse = new IBoxPaymentResponse
        {
            UserId = _orderSettings.CustomerId,
            OrderId = order.Id,
            PaymentDate = DateTime.UtcNow,
            Sum = 90,
        };

        _orderRepoMock.Setup(r => r.GetOpenOrderByCustomerIdAsync(_orderSettings.CustomerId)).ReturnsAsync(order);
        _paymentGatewayClientMock
            .SetupSequence(p => p.PayIBoxAsync(_orderSettings.CustomerId, order.Id, It.IsAny<double>(), It.IsAny<DateTime>()))
            .ReturnsAsync((IBoxPaymentResponse?)null)
            .ReturnsAsync(paymentResponse);

        var result = await _orderService.PayByIBoxAsync();

        Assert.Same(paymentResponse, result);
        Assert.Equal(OrderStatus.Paid, order.Status);
        Assert.NotNull(order.Date);
        _paymentGatewayClientMock.Verify(
            p => p.PayIBoxAsync(_orderSettings.CustomerId, order.Id, It.IsAny<double>(), It.IsAny<DateTime>()),
            Times.Exactly(2));
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Exactly(2));
    }

    [Fact]
    public async Task PayByVisaAsyncThrowsAndCancelsOrderWhenAllRetriesFail()
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = _orderSettings.CustomerId,
            Status = OrderStatus.Open,
            OrderGames =
            [
                new OrderGame { ProductId = Guid.NewGuid(), Price = 100, Quantity = 1, Discount = 0 },
            ],
        };
        var model = new VisaPaymentModel
        {
            Holder = "Test User",
            CardNumber = "1111222233334444",
            MonthExpire = 12,
            YearExpire = 2030,
            Cvv2 = 123,
        };

        _orderRepoMock.Setup(r => r.GetOpenOrderByCustomerIdAsync(_orderSettings.CustomerId)).ReturnsAsync(order);
        _paymentGatewayClientMock.Setup(p => p.PayVisaAsync(model, It.IsAny<double>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<ArgumentException>(() => _orderService.PayByVisaAsync(model));

        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.NotNull(order.Date);
        _paymentGatewayClientMock.Verify(p => p.PayVisaAsync(model, It.IsAny<double>()), Times.Exactly(_orderSettings.PaymentRetryCount));
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Exactly(2));
    }

    [Fact]
    public async Task PayByVisaAsyncMarksOrderPaidWhenPaymentSucceeds()
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = _orderSettings.CustomerId,
            Status = OrderStatus.Open,
            OrderGames =
            [
                new OrderGame { ProductId = Guid.NewGuid(), Price = 120, Quantity = 1, Discount = 20 },
            ],
        };
        var model = new VisaPaymentModel
        {
            Holder = "Test User",
            CardNumber = "1111222233334444",
            MonthExpire = 12,
            YearExpire = 2030,
            Cvv2 = 123,
        };

        _orderRepoMock.Setup(r => r.GetOpenOrderByCustomerIdAsync(_orderSettings.CustomerId)).ReturnsAsync(order);
        _paymentGatewayClientMock.Setup(p => p.PayVisaAsync(model, It.IsAny<double>())).ReturnsAsync(true);

        await _orderService.PayByVisaAsync(model);

        Assert.Equal(OrderStatus.Paid, order.Status);
        Assert.NotNull(order.Date);
        _paymentGatewayClientMock.Verify(p => p.PayVisaAsync(model, It.IsAny<double>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Exactly(2));
    }
}
