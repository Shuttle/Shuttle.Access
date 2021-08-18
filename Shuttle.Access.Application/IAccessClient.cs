using System;
using RestSharp;

namespace Shuttle.Access.Application
{
    public interface IAccessClient
    {
        bool HasSession { get; }
        T Get<T>(RestRequest request) where T : new();
        IRestResponse GetResponse(RestRequest request);
        void Register(string name, string password, string system);
        void Logout();
        void Login();
        void Activate(string name, DateTime dateActivate);
        void Activate(Guid id, DateTime dateActivate);
        Guid GetPasswordResetToken(string name);
        void ResetPassword(string name, Guid passwordResetToken, string password);
        RegisterSessionResult RegisterSession(string identityName);
        GetDataResult<DataAccess.Query.Session> GetSession(Guid token);
    }
}