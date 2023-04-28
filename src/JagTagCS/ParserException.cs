namespace JagTagCS;

public class ParserException : Exception
{
    public ParserException(string message, Exception cause) : base(message, cause)
    {
    }

    public ParserException(string message) : base(message)
    {
    }
}
