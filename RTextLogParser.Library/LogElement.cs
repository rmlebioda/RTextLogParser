namespace RTextLogParser.Library;

public class LogElement
{
    public string Log { get; init; }
    public string[] RegexGroups { get; init; }
    public long Indent { get; init; }

    public LogElement(string log, string[] regexMatchedGroups, long indent)
    {
        Log = log;
        RegexGroups = regexMatchedGroups;
        Indent = indent;
    }
}