using Shuttle.Core.Contract;

namespace Shuttle.Access.Application
{
    public class RequestResponse<TRequest, TResponse> where TRequest : class where TResponse : class
    {
        public RequestResponse(TRequest request)
        {
            Guard.AgainstNull(request, nameof(request));

            Request = request;
        }

        public RequestResponse(TRequest request, TResponse response)
        {
            Guard.AgainstNull(request, nameof(request));
            Guard.AgainstNull(response, nameof(response));

            Request = request;
            Response = response;
        }

        public TRequest Request { get; }
        public TResponse Response { get; private set; } = null;

        public RequestResponse<TRequest, TResponse> WithResponse(TResponse response)
        {
            Guard.AgainstNull(response, nameof(response));

            Response = response;

            return this;
        }
    }
}