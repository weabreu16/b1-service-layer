namespace B1ServiceLayer.Exceptions;

public class SAPException : Exception
{
    public SAPException()
    {
    }

    public SAPException(string? message) : base(message)
    {
    }

    public SAPException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
