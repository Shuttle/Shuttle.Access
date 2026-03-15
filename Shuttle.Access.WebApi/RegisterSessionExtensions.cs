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

            if (registerSession.Identity == null || registerSession.Session == null)
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
                Session = new()
                {
                    Id = registerSession.Session.Id,
                    TenantId = registerSession.Session.TenantId,
                    IdentityId = registerSession.Session.IdentityId,
                    IdentityName = registerSession.Session.IdentityName,
                    DateRegistered = registerSession.Session.DateRegistered,
                    ExpiryDate = registerSession.Session.ExpiryDate,
                    Permissions = registerSession.Session.Permissions.Select(item => new Contracts.v1.Permission
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Description = item.Description,
                        Status = (int)item.Status,
                        StatusName = item.Status.ToString()
                    }).ToList()
                },
                Tenants = registerSession.Tenants.Select(item => new SessionResponse.Tenant
                {
                    Id = item.Id,
                    Name = item.Name
                }).ToList()
            };
        }
    }
}