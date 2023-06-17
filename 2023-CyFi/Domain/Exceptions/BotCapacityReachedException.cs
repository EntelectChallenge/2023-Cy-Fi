namespace Domain.Exceptions
{
    public class BotCapacityReachedException : Exception
    {
        public BotCapacityReachedException()
        {
        }

        public BotCapacityReachedException(string message) : base(message)
        {
        }

        public BotCapacityReachedException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
