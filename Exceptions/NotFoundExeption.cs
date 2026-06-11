namespace PaymentTracker.Exceptions
{
    public class NotFoundException(string message) : Exception(message);
    public class SaveOperationException(string message) : Exception(message);
}
