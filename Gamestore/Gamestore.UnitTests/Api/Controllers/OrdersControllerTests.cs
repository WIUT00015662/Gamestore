using Gamestore.Api.Controllers;
using Gamestore.BLL.DTOs.Order;
using Gamestore.BLL.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GameStore.UnitTests.Api.Controllers;

public class OrdersControllerTests
{
    private readonly Mock<IOrderService> _orderServiceMock;
    private readonly OrdersController _controller;

    public OrdersControllerTests()
    {
        _orderServiceMock = new Mock<IOrderService>();
        _controller = new OrdersController(_orderServiceMock.Object);
    }

    [Fact]
    public async Task DeleteGameFromCartReturnsNoContent()
    {
        _orderServiceMock.Setup(s => s.RemoveGameFromCartAsync("test-game")).Returns(Task.CompletedTask);

        var result = await _controller.DeleteGameFromCart("test-game");

        Assert.IsType<NoContentResult>(result);
        _orderServiceMock.Verify(s => s.RemoveGameFromCartAsync("test-game"), Times.Once);
    }

    [Fact]
    public async Task GetOrdersReturnsOkWithOrders()
    {
        var orders = new List<OrderResponse>
        {
            new() { Id = Guid.NewGuid(), CustomerId = Guid.NewGuid(), Date = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), CustomerId = Guid.NewGuid(), Date = DateTime.UtcNow.AddDays(-1) },
        };

        _orderServiceMock.Setup(s => s.GetOrdersAsync()).ReturnsAsync(orders);

        var result = await _controller.GetOrders();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(orders, okResult.Value);
    }

    [Fact]
    public async Task GetOrderByIdReturnsOkWithOrder()
    {
        var id = Guid.NewGuid();
        var order = new OrderResponse { Id = id, CustomerId = Guid.NewGuid(), Date = DateTime.UtcNow };
        _orderServiceMock.Setup(s => s.GetOrderByIdAsync(id)).ReturnsAsync(order);

        var result = await _controller.GetOrderById(id);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(order, okResult.Value);
    }

    [Fact]
    public async Task GetOrderDetailsReturnsOkWithDetails()
    {
        var id = Guid.NewGuid();
        var details = new List<OrderGameResponse>
        {
            new() { ProductId = Guid.NewGuid(), Price = 10, Quantity = 2, Discount = 0 },
            new() { ProductId = Guid.NewGuid(), Price = 20, Quantity = 1, Discount = 5 },
        };

        _orderServiceMock.Setup(s => s.GetOrderDetailsAsync(id)).ReturnsAsync(details);

        var result = await _controller.GetOrderDetails(id);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(details, okResult.Value);
    }

    [Fact]
    public async Task GetCartReturnsOkWithCartItems()
    {
        var cart = new List<OrderGameResponse>
        {
            new() { ProductId = Guid.NewGuid(), Price = 30, Quantity = 1, Discount = 0 },
        };

        _orderServiceMock.Setup(s => s.GetCartAsync()).ReturnsAsync(cart);

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
                new PaymentMethodResponse
                {
                    ImageUrl = "https://example.com/bank.png",
                    Title = "Bank",
                    Description = "Pay by invoice",
                },
                new PaymentMethodResponse
                {
                    ImageUrl = "https://example.com/ibox.png",
                    Title = "IBox terminal",
                    Description = "Pay at terminal",
                },
                new PaymentMethodResponse
                {
                    ImageUrl = "https://example.com/visa.png",
                    Title = "Visa",
                    Description = "Pay with card",
                },
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

        _orderServiceMock.Setup(s => s.PayByBankAsync()).ReturnsAsync(invoice);

        var result = await _controller.Pay(new PaymentRequest { Method = "Bank" });

        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", fileResult.ContentType);
        Assert.Equal("invoice.pdf", fileResult.FileDownloadName);
        Assert.Equal(invoice.Content, fileResult.FileContents);
    }

    [Fact]
    public async Task PayIBoxReturnsOkWithResponse()
    {
        var response = new IBoxPaymentResponse
        {
            UserId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            PaymentDate = DateTime.UtcNow,
            Sum = 100,
        };

        _orderServiceMock.Setup(s => s.PayByIBoxAsync()).ReturnsAsync(response);

        var result = await _controller.Pay(new PaymentRequest { Method = "IBox terminal" });

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(response, okResult.Value);
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

        _orderServiceMock.Setup(s => s.PayByVisaAsync(model)).Returns(Task.CompletedTask);

        var result = await _controller.Pay(new PaymentRequest
        {
            Method = "Visa",
            Model = model,
        });

        Assert.IsType<NoContentResult>(result);
        _orderServiceMock.Verify(s => s.PayByVisaAsync(model), Times.Once);
    }

    [Fact]
    public async Task PayVisaThrowsWhenModelIsMissing()
    {
        var request = new PaymentRequest { Method = "Visa" };

        await Assert.ThrowsAsync<ArgumentException>(() => _controller.Pay(request));
    }

    [Fact]
    public async Task PayThrowsWhenMethodIsUnsupported()
    {
        var request = new PaymentRequest { Method = "Cash" };

        await Assert.ThrowsAsync<ArgumentException>(() => _controller.Pay(request));
    }
}
