using Gamestore.Api.Auth;
using Gamestore.BLL.DTOs.Comment;
using Gamestore.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.Api.Controllers;

[ApiController]
[Route("comments")]
public class CommentsController(ICommentService commentService) : ControllerBase
{
    private readonly ICommentService _commentService = commentService;

    [Authorize]
    [HttpGet("ban/durations")]
    public IActionResult GetBanDurations()
    {
        var durations = _commentService.GetBanDurations();
        return Ok(durations);
    }

    [Authorize(Policy = Permissions.BanUsers)]
    [HttpGet("users/search")]
    public async Task<IActionResult> SearchUsers([FromQuery] string query = "", [FromQuery] int take = 20)
    {
        var users = await _commentService.SearchUserNamesAsync(query, take);
        return Ok(users);
    }

    [Authorize(Policy = Permissions.BanUsers)]
    [HttpPost("ban")]
    public async Task<IActionResult> BanUser([FromBody] BanUserRequest request)
    {
        await _commentService.BanUserAsync(request);
        return NoContent();
    }
}
