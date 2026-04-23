using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Gamestore.Api.Auth.Models;
using Gamestore.Domain.Entities;
using Gamestore.Domain.Exceptions;
using Gamestore.Domain.Repositories;
using Microsoft.IdentityModel.Tokens;

namespace Gamestore.Api.Auth;

public class AuthManagementService(IUnitOfWork unitOfWork, JwtSettings jwtSettings) : IAuthManagementService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly JwtSettings _jwtSettings = jwtSettings;

    public async Task<TokenResponse> LoginAsync(LoginRequest request)
    {
        if (!request.Model.InternalAuth)
        {
            throw new ArgumentException("Only internal authentication is supported.");
        }

        var user = await _unitOfWork.Users.GetByNameAsync(request.Model.Login)
            ?? throw new ArgumentException("Invalid credentials.");

        if (!VerifyPassword(request.Model.Password, user.PasswordHash))
        {
            throw new ArgumentException("Invalid credentials.");
        }

        var roles = user.UserRoles
            .Where(ur => ur.Role is not null)
            .Select(ur => ur.Role)
            .OfType<Role>()
            .ToList();

        var permissions = roles
            .SelectMany(r => r.Permissions.Select(p => p.Permission))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new TokenResponse { Token = CreateToken(user, roles.Select(r => r.Name), permissions) };
    }

    public async Task<TokenResponse> RegisterAsync(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Username, email and password are required.");
        }

        if (await _unitOfWork.Users.GetByNameAsync(request.UserName.Trim()) is not null)
        {
            throw new EntityAlreadyExistsException(nameof(User), nameof(User.Name), request.UserName.Trim());
        }

        var userRole = await _unitOfWork.Roles.GetByNameAsync(DefaultRoles.User)
            ?? throw new EntityNotFoundException(nameof(Role), DefaultRoles.User);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.UserName.Trim(),
            PasswordHash = HashPassword(request.Password),
            UserRoles =
            [
                new UserRole { RoleId = userRole.Id },
            ],
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var permissions = userRole.Permissions.Select(p => p.Permission).Distinct(StringComparer.OrdinalIgnoreCase);
        return new TokenResponse { Token = CreateToken(user, [userRole.Name], permissions) };
    }

    public async Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var principal = ValidateExpiredToken(request.Token);
        var userName = principal.Identity?.Name
            ?? principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
            ?? throw new ArgumentException("Invalid token.");

        var user = await _unitOfWork.Users.GetByNameAsync(userName)
            ?? throw new ArgumentException("User no longer exists.");

        var roles = user.UserRoles
            .Where(ur => ur.Role is not null)
            .Select(ur => ur.Role)
            .OfType<Role>()
            .ToList();

        var permissions = roles
            .SelectMany(r => r.Permissions.Select(p => p.Permission))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new TokenResponse { Token = CreateToken(user, roles.Select(r => r.Name), permissions) };
    }

    public async Task<bool> CheckAccessAsync(ClaimsPrincipal principal, AccessRequest request)
    {
        var permission = request.TargetPage.Trim().ToLowerInvariant() switch
        {
            "game" or "games" => Permissions.ViewGame,
            "genre" or "genres" or "platform" or "platforms" or "publisher" or "publishers" => Permissions.ManageEntities,
            "order" or "orders" => Permissions.ManageOrders,
            "comment" or "comments" => Permissions.ManageComments,
            "user" or "users" => Permissions.ManageUsers,
            "role" or "roles" => Permissions.ManageRoles,
            _ => Permissions.ViewGame,
        };

        var hasPermission = principal.Claims.Any(c => c.Type == "permission" && c.Value.Equals(permission, StringComparison.OrdinalIgnoreCase));
        if (!hasPermission)
        {
            return false;
        }

        if ((request.TargetPage.Equals("game", StringComparison.OrdinalIgnoreCase)
            || request.TargetPage.Equals("games", StringComparison.OrdinalIgnoreCase))
            && request.TargetId is { } gameId)
        {
            var game = await _unitOfWork.Games.GetByIdWithDetailsIncludingDeletedAsync(gameId);
            if (game is null)
            {
                return false;
            }

            if (game.IsDeleted && !principal.Claims.Any(c => c.Type == "permission" && c.Value == Permissions.ViewDeletedGames))
            {
                return false;
            }
        }

        return true;
    }

    public async Task<IEnumerable<BasicUserResponse>> GetUsersAsync()
    {
        var users = await _unitOfWork.Users.GetAllAsync();
        return users.Select(u => new BasicUserResponse { Id = u.Id, Name = u.Name });
    }

    public async Task<BasicUserResponse> GetUserByIdAsync(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(User), id);

        return new BasicUserResponse { Id = user.Id, Name = user.Name };
    }

    public async Task DeleteUserAsync(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdWithRolesAsync(id)
            ?? throw new EntityNotFoundException(nameof(User), id);

        _unitOfWork.Users.Delete(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<BasicUserResponse> CreateUserAsync(CreateOrUpdateUserRequest request)
    {
        if (await _unitOfWork.Users.GetByNameAsync(request.User.Name) is not null)
        {
            throw new EntityAlreadyExistsException(nameof(User), nameof(User.Name), request.User.Name);
        }

        var roles = await ResolveRolesAsync(request.Roles);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.User.Name,
            PasswordHash = HashPassword(request.Password),
            UserRoles =
            [
                .. roles.Select(r => new UserRole { RoleId = r.Id }),
            ],
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return new BasicUserResponse { Id = user.Id, Name = user.Name };
    }

    public async Task UpdateUserAsync(CreateOrUpdateUserRequest request)
    {
        var user = await _unitOfWork.Users.GetByIdWithRolesAsync(request.User.Id)
            ?? throw new EntityNotFoundException(nameof(User), request.User.Id);

        var duplicate = await _unitOfWork.Users.GetByNameAsync(request.User.Name);
        if (duplicate is not null && duplicate.Id != user.Id)
        {
            throw new EntityAlreadyExistsException(nameof(User), nameof(User.Name), request.User.Name);
        }

        var roles = await ResolveRolesAsync(request.Roles);

        user.Name = request.User.Name;
        user.PasswordHash = HashPassword(request.Password);
        user.UserRoles.Clear();
        foreach (var role in roles)
        {
            user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        }

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<BasicRoleResponse>> GetRolesAsync()
    {
        var roles = await _unitOfWork.Roles.GetAllAsync();
        return roles.Select(r => new BasicRoleResponse { Id = r.Id, Name = r.Name });
    }

    public async Task<BasicRoleResponse> GetRoleByIdAsync(Guid id)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(id)
            ?? throw new EntityNotFoundException(nameof(Role), id);

        return new BasicRoleResponse { Id = role.Id, Name = role.Name };
    }

    public async Task DeleteRoleAsync(Guid id)
    {
        var role = await _unitOfWork.Roles.GetByIdWithPermissionsAsync(id)
            ?? throw new EntityNotFoundException(nameof(Role), id);

        _unitOfWork.Roles.Delete(role);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<BasicRoleResponse> CreateRoleAsync(CreateOrUpdateRoleRequest request)
    {
        if (await _unitOfWork.Roles.GetByNameAsync(request.Role.Name) is not null)
        {
            throw new EntityAlreadyExistsException(nameof(Role), nameof(Role.Name), request.Role.Name);
        }

        ValidatePermissions(request.Permissions);

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = request.Role.Name,
            IsSystem = false,
            Permissions =
            [
                .. request.Permissions
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Select(p => new RolePermissionEntry { Permission = p }),
            ],
        };

        await _unitOfWork.Roles.AddAsync(role);
        await _unitOfWork.SaveChangesAsync();

        return new BasicRoleResponse { Id = role.Id, Name = role.Name };
    }

    public async Task UpdateRoleAsync(CreateOrUpdateRoleRequest request)
    {
        var role = await _unitOfWork.Roles.GetByIdWithPermissionsAsync(request.Role.Id)
            ?? throw new EntityNotFoundException(nameof(Role), request.Role.Id);

        var duplicate = await _unitOfWork.Roles.GetByNameAsync(request.Role.Name);
        if (duplicate is not null && duplicate.Id != role.Id)
        {
            throw new EntityAlreadyExistsException(nameof(Role), nameof(Role.Name), request.Role.Name);
        }

        ValidatePermissions(request.Permissions);

        role.Name = request.Role.Name;
        role.Permissions.Clear();
        foreach (var permission in request.Permissions.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            role.Permissions.Add(new RolePermissionEntry { RoleId = role.Id, Permission = permission });
        }

        _unitOfWork.Roles.Update(role);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<BasicRoleResponse>> GetUserRolesAsync(Guid userId)
    {
        var user = await _unitOfWork.Users.GetByIdWithRolesAsync(userId)
            ?? throw new EntityNotFoundException(nameof(User), userId);

        return user.UserRoles
            .Where(ur => ur.Role is not null)
            .Select(ur => ur.Role)
            .OfType<Role>()
            .Select(role => new BasicRoleResponse { Id = role.Id, Name = role.Name });
    }

    public IEnumerable<string> GetPermissions()
    {
        return Permissions.All;
    }

    public async Task<IEnumerable<string>> GetRolePermissionsAsync(Guid roleId)
    {
        var role = await _unitOfWork.Roles.GetByIdWithPermissionsAsync(roleId)
            ?? throw new EntityNotFoundException(nameof(Role), roleId);

        return role.Permissions.Select(p => p.Permission);
    }

    public async Task EnsureDefaultsAsync()
    {
        if ((await _unitOfWork.Roles.GetAllAsync()).Any())
        {
            return;
        }

        var roleEntities = new Dictionary<string, Role>(StringComparer.OrdinalIgnoreCase);
        foreach (var role in DefaultRoles.PermissionsByRole)
        {
            roleEntities[role.Key] = new Role
            {
                Id = Guid.NewGuid(),
                Name = role.Key,
                IsSystem = true,
                Permissions =
                [
                    .. role.Value.Select(p => new RolePermissionEntry { Permission = p }),
                ],
            };
        }

        foreach (var role in roleEntities.Values)
        {
            await _unitOfWork.Roles.AddAsync(role);
        }

        var admin = new User
        {
            Id = Guid.NewGuid(),
            Name = "admin",
            PasswordHash = HashPassword("admin"),
            UserRoles =
            [
                new UserRole { RoleId = roleEntities[DefaultRoles.Administrator].Id },
            ],
        };

        await _unitOfWork.Users.AddAsync(admin);
        await _unitOfWork.SaveChangesAsync();
    }

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100000, HashAlgorithmName.SHA256, 32);
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    private static bool VerifyPassword(string password, string stored)
    {
        var parts = stored.Split(':');
        if (parts.Length != 2)
        {
            return false;
        }

        var salt = Convert.FromBase64String(parts[0]);
        var hash = Convert.FromBase64String(parts[1]);
        var incoming = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100000, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(hash, incoming);
    }

    private async Task<List<Role>> ResolveRolesAsync(IEnumerable<Guid> roleIds)
    {
        var roles = new List<Role>();

        foreach (var roleId in roleIds.Distinct())
        {
            var role = await _unitOfWork.Roles.GetByIdAsync(roleId)
                ?? throw new EntityNotFoundException(nameof(Role), roleId);
            roles.Add(role);
        }

        return roles;
    }

    private static void ValidatePermissions(IEnumerable<string> permissions)
    {
        var invalid = permissions.FirstOrDefault(p => !Permissions.All.Contains(p, StringComparer.OrdinalIgnoreCase));
        if (invalid is not null)
        {
            throw new ArgumentException($"Unsupported permission: {invalid}");
        }
    }

    private string CreateToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(permissions.Select(permission => new Claim("permission", permission)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private ClaimsPrincipal ValidateExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = false,
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key)),
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtToken ||
            !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token.");
        }

        return principal;
    }
}
