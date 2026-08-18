using Microsoft.AspNetCore.Http;

namespace Shuttle.Access.AspNetCore;

/// <summary>
///     Establishes the <see cref="Query.Session" /> represented by an incoming request.
/// </summary>
/// <remarks>
///     The default implementation, <see cref="DelegatedSessionResolver" />, asks the Shuttle.Access web API who the
///     caller is; it never inspects the credential.  The Shuttle.Access web API is the only application that should
///     replace this — it is the authority, and validating issuers or session tokens anywhere else duplicates security
///     configuration that belongs in one place.
/// </remarks>
public interface ISessionResolver
{
    Task<SessionResolutionResult> ResolveAsync(HttpContext httpContext, CancellationToken cancellationToken = default);
}
