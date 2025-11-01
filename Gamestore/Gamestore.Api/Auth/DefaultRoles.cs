namespace Gamestore.Api.Auth;

public static class DefaultRoles
{
    public const string Administrator = "Administrator";
    public const string Manager = "Manager";
    public const string Moderator = "Moderator";
    public const string User = "User";
    public const string Guest = "Guest";

    public static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> PermissionsByRole =
        new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [Guest] =
            [
                Permissions.ViewGame,
            ],
            [User] =
            [
                Permissions.ViewGame,
                Permissions.BuyGame,
                Permissions.CommentGame,
            ],
            [Moderator] =
            [
                Permissions.ViewGame,
                Permissions.BuyGame,
                Permissions.CommentGame,
                Permissions.ManageComments,
                Permissions.BanUsers,
            ],
            [Manager] =
            [
                Permissions.ViewGame,
                Permissions.BuyGame,
                Permissions.CommentGame,
                Permissions.ManageComments,
                Permissions.BanUsers,
                Permissions.AddGame,
                Permissions.UpdateGame,
                Permissions.DeleteGame,
                Permissions.ManageEntities,
                Permissions.ManageOrders,
                Permissions.ViewOrderHistory,
                Permissions.ShipOrder,
            ],
            [Administrator] =
            [
                Permissions.ViewGame,
                Permissions.BuyGame,
                Permissions.CommentGame,
                Permissions.ManageComments,
                Permissions.BanUsers,
                Permissions.AddGame,
                Permissions.UpdateGame,
                Permissions.DeleteGame,
                Permissions.ManageEntities,
                Permissions.ManageOrders,
                Permissions.ViewOrderHistory,
                Permissions.ShipOrder,
                Permissions.ManageUsers,
                Permissions.ManageRoles,
                Permissions.ViewDeletedGames,
                Permissions.EditDeletedGame,
            ],
        };
}
