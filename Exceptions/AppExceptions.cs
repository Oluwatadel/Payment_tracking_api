namespace PaymentTracker.Exceptions
{
    public class AppExceptions : Exception
    {
        public AppExceptions()
        {
        }

        public AppExceptions(string message) : base(message)
        {
        }

        public AppExceptions(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
