namespace RTextLogParser.Library;

public class LogElement
{
    public string Log { get; set; }
    public string[] RegexGroups { get; set; }

    public LogElement(string log, IEnumerable<string> regexMatchedGroups)
    {
        Log = log;
        RegexGroups = regexMatchedGroups.ToArray();
    }
}