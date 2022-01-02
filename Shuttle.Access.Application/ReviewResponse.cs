using Shuttle.Core.Contract;

namespace Shuttle.Access.Application
{
    public class ReviewResponse
    {
        public bool Ok => !string.IsNullOrWhiteSpace(Message);
        public string Message { get; private set; }

        public ReviewResponse Failed(string message)
        {
            Guard.AgainstNullOrEmptyString(message, nameof(message));

            Message = message;

            return this;
        }
    }
}