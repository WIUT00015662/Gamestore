using Gamestore.Api.Auth;
using Gamestore.Api.Services;
using Gamestore.BLL.DTOs.Order;
using Gamestore.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.Api.Controllers;

[ApiController]
[Route("orders")]
public class OrdersController(IOrderService orderService, ICurrentUserService currentUserService) : ControllerBase
{
    private readonly IOrderService _orderService = orderService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    [Authorize(Policy = Permissions.BuyGame)]
    [HttpDelete("cart/{key}")]
    public async Task<IActionResult> DeleteGameFromCart(string key)
    {
        var userId = _currentUserService.GetUserId();
        await _orderService.RemoveGameFromCartAsync(key, userId);
        return NoContent();
    }

    [Authorize(Policy = Permissions.ViewOrderHistory)]
    [HttpGet("my-orders")]
    public async Task<IActionResult> GetMyOrders()
    {
        var userId = _currentUserService.GetUserId();
        var orders = await _orderService.GetMyOrdersAsync(userId);
        return Ok(orders);
    }

    [Authorize(Policy = Permissions.ViewOrderHistory)]
    [HttpGet("my-orders/{id:guid}")]
    public async Task<IActionResult> GetMyOrder(Guid id)
    {
        var userId = _currentUserService.GetUserId();
        var order = await _orderService.GetMyOrderByIdAsync(id, userId);
        return Ok(order);
    }

    [Authorize(Policy = Permissions.ViewOrderHistory)]
    [HttpGet("my-orders/{id:guid}/details")]
    public async Task<IActionResult> GetMyOrderDetails(Guid id)
    {
        var userId = _currentUserService.GetUserId();
        var details = await _orderService.GetMyOrderDetailsAsync(id, userId);
        return Ok(details);
    }

    [Authorize(Policy = Permissions.ManageOrders)]
    [HttpGet("all-orders")]
    public async Task<IActionResult> GetAllOrders()
    {
        var orders = await _orderService.GetAllOrdersAsync();
        return Ok(orders);
    }

    [Authorize(Policy = Permissions.ManageOrders)]
    [HttpGet("all-orders/{id:guid}")]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        return Ok(order);
    }

    [Authorize(Policy = Permissions.ManageOrders)]
    [HttpGet("all-orders/{id:guid}/details")]
    public async Task<IActionResult> GetOrderDetails(Guid id)
    {
        var details = await _orderService.GetOrderDetailsAsync(id);
        return Ok(details);
    }

    [Authorize(Policy = Permissions.BuyGame)]
    [HttpPatch("details/{id:guid}/quantity")]
    public async Task<IActionResult> UpdateOrderDetailQuantity(Guid id, [FromBody] UpdateOrderDetailQuantityRequest request)
    {
        await _orderService.UpdateOrderDetailQuantityAsync(id, request.Count);
        return NoContent();
    }

    [Authorize(Policy = Permissions.BuyGame)]
    [HttpDelete("details/{id:guid}")]
    public async Task<IActionResult> DeleteOrderDetail(Guid id)
    {
        await _orderService.DeleteOrderDetailAsync(id);
        return NoContent();
    }

    [Authorize(Policy = Permissions.ShipOrder)]
    [HttpPost("all-orders/{id:guid}/ship")]
    public async Task<IActionResult> ShipOrder(Guid id)
    {
        await _orderService.ShipOrderAsync(id);
        return NoContent();
    }

    [Authorize(Policy = Permissions.ManageOrders)]
    [HttpPost("all-orders/{id:guid}/details/{key}")]
    public async Task<IActionResult> AddGameToOrder(Guid id, string key)
    {
        await _orderService.AddGameToOrderAsync(id, key);
        return NoContent();
    }

    [Authorize(Policy = Permissions.BuyGame)]
    [HttpGet("cart")]
    public async Task<IActionResult> GetCart()
    {
        var userId = _currentUserService.GetUserId();
        var cart = await _orderService.GetCartAsync(userId);
        return Ok(cart);
    }

    [Authorize]
    [HttpGet("payment-methods")]
    public IActionResult GetPaymentMethods()
    {
        var methods = _orderService.GetPaymentMethods();
        return Ok(methods);
    }

    [Authorize(Policy = Permissions.BuyGame)]
    [HttpPost("payment")]
    public async Task<IActionResult> Pay([FromBody] PaymentRequest request)
    {
        var userId = _currentUserService.GetUserId();

        if (request.Method is PaymentMethodType.Bank)
        {
            var invoice = await _orderService.PayByBankAsync(userId);
            return File(invoice.Content, "application/pdf", invoice.FileName);
        }

        if (request.Method is PaymentMethodType.Visa)
        {
            await _orderService.PayByVisaAsync(request.Model!, userId);
            return NoContent();
        }

        throw new ArgumentException($"Unsupported payment method: {request.Method?.ToString() ?? "null"}");
    }
}
