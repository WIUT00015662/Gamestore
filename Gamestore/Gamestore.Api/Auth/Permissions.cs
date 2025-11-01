namespace Gamestore.Api.Auth;

public static class Permissions
{
    public const string ViewGame = "ViewGame";
    public const string AddGame = "AddGame";
    public const string UpdateGame = "UpdateGame";
    public const string DeleteGame = "DeleteGame";
    public const string BuyGame = "BuyGame";
    public const string CommentGame = "CommentGame";
    public const string ManageComments = "ManageComments";
    public const string BanUsers = "BanUsers";
    public const string ManageEntities = "ManageEntities";
    public const string ManageOrders = "ManageOrders";
    public const string ViewOrderHistory = "ViewOrderHistory";
    public const string ShipOrder = "ShipOrder";
    public const string ManageUsers = "ManageUsers";
    public const string ManageRoles = "ManageRoles";
    public const string ViewDeletedGames = "ViewDeletedGames";
    public const string EditDeletedGame = "EditDeletedGame";

    public static readonly IReadOnlyList<string> All =
    [
        ViewGame,
        AddGame,
        UpdateGame,
        DeleteGame,
        BuyGame,
        CommentGame,
        ManageComments,
        BanUsers,
        ManageEntities,
        ManageOrders,
        ViewOrderHistory,
        ShipOrder,
        ManageUsers,
        ManageRoles,
        ViewDeletedGames,
        EditDeletedGame,
    ];
}
