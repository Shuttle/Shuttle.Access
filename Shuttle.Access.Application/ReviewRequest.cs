using Shuttle.Core.Contract;

namespace Shuttle.Access.Application
{
    public class ReviewRequest<T> where T : class
    {
        public ReviewRequest(T request)
        {
            Guard.AgainstNull(request, nameof(request));

            Request = request;
        }

        public T Request { get; }

        public bool Ok => string.IsNullOrWhiteSpace(Message);

        public string Message { get; private set; }

        public ReviewRequest<T> Failed(string message)
        {
            Guard.AgainstNullOrEmptyString(message, nameof(message));

            Message = message;

            return this;
        }
    }
}