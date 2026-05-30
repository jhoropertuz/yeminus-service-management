using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yeminus.Api.Common;
using Yeminus.Application.DTOs.Technicians;
using Yeminus.Application.Services.Interfaces;

namespace Yeminus.Api.Controllers;

[ApiController]
[Route("api/technicians")]
[Authorize]
public class TechniciansController(ITechnicianService technicianService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<TechnicianResponse>>>> GetAll()
    {
        var technicians = await technicianService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<TechnicianResponse>>.Ok(technicians));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TechnicianResponse>>> GetById(Guid id)
    {
        var technician = await technicianService.GetByIdAsync(id);
        return Ok(ApiResponse<TechnicianResponse>.Ok(technician));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TechnicianResponse>>> Create([FromBody] CreateTechnicianRequest request)
    {
        var technician = await technicianService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = technician.Id },
            ApiResponse<TechnicianResponse>.Ok(technician, "Technician created successfully."));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TechnicianResponse>>> Update(Guid id, [FromBody] UpdateTechnicianRequest request)
    {
        var technician = await technicianService.UpdateAsync(id, request);
        return Ok(ApiResponse<TechnicianResponse>.Ok(technician, "Technician updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        await technicianService.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Technician deleted successfully."));
    }
}
