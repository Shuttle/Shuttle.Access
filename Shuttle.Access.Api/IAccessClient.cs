using RestSharp;

namespace Shuttle.Access.Api
{
    public interface IAccessClient
    {
        bool HasSession { get; }
        T Get<T>(RestRequest request) where T : new();
        IRestResponse GetResponse(RestRequest request);
        void Register(string identityName);
        void Logout();
        void Login();
    }
}