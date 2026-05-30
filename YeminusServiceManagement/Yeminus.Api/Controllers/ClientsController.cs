using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yeminus.Api.Common;
using Yeminus.Application.DTOs.Clients;
using Yeminus.Application.Services.Interfaces;

namespace Yeminus.Api.Controllers;

[ApiController]
[Route("api/clients")]
[Authorize]
public class ClientsController(IClientService clientService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ClientResponse>>>> GetAll()
    {
        var clients = await clientService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<ClientResponse>>.Ok(clients));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ClientResponse>>> GetById(Guid id)
    {
        var client = await clientService.GetByIdAsync(id);
        return Ok(ApiResponse<ClientResponse>.Ok(client));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ClientResponse>>> Create([FromBody] CreateClientRequest request)
    {
        var client = await clientService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = client.Id },
            ApiResponse<ClientResponse>.Ok(client, "Client created successfully."));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ClientResponse>>> Update(Guid id, [FromBody] UpdateClientRequest request)
    {
        var client = await clientService.UpdateAsync(id, request);
        return Ok(ApiResponse<ClientResponse>.Ok(client, "Client updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        await clientService.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Client deleted successfully."));
    }
}
