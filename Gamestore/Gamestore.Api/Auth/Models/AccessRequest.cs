namespace Gamestore.Api.Auth.Models;

public class AccessRequest
{
    public required string TargetPage { get; set; }

    public Guid? TargetId { get; set; }
}
