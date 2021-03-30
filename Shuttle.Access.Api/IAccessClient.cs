using System;
using RestSharp;

namespace Shuttle.Access.Api
{
    public interface IAccessClient
    {
        bool HasSession { get; }
        T Get<T>(RestRequest request) where T : new();
        IRestResponse GetResponse(RestRequest request);
        void Register(string name, string password);
        void Logout();
        void Login();
        void Activate(string name, DateTime dateActivate);
        void Activate(Guid id, DateTime dateActivate);
        Guid GetPasswordResetToken(string name);
        void ResetPassword(string name, Guid passwordResetToken, string password);
    }
}