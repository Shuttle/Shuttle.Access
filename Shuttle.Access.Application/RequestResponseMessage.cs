using Shuttle.Core.Contract;

namespace Shuttle.Access.Application
{
    public class RequestResponseMessage<TRequest, TResponse> : RequestMessage<TRequest>
    {
        public RequestResponseMessage(TRequest request) : base(request)
        {
        }

        public RequestResponseMessage(TRequest request, TResponse response) : base(request)
        {
            if (!typeof(TResponse).IsValueType)
            {
                Guard.AgainstNull(response, nameof(response));
            }

            Response = response;
        }

        public TResponse Response { get; private set; }

        public RequestResponseMessage<TRequest, TResponse> WithResponse(TResponse response)
        {
            Guard.AgainstNull(response, nameof(response));

            Response = response;

            return this;
        }
    }
}