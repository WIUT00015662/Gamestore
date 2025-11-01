using System.Security.Claims;
using Gamestore.Api.Auth;
using Microsoft.AspNetCore.Authorization;

namespace GameStore.UnitTests.Api.Auth;

public class PermissionAuthorizationHandlerTests
{
    [Fact]
    public async Task HandleRequirementAsyncSucceedsWhenPermissionClaimExists()
    {
        var requirement = new PermissionRequirement(Permissions.ManageUsers);
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim("permission", Permissions.ManageUsers)], "TestAuth"));
        var context = new AuthorizationHandlerContext([requirement], principal, null);
        var handler = new PermissionAuthorizationHandler();

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleRequirementAsyncDoesNotSucceedWhenPermissionClaimDoesNotExist()
    {
        var requirement = new PermissionRequirement(Permissions.ManageUsers);
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim("permission", Permissions.ViewGame)], "TestAuth"));
        var context = new AuthorizationHandlerContext([requirement], principal, null);
        var handler = new PermissionAuthorizationHandler();

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }
}
