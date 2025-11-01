using Gamestore.Api.Auth;
using Gamestore.Api.Auth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.Api.Controllers;

[ApiController]
[Route("roles")]
public class RolesController(IAuthManagementService authManagementService) : ControllerBase
{
    private readonly IAuthManagementService _authManagementService = authManagementService;

    [Authorize(Policy = Permissions.ManageRoles)]
    [HttpGet]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _authManagementService.GetRolesAsync();
        return Ok(roles);
    }

    [Authorize(Policy = Permissions.ManageRoles)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetRoleById(Guid id)
    {
        var role = await _authManagementService.GetRoleByIdAsync(id);
        return Ok(role);
    }

    [Authorize(Policy = Permissions.ManageRoles)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        await _authManagementService.DeleteRoleAsync(id);
        return NoContent();
    }

    [Authorize(Policy = Permissions.ManageRoles)]
    [HttpGet("permissions")]
    public IActionResult GetPermissions()
    {
        var permissions = _authManagementService.GetPermissions();
        return Ok(permissions);
    }

    [Authorize(Policy = Permissions.ManageRoles)]
    [HttpGet("{id:guid}/permissions")]
    public async Task<IActionResult> GetRolePermissions(Guid id)
    {
        var permissions = await _authManagementService.GetRolePermissionsAsync(id);
        return Ok(permissions);
    }

    [Authorize(Policy = Permissions.ManageRoles)]
    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] CreateOrUpdateRoleRequest request)
    {
        var created = await _authManagementService.CreateRoleAsync(request);
        return CreatedAtAction(nameof(GetRoleById), new { id = created.Id }, created);
    }

    [Authorize(Policy = Permissions.ManageRoles)]
    [HttpPut]
    public async Task<IActionResult> UpdateRole([FromBody] CreateOrUpdateRoleRequest request)
    {
        await _authManagementService.UpdateRoleAsync(request);
        return NoContent();
    }
}
