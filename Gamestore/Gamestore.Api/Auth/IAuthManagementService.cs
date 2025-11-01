using System.Security.Claims;
using Gamestore.Api.Auth.Models;

namespace Gamestore.Api.Auth;

public interface IAuthManagementService
{
    Task<TokenResponse> LoginAsync(LoginRequest request);

    Task<bool> CheckAccessAsync(ClaimsPrincipal principal, AccessRequest request);

    Task<IEnumerable<BasicUserResponse>> GetUsersAsync();

    Task<BasicUserResponse> GetUserByIdAsync(Guid id);

    Task DeleteUserAsync(Guid id);

    Task<BasicUserResponse> CreateUserAsync(CreateOrUpdateUserRequest request);

    Task UpdateUserAsync(CreateOrUpdateUserRequest request);

    Task<IEnumerable<BasicRoleResponse>> GetRolesAsync();

    Task<BasicRoleResponse> GetRoleByIdAsync(Guid id);

    Task DeleteRoleAsync(Guid id);

    Task<BasicRoleResponse> CreateRoleAsync(CreateOrUpdateRoleRequest request);

    Task UpdateRoleAsync(CreateOrUpdateRoleRequest request);

    Task<IEnumerable<BasicRoleResponse>> GetUserRolesAsync(Guid userId);

    IEnumerable<string> GetPermissions();

    Task<IEnumerable<string>> GetRolePermissionsAsync(Guid roleId);

    Task EnsureDefaultsAsync();
}
