using Shuttle.Core.Contract;

namespace Shuttle.Access.Application
{
    public class GetDataResult<T>
    {
        public T Data { get; private set; }
        public string Message { get; private set; }

        public bool Ok => string.IsNullOrWhiteSpace(Message);

        public static GetDataResult<T> Success(T data)
        {
            Guard.AgainstNull(data, nameof(data));

            return new GetDataResult<T>
            {
                Data = data
            };
        }

        public static GetDataResult<T> Failure(string message)
        {
            Guard.AgainstNullOrEmptyString(message, nameof(message));

            return new GetDataResult<T>
            {
                Message = message
            };
        }
    }
}