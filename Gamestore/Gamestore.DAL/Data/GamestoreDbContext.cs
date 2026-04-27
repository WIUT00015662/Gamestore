using Gamestore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.DAL.Data;

/// <summary>
/// Database context for the Gamestore application.
/// </summary>
public class GamestoreDbContext(DbContextOptions<GamestoreDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets or sets the Games DbSet.
    /// </summary>
    public DbSet<Game> Games { get; set; }

    /// <summary>
    /// Gets or sets the Genres DbSet.
    /// </summary>
    public DbSet<Genre> Genres { get; set; }

    /// <summary>
    /// Gets or sets the Platforms DbSet.
    /// </summary>
    public DbSet<Platform> Platforms { get; set; }

    /// <summary>
    /// Gets or sets the Publishers DbSet.
    /// </summary>
    public DbSet<Publisher> Publishers { get; set; }

    /// <summary>
    /// Gets or sets the Orders DbSet.
    /// </summary>
    public DbSet<Order> Orders { get; set; }

    /// <summary>
    /// Gets or sets the OrderGames DbSet.
    /// </summary>
    public DbSet<OrderGame> OrderGames { get; set; }

    /// <summary>
    /// Gets or sets the Comments DbSet.
    /// </summary>
    public DbSet<Comment> Comments { get; set; }

    /// <summary>
    /// Gets or sets the CommentBans DbSet.
    /// </summary>
    public DbSet<CommentBan> CommentBans { get; set; }

    /// <summary>
    /// Gets or sets the Users DbSet.
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Gets or sets the Roles DbSet.
    /// </summary>
    public DbSet<Role> Roles { get; set; }

    /// <summary>
    /// Gets or sets the UserRoles DbSet.
    /// </summary>
    public DbSet<UserRole> UserRoles { get; set; }

    /// <summary>
    /// Gets or sets the RolePermissions DbSet.
    /// </summary>
    public DbSet<RolePermissionEntry> RolePermissions { get; set; }

    /// <summary>
    /// Gets or sets the GameVendorOffers DbSet.
    /// </summary>
    public DbSet<GameVendorOffer> GameVendorOffers { get; set; }

    /// <summary>
    /// Gets or sets the GameDiscountSnapshots DbSet.
    /// </summary>
    public DbSet<GameDiscountSnapshot> GameDiscountSnapshots { get; set; }

    /// <summary>
    /// Gets or sets the EmailSubscriptions DbSet.
    /// </summary>
    public DbSet<EmailSubscription> EmailSubscriptions { get; set; }

    /// <summary>
    /// Gets or sets the GameDiscounts DbSet.
    /// </summary>
    public DbSet<GameDiscount> GameDiscounts { get; set; }

    /// <summary>
    /// Gets or sets the DiscountConfigurations DbSet.
    /// </summary>
    public DbSet<DiscountConfiguration> DiscountConfigurations { get; set; }

    /// <summary>
    /// Gets or sets the PollingRuns DbSet.
    /// </summary>
    public DbSet<PollingRun> PollingRuns { get; set; }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GamestoreDbContext).Assembly);
    }
}
