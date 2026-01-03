using Microsoft.AspNetCore.Mvc;
using Shuttle.Core.Contract;

namespace Shuttle.Access.AspNetCore;

public class SessionTokenResult
{
    private SessionTokenResult()
    {
        SessionToken = Guid.Empty;
        ActionResult = null;
    }

    public IActionResult? ActionResult { get; private init; }

    public bool Ok => ActionResult == null;

    public Guid SessionToken { get; private init; }

    public static SessionTokenResult Failure(IActionResult httpActionResult)
    {
        Guard.AgainstNull(httpActionResult);

        return new()
        {
            ActionResult = httpActionResult
        };
    }

    public static SessionTokenResult Success(Guid sessionToken)
    {
        return new()
        {
            SessionToken = sessionToken
        };
    }
}