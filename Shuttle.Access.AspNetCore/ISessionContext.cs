using System.Diagnostics.CodeAnalysis;

namespace Shuttle.Access.AspNetCore;

public interface ISessionContext
{
    public Shuttle.Access.Messages.v1.Session? Session { get; set; }

    [MemberNotNullWhen(true, nameof(Session))]
    public bool IsAuthorized { get; }
}
