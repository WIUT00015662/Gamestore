using Gamestore.BLL.DTOs.Comment;
using Gamestore.BLL.Services;
using Gamestore.Domain.Entities;
using Gamestore.Domain.Repositories;
using Moq;

namespace GameStore.UnitTests.BLL.Services;

public class CommentServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IGameRepository> _gameRepoMock;
    private readonly Mock<ICommentRepository> _commentRepoMock;
    private readonly Mock<ICommentBanRepository> _commentBanRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly CommentService _commentService;

    public CommentServiceTests()
    {
        _gameRepoMock = new Mock<IGameRepository>();
        _commentRepoMock = new Mock<ICommentRepository>();
        _commentBanRepoMock = new Mock<ICommentBanRepository>();
        _userRepoMock = new Mock<IUserRepository>();

        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock.Setup(x => x.Games).Returns(_gameRepoMock.Object);
        _unitOfWorkMock.Setup(x => x.Comments).Returns(_commentRepoMock.Object);
        _unitOfWorkMock.Setup(x => x.CommentBans).Returns(_commentBanRepoMock.Object);
        _unitOfWorkMock.Setup(x => x.Users).Returns(_userRepoMock.Object);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        _commentRepoMock.Setup(x => x.AddAsync(It.IsAny<Comment>())).Returns(Task.CompletedTask);
        _commentBanRepoMock.Setup(x => x.AddAsync(It.IsAny<CommentBan>())).Returns(Task.CompletedTask);

        _commentService = new CommentService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task AddCommentAsyncThrowsWhenUserIsBanned()
    {
        var actorUserId = Guid.NewGuid();
        var game = new Game { Id = Guid.NewGuid(), Name = "Game", Key = "game" };
        var request = new AddCommentRequest
        {
            Comment = new CommentBodyDto { Name = "John", Body = "Hello" },
        };

        _gameRepoMock.Setup(x => x.GetByKeyAsync("game")).ReturnsAsync(game);
        _commentBanRepoMock.Setup(x => x.GetByUserIdAsync(actorUserId)).ReturnsAsync(new CommentBan
        {
            Id = Guid.NewGuid(),
            UserId = actorUserId,
            Name = "John",
            IsPermanent = true,
        });

        await Assert.ThrowsAsync<ArgumentException>(() => _commentService.AddCommentAsync("game", request, actorUserId, "John"));
    }

    [Fact]
    public async Task AddCommentAsyncFormatsReplyBody()
    {
        var actorUserId = Guid.NewGuid();
        var game = new Game { Id = Guid.NewGuid(), Name = "Game", Key = "game" };
        var parentComment = new Comment
        {
            Id = Guid.NewGuid(),
            Name = "ParentUser",
            Body = "Parent body",
            GameId = game.Id,
        };

        var request = new AddCommentRequest
        {
            Comment = new CommentBodyDto { Name = "John", Body = "Reply text" },
            ParentId = parentComment.Id,
            Action = "reply",
        };

        _gameRepoMock.Setup(x => x.GetByKeyAsync("game")).ReturnsAsync(game);
        _commentBanRepoMock.Setup(x => x.GetByUserIdAsync(actorUserId)).ReturnsAsync((CommentBan?)null);
        _commentRepoMock.Setup(x => x.GetByIdWithDetailsAsync(parentComment.Id)).ReturnsAsync(parentComment);

        Comment? addedComment = null;
        _commentRepoMock
            .Setup(x => x.AddAsync(It.IsAny<Comment>()))
            .Callback<Comment>(c => addedComment = c)
            .Returns(Task.CompletedTask);

        await _commentService.AddCommentAsync("game", request, actorUserId, "John");

        Assert.NotNull(addedComment);
        Assert.Equal(actorUserId, addedComment.AuthorUserId);
        Assert.Equal("John", addedComment.Name);
        Assert.Equal("ParentUser, Reply text", addedComment.Body);
        Assert.Equal(parentComment.Id, addedComment.ParentCommentId);
        Assert.Null(addedComment.QuotedCommentId);
    }

    [Fact]
    public async Task GetCommentsByGameKeyAsyncReturnsHierarchy()
    {
        var game = new Game { Id = Guid.NewGuid(), Name = "Game", Key = "game" };
        var rootId = Guid.NewGuid();

        _gameRepoMock.Setup(x => x.GetByKeyIncludingDeletedAsync("game")).ReturnsAsync(game);
        _commentRepoMock
            .Setup(x => x.GetByGameIdAsync(game.Id))
            .ReturnsAsync([
                new Comment { Id = rootId, Name = "A", Body = "Root", GameId = game.Id },
                new Comment { Id = Guid.NewGuid(), Name = "B", Body = "Child", GameId = game.Id, ParentCommentId = rootId },
            ]);

        var result = (await _commentService.GetCommentsByGameKeyAsync("game")).ToList();

        Assert.Single(result);
        Assert.Single(result[0].ChildComments);
        Assert.Equal("Root", result[0].Body);
        Assert.Equal("Child", result[0].ChildComments[0].Body);
    }

    [Fact]
    public async Task DeleteCommentAsyncMarksDeletedAndUpdatesQuotedComments()
    {
        var game = new Game { Id = Guid.NewGuid(), Name = "Game", Key = "game" };
        var commentId = Guid.NewGuid();
        var comment = new Comment { Id = commentId, Name = "A", Body = "Original", GameId = game.Id };
        var quotingComment = new Comment
        {
            Id = Guid.NewGuid(),
            Name = "B",
            Body = "Original, my quote text",
            GameId = game.Id,
            QuotedCommentId = commentId,
        };

        _gameRepoMock.Setup(x => x.GetByKeyIncludingDeletedAsync("game")).ReturnsAsync(game);
        _commentRepoMock.Setup(x => x.GetByIdWithDetailsAsync(commentId)).ReturnsAsync(comment);
        _commentRepoMock.Setup(x => x.GetQuotedByCommentIdAsync(commentId)).ReturnsAsync([quotingComment]);

        await _commentService.DeleteCommentAsync("game", commentId, Guid.NewGuid(), "A", false);

        Assert.Equal("A comment/quote was deleted", comment.Body);
        Assert.Equal("A comment/quote was deleted, my quote text", quotingComment.Body);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public void GetBanDurationsReturnsExpectedValues()
    {
        var result = _commentService.GetBanDurations().ToList();

        Assert.Contains(BanDurationType.OneHour, result);
        Assert.Contains(BanDurationType.OneDay, result);
        Assert.Contains(BanDurationType.OneWeek, result);
        Assert.Contains(BanDurationType.OneMonth, result);
        Assert.Contains(BanDurationType.Permanent, result);
    }

    [Fact]
    public async Task BanUserAsyncCreatesPermanentBan()
    {
        var userId = Guid.NewGuid();
        _commentBanRepoMock.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync((CommentBan?)null);
        _userRepoMock.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(new User
        {
            Id = userId,
            Name = "John",
            PasswordHash = "hash",
        });

        CommentBan? addedBan = null;
        _commentBanRepoMock
            .Setup(x => x.AddAsync(It.IsAny<CommentBan>()))
            .Callback<CommentBan>(b => addedBan = b)
            .Returns(Task.CompletedTask);

        await _commentService.BanUserAsync(
            new BanUserRequest
            {
                UserId = userId,
                Duration = BanDurationType.Permanent,
            });

        Assert.NotNull(addedBan);
        Assert.Equal(userId, addedBan.UserId);
        Assert.Equal("John", addedBan.Name);
        Assert.True(addedBan.IsPermanent);
        Assert.Null(addedBan.BannedUntil);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}
