using System.Diagnostics.CodeAnalysis;

namespace Shuttle.Access.AspNetCore;

public interface ISessionContext
{
    public Query.Session? Session { get; set; }

    [MemberNotNullWhen(true, nameof(Session))]
    public bool IsAuthorized { get; }
}
