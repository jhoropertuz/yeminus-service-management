using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yeminus.Api.Common;
using Yeminus.Application.DTOs.Orders;
using Yeminus.Application.Services.Interfaces;
using Yeminus.Domain.Enums;

namespace Yeminus.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController(IServiceOrderService orderService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<OrderResponse>>>> GetAll(
        [FromQuery] OrderStatus? status = null,
        [FromQuery] string? technicianName = null,
        [FromQuery] string? specialty = null,
        [FromQuery] string? clientName = null,
        [FromQuery] string? clientDocument = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null)
    {
        var filter = new OrderFilter
        {
            Status = status,
            TechnicianName = technicianName,
            Specialty = specialty,
            ClientName = clientName,
            ClientDocument = clientDocument,
            DateFrom = dateFrom,
            DateTo = dateTo
        };

        var hasFilter = status.HasValue || !string.IsNullOrEmpty(technicianName)
            || !string.IsNullOrEmpty(specialty) || !string.IsNullOrEmpty(clientName)
            || !string.IsNullOrEmpty(clientDocument) || dateFrom.HasValue || dateTo.HasValue;

        var orders = await orderService.GetAllAsync(hasFilter ? filter : null);
        return Ok(ApiResponse<IEnumerable<OrderResponse>>.Ok(orders));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<OrderResponse>>> GetById(Guid id)
    {
        var order = await orderService.GetByIdAsync(id);
        return Ok(ApiResponse<OrderResponse>.Ok(order));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<OrderResponse>>> Create([FromBody] CreateOrderRequest request)
    {
        var userId = GetCurrentUserId();
        var order = await orderService.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { id = order.Id },
            ApiResponse<OrderResponse>.Ok(order, "Service order created successfully."));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<OrderResponse>>> Update(Guid id, [FromBody] UpdateOrderRequest request)
    {
        var userId = GetCurrentUserId();
        var order = await orderService.UpdateAsync(id, request, userId);
        return Ok(ApiResponse<OrderResponse>.Ok(order, "Service order updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        await orderService.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Service order deleted successfully."));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<ApiResponse<OrderResponse>>> ChangeStatus(
        Guid id, [FromBody] ChangeOrderStatusRequest request)
    {
        var userId = GetCurrentUserId();
        var order = await orderService.ChangeStatusAsync(id, request, userId);
        return Ok(ApiResponse<OrderResponse>.Ok(order, "Order status updated successfully."));
    }

    private Guid GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new InvalidOperationException("User ID not found in token.");
        return Guid.Parse(value);
    }
}
