using Gamestore.BLL.DTOs.Comment;
using Gamestore.Domain.Entities;
using Gamestore.Domain.Exceptions;
using Gamestore.Domain.Repositories;

namespace Gamestore.BLL.Services;

public class CommentService(IUnitOfWork unitOfWork) : ICommentService
{
    private const string DeletedMessage = "A comment/quote was deleted";

    private static readonly IReadOnlyDictionary<BanDurationType, TimeSpan?> BanDurations = new Dictionary<BanDurationType, TimeSpan?>
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

        var action = request.Action?.Trim();
        string finalBody;
        Guid? quotedCommentId = null;

        if (request.ParentId is null)
        {
            if (!string.IsNullOrWhiteSpace(action))
            {
                throw new ArgumentException("Action requires parent comment.");
            }

            finalBody = request.Comment.Body;
        }
        else
        {
            var parent = await _unitOfWork.Comments.GetByIdWithDetailsAsync(request.ParentId.Value)
                ?? throw new EntityNotFoundException(nameof(Comment), request.ParentId.Value);

            if (parent.GameId != game.Id)
            {
                throw new ArgumentException("Parent comment does not belong to the specified game.");
            }

            finalBody = BuildCommentBody(parent, request.Comment.Body, action);
            if (string.Equals(action, "quote", StringComparison.OrdinalIgnoreCase))
            {
                quotedCommentId = parent.Id;
            }
        }

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            AuthorUserId = actorUserId,
            Name = actorName,
            Body = finalBody,
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
        var game = await _unitOfWork.Games.GetByKeyIncludingDeletedAsync(gameKey)
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
        var game = await _unitOfWork.Games.GetByKeyIncludingDeletedAsync(gameKey)
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

        var oldBody = comment.Body;
        comment.Body = request.Comment.Body;
        _unitOfWork.Comments.Update(comment);

        await RefreshQuotedCascadeAsync(comment.Id, oldBody, comment.Body);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteCommentAsync(string gameKey, Guid commentId, Guid actorUserId, string actorName, bool canManageComments)
    {
        var game = await _unitOfWork.Games.GetByKeyIncludingDeletedAsync(gameKey)
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

        var oldBody = comment.Body;
        comment.Body = DeletedMessage;
        comment.IsDeleted = true;
        _unitOfWork.Comments.Update(comment);

        await RefreshQuotedCascadeAsync(comment.Id, oldBody, comment.Body);
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

    private static string BuildCommentBody(Comment parent, string userBody, string? action)
    {
        return action?.Trim().ToLowerInvariant() switch
        {
            null or "" => userBody,
            "reply" => $"{parent.Name}, {userBody}",
            "quote" => $"{parent.Body}, {userBody}",
            _ => throw new ArgumentException($"Unsupported action: {action}"),
        };
    }

    private async Task RefreshQuotedCascadeAsync(Guid sourceCommentId, string oldPrefix, string newPrefix)
    {
        var quotedComments = (await _unitOfWork.Comments.GetQuotedByCommentIdAsync(sourceCommentId)).ToList();
        foreach (var quotedComment in quotedComments)
        {
            var oldQuotedBody = quotedComment.Body;
            var suffix = ExtractSuffix(quotedComment.Body, oldPrefix);
            quotedComment.Body = string.IsNullOrWhiteSpace(suffix)
                ? newPrefix
                : $"{newPrefix}, {suffix}";
            _unitOfWork.Comments.Update(quotedComment);

            await RefreshQuotedCascadeAsync(quotedComment.Id, oldQuotedBody, quotedComment.Body);
        }
    }

    private static string ExtractSuffix(string body, string expectedPrefix)
    {
        var prefixWithSeparator = $"{expectedPrefix}, ";
        if (body.StartsWith(prefixWithSeparator, StringComparison.Ordinal))
        {
            return body[prefixWithSeparator.Length..].Trim();
        }

        if (body.Equals(expectedPrefix, StringComparison.Ordinal))
        {
            return string.Empty;
        }

        var commaIndex = body.IndexOf(',');
        return commaIndex < 0 ? string.Empty : body[(commaIndex + 1)..].Trim();
    }
}
