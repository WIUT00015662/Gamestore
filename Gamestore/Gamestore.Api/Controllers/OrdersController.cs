using Gamestore.Api.Auth;
using Gamestore.BLL.DTOs.Order;
using Gamestore.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.Api.Controllers;

[ApiController]
[Route("orders")]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    private readonly IOrderService _orderService = orderService;

    [Authorize(Policy = Permissions.BuyGame)]
    [HttpDelete("cart/{key}")]
    public async Task<IActionResult> DeleteGameFromCart(string key)
    {
        await _orderService.RemoveGameFromCartAsync(key);
        return NoContent();
    }

    [Authorize(Policy = Permissions.ViewOrderHistory)]
    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var orders = await _orderService.GetOrdersAsync();
        return Ok(orders);
    }

    [Authorize(Policy = Permissions.ViewOrderHistory)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        return Ok(order);
    }

    [Authorize(Policy = Permissions.ManageOrders)]
    [HttpGet("{id:guid}/details")]
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
    [HttpPost("{id:guid}/ship")]
    public async Task<IActionResult> ShipOrder(Guid id)
    {
        await _orderService.ShipOrderAsync(id);
        return NoContent();
    }

    [Authorize(Policy = Permissions.ManageOrders)]
    [HttpPost("{id:guid}/details/{key}")]
    public async Task<IActionResult> AddGameToOrder(Guid id, string key)
    {
        await _orderService.AddGameToOrderAsync(id, key);
        return NoContent();
    }

    [Authorize(Policy = Permissions.BuyGame)]
    [HttpGet("cart")]
    public async Task<IActionResult> GetCart()
    {
        var cart = await _orderService.GetCartAsync();
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
        if (request.Method.Equals("Bank", StringComparison.OrdinalIgnoreCase))
        {
            var invoice = await _orderService.PayByBankAsync();
            return File(invoice.Content, "application/pdf", invoice.FileName);
        }

        if (request.Method.Equals("IBox terminal", StringComparison.OrdinalIgnoreCase))
        {
            var response = await _orderService.PayByIBoxAsync();
            return Ok(response);
        }

        if (request.Method.Equals("Visa", StringComparison.OrdinalIgnoreCase))
        {
            if (request.Model is null)
            {
                throw new ArgumentException("Visa model is required.");
            }

            await _orderService.PayByVisaAsync(request.Model);
            return NoContent();
        }

        throw new ArgumentException($"Unsupported payment method: {request.Method}");
    }
}
