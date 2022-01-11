using Shuttle.Core.Contract;

namespace Shuttle.Access.Application
{
    public class RequestMessage<TRequest>
    {
        public RequestMessage(TRequest request)
        {
            if (!typeof(TRequest).IsValueType)
            {
                Guard.AgainstNull(request, nameof(request));
            }

            Request = request;
        }

        public TRequest Request { get; }

        public bool Ok => string.IsNullOrWhiteSpace(Message);

        public string Message { get; private set; }

        public RequestMessage<TRequest> Failed(string message)
        {
            Guard.AgainstNullOrEmptyString(message, nameof(message));

            Message = message;

            return this;
        }
    }
}