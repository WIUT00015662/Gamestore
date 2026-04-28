using Gamestore.BLL.DTOs.Comment;
using Gamestore.Domain.Entities;
using Gamestore.Domain.Exceptions;
using Gamestore.Domain.Repositories;

namespace Gamestore.BLL.Services;

public class CommentService(IUnitOfWork unitOfWork) : ICommentService
{
    private static readonly Dictionary<BanDurationType, TimeSpan?> BanDurations = new()
    {
        [BanDurationType.OneHour] = TimeSpan.FromHours(1),
        [BanDurationType.OneDay] = TimeSpan.FromDays(1),
        [BanDurationType.OneWeek] = TimeSpan.FromDays(7),
        [BanDurationType.OneMonth] = TimeSpan.FromDays(30),
        [BanDurationType.Permanent] = null,
    };

    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task AddCommentAsync(string gameKey, AddCommentRequest request, Guid actorUserId, string actorName)
    {
        var game = await _unitOfWork.Games.GetByKeyAsync(gameKey)
            ?? throw new EntityNotFoundException(nameof(Game), gameKey);

        if (await IsUserBannedAsync(actorUserId))
        {
            throw new ArgumentException("User is banned from commenting.");
        }

        if (request.Action != CommentActionType.None && request.ParentId is null)
        {
            throw new ArgumentException("Action requires parent comment.");
        }

        var parent = request.ParentId.HasValue
            ? await _unitOfWork.Comments.GetByIdWithDetailsAsync(request.ParentId.Value)
            : null;

        if (parent is not null && parent.GameId != game.Id)
        {
            throw new ArgumentException("Parent comment does not belong to the specified game.");
        }

        var quotedCommentId = request.Action == CommentActionType.Quote ? request.ParentId : null;

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            AuthorUserId = actorUserId,
            Name = actorName,
            Body = request.Comment.Body,
            ParentCommentId = request.ParentId,
            QuotedCommentId = quotedCommentId,
            GameId = game.Id,
            IsDeleted = false,
        };

        await _unitOfWork.Comments.AddAsync(comment);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<CommentResponse>> GetCommentsByGameKeyAsync(string gameKey)
    {
        var game = await _unitOfWork.Games.GetByKeyAsync(gameKey)
            ?? throw new EntityNotFoundException(nameof(Game), gameKey);

        List<Comment> comments = [.. await _unitOfWork.Comments.GetByGameIdAsync(game.Id)];

        var grouped = comments.ToLookup(c => c.ParentCommentId);

        List<CommentResponse> BuildTree(Guid? parentId)
        {
            return
            [
                .. grouped[parentId]
                    .Select(comment => new CommentResponse
                    {
                        Id = comment.Id,
                        AuthorUserId = comment.AuthorUserId,
                        Name = comment.Name,
                        Body = comment.Body,
                        IsDeleted = comment.IsDeleted,
                        ChildComments = BuildTree(comment.Id),
                    }),
            ];
        }

        return BuildTree(null);
    }

    public async Task UpdateCommentAsync(string gameKey, Guid commentId, UpdateCommentRequest request, Guid actorUserId, string actorName)
    {
        var game = await _unitOfWork.Games.GetByKeyAsync(gameKey)
            ?? throw new EntityNotFoundException(nameof(Game), gameKey);

        var comment = await _unitOfWork.Comments.GetByIdWithDetailsAsync(commentId)
            ?? throw new EntityNotFoundException(nameof(Comment), commentId);

        if (comment.GameId != game.Id)
        {
            throw new ArgumentException("Comment does not belong to the specified game.");
        }

        var isOwner = comment.AuthorUserId.HasValue
            ? comment.AuthorUserId.Value == actorUserId
            : comment.Name.Equals(actorName, StringComparison.OrdinalIgnoreCase);

        if (!isOwner)
        {
            throw new ArgumentException("Only comment owner can edit comment.");
        }

        if (comment.IsDeleted)
        {
            throw new ArgumentException("Deleted comments cannot be edited.");
        }

        comment.Body = request.Comment.Body;
        _unitOfWork.Comments.Update(comment);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteCommentAsync(string gameKey, Guid commentId, Guid actorUserId, string actorName, bool canManageComments)
    {
        var game = await _unitOfWork.Games.GetByKeyAsync(gameKey)
            ?? throw new EntityNotFoundException(nameof(Game), gameKey);

        var comment = await _unitOfWork.Comments.GetByIdWithDetailsAsync(commentId)
            ?? throw new EntityNotFoundException(nameof(Comment), commentId);

        if (comment.GameId != game.Id)
        {
            throw new ArgumentException("Comment does not belong to the specified game.");
        }

        var isOwner = comment.AuthorUserId.HasValue
            ? comment.AuthorUserId.Value == actorUserId
            : comment.Name.Equals(actorName, StringComparison.OrdinalIgnoreCase);

        if (!canManageComments && !isOwner)
        {
            throw new ArgumentException("Only comment owner or moderator can delete comment.");
        }

        if (comment.IsDeleted)
        {
            return;
        }

        comment.IsDeleted = true;
        _unitOfWork.Comments.Update(comment);
        await _unitOfWork.SaveChangesAsync();
    }

    public IEnumerable<BanDurationType> GetBanDurations()
    {
        return Enum.GetValues<BanDurationType>();
    }

    public async Task BanUserAsync(BanUserRequest request)
    {
        if (request.Duration is null)
        {
            throw new ArgumentException("Ban duration is required.");
        }

        if (!BanDurations.TryGetValue(request.Duration.Value, out var duration))
        {
            throw new ArgumentException($"Unsupported ban duration: {request.Duration.Value}");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId)
            ?? throw new EntityNotFoundException(nameof(User), request.UserId);

        var existingBan = await _unitOfWork.CommentBans.GetByUserIdAsync(request.UserId);
        var now = DateTime.UtcNow;

        if (existingBan is null)
        {
            existingBan = new CommentBan
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Name = user.Name,
            };

            await _unitOfWork.CommentBans.AddAsync(existingBan);
        }
        else
        {
            existingBan.Name = user.Name;
        }

        existingBan.IsPermanent = duration is null;
        existingBan.BannedUntil = duration is null ? null : now.Add(duration.Value);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<UserLookupResponse>> SearchUsersAsync(string query, int take)
    {
        var users = await _unitOfWork.Users.GetAllAsync();
        var normalized = query.Trim();
        var limit = Math.Clamp(take, 1, 50);

        return users
            .Where(user => string.IsNullOrWhiteSpace(normalized) || user.Name.Contains(normalized, StringComparison.OrdinalIgnoreCase))
            .OrderBy(user => user.Name)
            .Take(limit)
            .Select(user => new UserLookupResponse
            {
                Id = user.Id,
                Name = user.Name,
            })
            .ToList();
    }

    private async Task<bool> IsUserBannedAsync(Guid userId)
    {
        var ban = await _unitOfWork.CommentBans.GetByUserIdAsync(userId);
        return ban is not null
            && (ban.IsPermanent || (ban.BannedUntil is { } bannedUntil && bannedUntil > DateTime.UtcNow));
    }


}
