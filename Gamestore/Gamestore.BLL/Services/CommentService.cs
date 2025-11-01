using Gamestore.BLL.DTOs.Comment;
using Gamestore.Domain.Entities;
using Gamestore.Domain.Exceptions;
using Gamestore.Domain.Repositories;

namespace Gamestore.BLL.Services;

public class CommentService(IUnitOfWork unitOfWork) : ICommentService
{
    private const string DeletedMessage = "A comment/quote was deleted";

    private static readonly IReadOnlyDictionary<string, TimeSpan?> BanDurations = new Dictionary<string, TimeSpan?>(StringComparer.OrdinalIgnoreCase)
    {
        ["1 hour"] = TimeSpan.FromHours(1),
        ["1 day"] = TimeSpan.FromDays(1),
        ["1 week"] = TimeSpan.FromDays(7),
        ["1 month"] = TimeSpan.FromDays(30),
        ["permanent"] = null,
    };

    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task AddCommentAsync(string gameKey, AddCommentRequest request)
    {
        var game = await _unitOfWork.Games.GetByKeyAsync(gameKey)
            ?? throw new EntityNotFoundException(nameof(Game), gameKey);

        if (await IsUserBannedAsync(request.Comment.Name))
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
            Name = request.Comment.Name,
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
                        Name = comment.Name,
                        Body = comment.Body,
                        ChildComments = BuildTree(comment.Id),
                    }),
            ];
        }

        return BuildTree(null);
    }

    public async Task DeleteCommentAsync(string gameKey, Guid commentId)
    {
        var game = await _unitOfWork.Games.GetByKeyIncludingDeletedAsync(gameKey)
            ?? throw new EntityNotFoundException(nameof(Game), gameKey);

        var comment = await _unitOfWork.Comments.GetByIdWithDetailsAsync(commentId)
            ?? throw new EntityNotFoundException(nameof(Comment), commentId);

        if (comment.GameId != game.Id)
        {
            throw new ArgumentException("Comment does not belong to the specified game.");
        }

        comment.Body = DeletedMessage;
        comment.IsDeleted = true;
        _unitOfWork.Comments.Update(comment);

        var quotedComments = await _unitOfWork.Comments.GetQuotedByCommentIdAsync(commentId);
        foreach (var quotedComment in quotedComments)
        {
            quotedComment.Body = ReplaceQuotedPrefix(quotedComment.Body);
            _unitOfWork.Comments.Update(quotedComment);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public IEnumerable<string> GetBanDurations()
    {
        return BanDurations.Keys;
    }

    public async Task BanUserAsync(BanUserRequest request)
    {
        if (!BanDurations.TryGetValue(request.Duration.Trim(), out var duration))
        {
            throw new ArgumentException($"Unsupported ban duration: {request.Duration}");
        }

        var existingBan = await _unitOfWork.CommentBans.GetByNameAsync(request.User);
        var now = DateTime.UtcNow;

        if (existingBan is null)
        {
            existingBan = new CommentBan
            {
                Id = Guid.NewGuid(),
                Name = request.User,
            };

            await _unitOfWork.CommentBans.AddAsync(existingBan);
        }

        existingBan.IsPermanent = duration is null;
        existingBan.BannedUntil = duration is null ? null : now.Add(duration.Value);

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<bool> IsUserBannedAsync(string name)
    {
        var ban = await _unitOfWork.CommentBans.GetByNameAsync(name);
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

    private static string ReplaceQuotedPrefix(string body)
    {
        var commaIndex = body.IndexOf(',');
        var suffix = commaIndex < 0 ? string.Empty : body[(commaIndex + 1)..].TrimStart();
        return string.IsNullOrWhiteSpace(suffix)
            ? DeletedMessage
            : $"{DeletedMessage}, {suffix}";
    }
}
