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
        var durations = new List<BanDurationType> { BanDurationType.OneHour, BanDurationType.OneDay, BanDurationType.Permanent };
        _commentServiceMock.Setup(x => x.GetBanDurations()).Returns(durations);

        var result = _controller.GetBanDurations();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(durations, okResult.Value);
    }

    [Fact]
    public async Task SearchUsersReturnsOkWithUsers()
    {
        var users = new List<UserLookupResponse>
        {
            new() { Id = Guid.NewGuid(), Name = "John" },
        };

        _commentServiceMock.Setup(x => x.SearchUsersAsync("jo", 20)).ReturnsAsync(users);

        var result = await _controller.SearchUsers("jo", 20);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(users, okResult.Value);
    }

    [Fact]
    public async Task BanUserReturnsNoContent()
    {
        var request = new BanUserRequest { UserId = Guid.NewGuid(), Duration = BanDurationType.OneWeek };
        _commentServiceMock.Setup(x => x.BanUserAsync(request)).Returns(Task.CompletedTask);

        var result = await _controller.BanUser(request);

        Assert.IsType<NoContentResult>(result);
        _commentServiceMock.Verify(x => x.BanUserAsync(request), Times.Once);
    }
}
