using Shuttle.Core.Contract;

namespace Shuttle.Access.Application
{
    public class RequestResponse<TRequest, TResponse>
    {
        public RequestResponse(TRequest request, TResponse response)
        {
            Guard.AgainstNull(request, nameof(request));
            Guard.AgainstNull(response, nameof(response));

            Request = request;
            Response = response;
        }

        public TRequest Request { get; }
        public TResponse Response { get; }
    }
}