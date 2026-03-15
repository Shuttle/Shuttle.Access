namespace Shuttle.Access.WebApi.Contracts.v1;

public static class SessionResponseExtensions
{
    extension(SessionResponse sessionResponse)
    {
        public bool IsSuccessResult()
        {
            return sessionResponse.Result.Equals("Registered", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}