using Gamestore.Api.Auth;
using Gamestore.Api.Auth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.Api.Controllers;

[ApiController]
[Route("users")]
public class UsersController(IAuthManagementService authManagementService) : ControllerBase
{
    private readonly IAuthManagementService _authManagementService = authManagementService;

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var token = await _authManagementService.LoginAsync(request);
        return Ok(token);
    }

    [Authorize]
    [HttpPost("access")]
    public async Task<IActionResult> CheckAccess([FromBody] AccessRequest request)
    {
        var hasAccess = await _authManagementService.CheckAccessAsync(User, request);
        return Ok(hasAccess);
    }

    [Authorize(Policy = Permissions.ManageUsers)]
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _authManagementService.GetUsersAsync();
        return Ok(users);
    }

    [Authorize(Policy = Permissions.ManageUsers)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await _authManagementService.GetUserByIdAsync(id);
        return Ok(user);
    }

    [Authorize(Policy = Permissions.ManageUsers)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        await _authManagementService.DeleteUserAsync(id);
        return NoContent();
    }

    [Authorize(Policy = Permissions.ManageUsers)]
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateOrUpdateUserRequest request)
    {
        var created = await _authManagementService.CreateUserAsync(request);
        return CreatedAtAction(nameof(GetUserById), new { id = created.Id }, created);
    }

    [Authorize(Policy = Permissions.ManageUsers)]
    [HttpPut]
    public async Task<IActionResult> UpdateUser([FromBody] CreateOrUpdateUserRequest request)
    {
        await _authManagementService.UpdateUserAsync(request);
        return NoContent();
    }

    [Authorize(Policy = Permissions.ManageUsers)]
    [HttpGet("{id:guid}/roles")]
    public async Task<IActionResult> GetUserRoles(Guid id)
    {
        var roles = await _authManagementService.GetUserRolesAsync(id);
        return Ok(roles);
    }
}
