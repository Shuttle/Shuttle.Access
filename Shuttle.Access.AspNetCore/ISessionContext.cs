namespace Shuttle.Access.AspNetCore;

public interface ISessionContext
{
    Guid TenantId { get; set; }

    /// <summary>
    ///     Never <c>null</c> — an unauthenticated request has <see cref="Query.Session.Empty" />, which has no
    ///     permissions or tokens.  Check <see cref="IsAuthorized" /> to distinguish a real session from the empty one.
    /// </summary>
    public Query.Session Session { get; set; }

    public bool IsAuthorized { get; }
}
