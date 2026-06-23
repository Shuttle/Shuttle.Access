using Shuttle.Access.Application;
using Shuttle.Access.WebApi.Contracts.v1;
using SessionRequest = Shuttle.Access.Application.SessionRequest;

namespace Shuttle.Access.WebApi;

public static class RegisterSessionExtensions
{
    extension(SessionRequest sessionRequest)
    {
        public SessionResponse GetSessionResponse(bool registrationRequested)
        {
            ArgumentNullException.ThrowIfNull(sessionRequest);

            if (sessionRequest.Session == null)
            {
                return new()
                {
                    Result = sessionRequest.Result.ToString()
                };
            }

            return new()
            {
                Token = sessionRequest.SessionToken,
                Result = sessionRequest.Result.ToString(),
                RegistrationRequested = registrationRequested,
                Session = sessionRequest.Session.Map(),
                Tenants = sessionRequest.Tenants.Select(item => new Contracts.v1.Tenant
                {
                    Id = item.Id,
                    Name = item.Name,
                    Status = (int)item.Status,
                    StatusName = item.Status.ToString(),
                    LogoUrl = item.LogoUrl,
                    LogoSvg = item.LogoSvg
                }).ToList()
            };
        }
    }
}