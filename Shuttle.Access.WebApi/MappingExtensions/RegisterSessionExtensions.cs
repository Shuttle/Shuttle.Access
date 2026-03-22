using Shuttle.Access.WebApi.Contracts.v1;
using RegisterSession = Shuttle.Access.Application.RegisterSession;

namespace Shuttle.Access.WebApi;

public static class RegisterSessionExtensions
{
    extension(RegisterSession registerSession)
    {
        public SessionResponse GetSessionResponse(bool registrationRequested)
        {
            ArgumentNullException.ThrowIfNull(registerSession);

            if (registerSession.Session == null)
            {
                return new()
                {
                    Result = registerSession.Result.ToString()
                };
            }

            return new()
            {
                Token = registerSession.SessionToken,
                Result = registerSession.Result.ToString(),
                RegistrationRequested = registrationRequested,
                Session = registerSession.Session.Map(),
                Tenants = registerSession.Tenants.Select(item => new Contracts.v1.Tenant
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