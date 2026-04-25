using Gamestore.Api.Controllers;
using Gamestore.Api.Services;
using Gamestore.BLL.DTOs.Order;
using Gamestore.BLL.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GameStore.UnitTests.Api.Controllers;

public class OrdersControllerTests
{
    private readonly Mock<IOrderService> _orderServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly OrdersController _controller;
    private readonly Guid _testUserId = Guid.NewGuid();

    public OrdersControllerTests()
    {
        _orderServiceMock = new Mock<IOrderService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _controller = new OrdersController(_orderServiceMock.Object, _currentUserServiceMock.Object);
        _currentUserServiceMock.Setup(s => s.GetUserId()).Returns(_testUserId);
    }

    [Fact]
    public async Task DeleteGameFromCartReturnsNoContent()
    {
        _orderServiceMock.Setup(s => s.RemoveGameFromCartAsync("test-game", _testUserId)).Returns(Task.CompletedTask);

        var result = await _controller.DeleteGameFromCart("test-game");

        Assert.IsType<NoContentResult>(result);
        _orderServiceMock.Verify(s => s.RemoveGameFromCartAsync("test-game", _testUserId), Times.Once);
    }

    [Fact]
    public async Task GetMyOrdersReturnsOkWithOrders()
    {
        var orders = new List<OrderResponse>
        {
            new() { Id = Guid.NewGuid(), CustomerId = _testUserId, Date = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), CustomerId = _testUserId, Date = DateTime.UtcNow.AddDays(-1) },
        };

        _orderServiceMock.Setup(s => s.GetMyOrdersAsync(_testUserId)).ReturnsAsync(orders);

        var result = await _controller.GetMyOrders();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(orders, okResult.Value);
    }

    [Fact]
    public async Task GetMyOrderReturnsOkWithOrder()
    {
        var id = Guid.NewGuid();
        var order = new OrderResponse { Id = id, CustomerId = _testUserId, Date = DateTime.UtcNow };
        _orderServiceMock.Setup(s => s.GetMyOrderByIdAsync(id, _testUserId)).ReturnsAsync(order);

        var result = await _controller.GetMyOrder(id);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(order, okResult.Value);
    }

    [Fact]
    public async Task GetMyOrderDetailsReturnsOkWithDetails()
    {
        var id = Guid.NewGuid();
        var details = new List<OrderGameResponse>
        {
            new() { ProductId = Guid.NewGuid(), Price = 10, Quantity = 2, Discount = 0 },
            new() { ProductId = Guid.NewGuid(), Price = 20, Quantity = 1, Discount = 5 },
        };

        _orderServiceMock.Setup(s => s.GetMyOrderDetailsAsync(id, _testUserId)).ReturnsAsync(details);

        var result = await _controller.GetMyOrderDetails(id);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(details, okResult.Value);
    }

    [Fact]
    public async Task GetAllOrdersReturnsOkWithOrders()
    {
        var orders = new List<OrderResponse>
        {
            new() { Id = Guid.NewGuid(), CustomerId = Guid.NewGuid(), Date = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), CustomerId = Guid.NewGuid(), Date = DateTime.UtcNow.AddDays(-1) },
        };

        _orderServiceMock.Setup(s => s.GetAllOrdersAsync()).ReturnsAsync(orders);

        var result = await _controller.GetAllOrders();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(orders, okResult.Value);
    }

    [Fact]
    public async Task GetCartReturnsOkWithCartItems()
    {
        var cart = new List<OrderGameResponse>
        {
            new() { ProductId = Guid.NewGuid(), Price = 30, Quantity = 1, Discount = 0 },
        };

        _orderServiceMock.Setup(s => s.GetCartAsync(_testUserId)).ReturnsAsync(cart);

        var result = await _controller.GetCart();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(cart, okResult.Value);
    }

    [Fact]
    public void GetPaymentMethodsReturnsOkWithMethods()
    {
        var methods = new PaymentMethodsResponse
        {
            PaymentMethods =
            [
                new() { Title = "Bank", Description = "Pay via invoice", ImageUrl = "url" },
                new() { Title = "Visa", Description = "Pay via card", ImageUrl = "url" },
            ],
        };

        _orderServiceMock.Setup(s => s.GetPaymentMethods()).Returns(methods);

        var result = _controller.GetPaymentMethods();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(methods, okResult.Value);
    }

    [Fact]
    public async Task PayBankReturnsPdfFileResult()
    {
        var invoice = new BankInvoiceResponse
        {
            Content = [1, 2, 3],
            FileName = "invoice.pdf",
        };

        _orderServiceMock.Setup(s => s.PayByBankAsync(_testUserId)).ReturnsAsync(invoice);

        var result = await _controller.Pay(new PaymentRequest { Method = PaymentMethodType.Bank });

        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", fileResult.ContentType);
        Assert.Equal("invoice.pdf", fileResult.FileDownloadName);
        Assert.Equal(invoice.Content, fileResult.FileContents);
    }

    [Fact]
    public async Task PayVisaReturnsNoContentWhenModelIsProvided()
    {
        var model = new VisaPaymentModel
        {
            Holder = "Vitalii",
            CardNumber = "123321122344231",
            MonthExpire = 10,
            YearExpire = 2030,
            Cvv2 = 111,
        };

        _orderServiceMock.Setup(s => s.PayByVisaAsync(model, _testUserId)).Returns(Task.CompletedTask);

        var result = await _controller.Pay(new PaymentRequest
        {
            Method = PaymentMethodType.Visa,
            Model = model,
        });

        Assert.IsType<NoContentResult>(result);
        _orderServiceMock.Verify(s => s.PayByVisaAsync(model, _testUserId), Times.Once);
    }

    [Fact]
    public async Task PayThrowsWhenMethodIsUnsupported()
    {
        var request = new PaymentRequest { Method = (PaymentMethodType)999 };

        await Assert.ThrowsAsync<ArgumentException>(() => _controller.Pay(request));
    }
}
