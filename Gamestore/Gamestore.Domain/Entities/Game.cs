using System.Text.RegularExpressions;

namespace Gamestore.Domain.Entities;

public partial class Game
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string Key { get; set; }

    public string? Description { get; set; }

    public int UnitInStock { get; set; }

    public Guid PublisherId { get; set; }

    public Publisher? Publisher { get; set; }

    public DateTime PublishDate { get; set; } = DateTime.UtcNow;

    public int ViewCount { get; set; }

    public ICollection<Genre> Genres { get; set; } = [];

    public ICollection<Platform> Platforms { get; set; } = [];

    public ICollection<Comment> Comments { get; set; } = [];

    public ICollection<GameVendorOffer> VendorOffers { get; set; } = [];

    /// <summary>
    /// Generates a URL-friendly key from a game name.
    /// </summary>
    public static string GenerateKeyFromName(string name)
    {
        var key = name.ToLowerInvariant().Trim();
        key = InvalidCharsRegex().Replace(key, string.Empty);
        key = WhitespaceRegex().Replace(key, "-");
        key = DashRegex().Replace(key, "-");
        return key.Trim('-');
    }

    [GeneratedRegex(@"[^a-z0-9\s-]")]
    private static partial Regex InvalidCharsRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"-+")]
    private static partial Regex DashRegex();
}
