namespace B1ServiceLayer.Exceptions;

public class SAPUnauthorizedException : SAPException
{
    public SAPUnauthorizedException()
    {
    }

    public SAPUnauthorizedException(string? message) : base(message)
    {
    }

    public SAPUnauthorizedException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
