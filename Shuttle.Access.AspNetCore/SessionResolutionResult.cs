using System.Diagnostics.CodeAnalysis;
using Shuttle.Contract;

namespace Shuttle.Access.AspNetCore;

/// <summary>
///     The outcome of an <see cref="ISessionResolver" /> call.
/// </summary>
public sealed class SessionResolutionResult
{
    /// <summary>
    ///     No credential was presented, so no attempt was made to establish a session.
    /// </summary>
    public static readonly SessionResolutionResult None = new();

    private SessionResolutionResult()
    {
    }

    private SessionResolutionResult(string failureReason)
    {
        FailureReason = Guard.AgainstEmpty(failureReason);
    }

    private SessionResolutionResult(string identityName, Guid tenantId, Query.Session? session, Guid? sessionToken)
    {
        IdentityName = Guard.AgainstEmpty(identityName);
        TenantId = tenantId;
        Session = session;
        SessionToken = sessionToken;
    }

    public string? FailureReason { get; }

    /// <summary>
    ///     The identity established from the credential.  This is available even when no <see cref="Session" /> could be
    ///     found, which is how an identity that does not yet have a session is able to register one.
    /// </summary>
    public string? IdentityName { get; }

    public Query.Session? Session { get; }
    public Guid? SessionToken { get; }
    public Guid TenantId { get; }

    [MemberNotNullWhen(true, nameof(IdentityName))]
    public bool IsAuthenticated => IdentityName != null;

    [MemberNotNullWhen(true, nameof(FailureReason))]
    public bool IsFailure => FailureReason != null;

    /// <summary>
    ///     A credential was presented and validated, and an active session was found.
    /// </summary>
    public static SessionResolutionResult Authenticated(Query.Session session, Guid tenantId, Guid? sessionToken = null)
    {
        return new(Guard.AgainstNull(session).IdentityName, tenantId, session, sessionToken);
    }

    /// <summary>
    ///     A credential was presented and validated, but there is no active session for the identity.
    /// </summary>
    public static SessionResolutionResult Authenticated(string identityName, Guid tenantId)
    {
        return new(identityName, tenantId, null, null);
    }

    /// <summary>
    ///     A credential was presented but could not be validated.
    /// </summary>
    public static SessionResolutionResult Failure(string failureReason)
    {
        return new(failureReason);
    }
}
