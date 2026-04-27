using Gamestore.Api.Middleware;
using Gamestore.BLL.Services;
using Microsoft.AspNetCore.Http;
using Moq;

namespace GameStore.UnitTests.Api.Middleware;

/*
public class GameCountMiddlewareTests
{
    [Fact]
    public async Task InvokeAsyncAddsTotalGamesToResponseHeaders()
    {
        // Arrange
        var nextDelegateMock = new Mock<RequestDelegate>();
        var gameServiceMock = new Mock<IGameService>();

        var contextMock = new Mock<HttpContext>();
        var responseMock = new Mock<HttpResponse>();
        var headers = new HeaderDictionary();

        contextMock.Setup(c => c.Response).Returns(responseMock.Object);
        responseMock.SetupGet(r => r.Headers).Returns(headers);

        Func<Task>? onStartingCallback = null;
        responseMock.Setup(r => r.OnStarting(It.IsAny<Func<Task>>()))
            .Callback<Func<Task>>(callback => onStartingCallback = callback);

        int expectedGameCount = 42;
        gameServiceMock.Setup(service => service.GetGamesCountAsync())
            .ReturnsAsync(expectedGameCount);

        var middleware = new GameCountMiddleware(nextDelegateMock.Object);

        // Act
        await middleware.InvokeAsync(contextMock.Object, gameServiceMock.Object);

        // Simulate Response Starting which executes the callback
        if (onStartingCallback != null)
        {
            await onStartingCallback();
        }

        // Assert
        Assert.True(headers.ContainsKey("x-total-numbers-of-games"));
        Assert.Equal(expectedGameCount.ToString(), headers["x-total-numbers-of-games"].ToString());

        nextDelegateMock.Verify(next => next(contextMock.Object), Times.Once);
        gameServiceMock.Verify(service => service.GetGamesCountAsync(), Times.Once);
    }
}
*/
