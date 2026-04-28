using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Gamestore.Api.Auth;
using Gamestore.Api.Auth.Models;
using Gamestore.Domain.Entities;
using Gamestore.Domain.Repositories;
using Moq;

namespace GameStore.UnitTests.Api.Auth;

public class AuthManagementServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IGameRepository> _gameRepositoryMock;
    private readonly AuthManagementService _service;

    public AuthManagementServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _gameRepositoryMock = new Mock<IGameRepository>();

        _unitOfWorkMock.Setup(x => x.Users).Returns(_userRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.Roles).Returns(_roleRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.Games).Returns(_gameRepositoryMock.Object);

        var jwtSettings = new JwtSettings
        {
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            Key = "test_secret_key_that_is_long_enough_1234567890",
            ExpirationMinutes = 60,
        };

        _service = new AuthManagementService(_unitOfWorkMock.Object, jwtSettings);
    }

    [Fact]
    public async Task LoginAsyncThrowsWhenInternalAuthenticationIsDisabled()
    {
        var request = new LoginRequest
        {
            Model = new LoginModel
            {
                Login = "user",
                Password = "pass",
                InternalAuth = false,
            },
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _service.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsyncThrowsWhenPasswordIsInvalid()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "user",
            PasswordHash = HashPassword("correct-password"),
        };

        _userRepositoryMock.Setup(x => x.GetByNameAsync("user")).ReturnsAsync(user);

        var request = new LoginRequest
        {
            Model = new LoginModel
            {
                Login = "user",
                Password = "wrong-password",
                InternalAuth = true,
            },
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _service.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsyncReturnsTokenWithRoleAndPermissionClaims()
    {
        var userRole = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            Permissions =
            [
                new RolePermissionEntry { Permission = Permissions.ViewGame },
                new RolePermissionEntry { Permission = Permissions.ManageUsers },
            ],
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "john",
            PasswordHash = HashPassword("secret"),
            UserRoles =
            [
                new UserRole { Role = userRole, RoleId = userRole.Id },
            ],
        };

        _userRepositoryMock.Setup(x => x.GetByNameAsync("john")).ReturnsAsync(user);

        var request = new LoginRequest
        {
            Model = new LoginModel
            {
                Login = "john",
                Password = "secret",
                InternalAuth = true,
            },
        };

        var response = await _service.LoginAsync(request);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(response.Token);

        Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.Name && c.Value == "john");
        Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        Assert.Contains(jwt.Claims, c => c.Type == "permission" && c.Value == Permissions.ViewGame);
        Assert.Contains(jwt.Claims, c => c.Type == "permission" && c.Value == Permissions.ManageUsers);
    }

    [Fact]
    public async Task CheckAccessAsyncReturnsFalseWhenPermissionClaimIsMissing()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity());
        var request = new AccessRequest { TargetPage = "users" };

        var result = await _service.CheckAccessAsync(principal, request);

        Assert.False(result);
    }

    [Fact]
    public async Task CheckAccessAsyncReturnsTrueForExistingGame()
    {
        var principal = CreatePrincipal(Permissions.ViewGame);
        var gameId = Guid.NewGuid();

        _gameRepositoryMock.Setup(x => x.GetByIdWithDetailsAsync(gameId)).ReturnsAsync(new Game
        {
            Id = gameId,
            Name = "Game",
            Key = "game",
        });

        var request = new AccessRequest { TargetPage = "games", TargetId = gameId };

        var result = await _service.CheckAccessAsync(principal, request);

        Assert.True(result);
    }

    [Fact]
    public async Task CheckAccessAsyncReturnsTrueForUsersPageWithManageUsersPermission()
    {
        var principal = CreatePrincipal(Permissions.ManageUsers);
        var request = new AccessRequest { TargetPage = "users" };

        var result = await _service.CheckAccessAsync(principal, request);

        Assert.True(result);
    }

    private static ClaimsPrincipal CreatePrincipal(params string[] permissions)
    {
        var claims = permissions.Select(p => new Claim("permission", p));
        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100000, HashAlgorithmName.SHA256, 32);
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }
}
