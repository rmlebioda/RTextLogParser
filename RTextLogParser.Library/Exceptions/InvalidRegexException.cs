using System.Runtime.Serialization;

namespace RTextLogParser.Library.Exceptions;

[Serializable]
public class InvalidRegexException : Exception
{
    protected InvalidRegexException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public InvalidRegexException(string? message) : base(message)
    {
    }

    public InvalidRegexException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public InvalidRegexException()
    {
    }
}