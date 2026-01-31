namespace Shuttle.Access.RestClient;

public interface IAuthenticationInterceptor
{
    Task ConfigureAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken = default);
}