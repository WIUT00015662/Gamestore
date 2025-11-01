using Gamestore.Api.Controllers;
using Gamestore.BLL.DTOs.Comment;
using Gamestore.BLL.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GameStore.UnitTests.Api.Controllers;

public class CommentsControllerTests
{
    private readonly Mock<ICommentService> _commentServiceMock;
    private readonly CommentsController _controller;

    public CommentsControllerTests()
    {
        _commentServiceMock = new Mock<ICommentService>();
        _controller = new CommentsController(_commentServiceMock.Object);
    }

    [Fact]
    public void GetBanDurationsReturnsOkWithDurations()
    {
        var durations = new List<string> { "1 hour", "1 day", "permanent" };
        _commentServiceMock.Setup(x => x.GetBanDurations()).Returns(durations);

        var result = _controller.GetBanDurations();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(durations, okResult.Value);
    }

    [Fact]
    public async Task BanUserReturnsNoContent()
    {
        var request = new BanUserRequest { User = "John", Duration = "1 week" };
        _commentServiceMock.Setup(x => x.BanUserAsync(request)).Returns(Task.CompletedTask);

        var result = await _controller.BanUser(request);

        Assert.IsType<NoContentResult>(result);
        _commentServiceMock.Verify(x => x.BanUserAsync(request), Times.Once);
    }
}
