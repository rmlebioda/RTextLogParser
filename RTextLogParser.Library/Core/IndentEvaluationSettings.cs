namespace RTextLogParser.Library.Core;

public class IndentEvaluationSettings
{
    /// <summary>
    /// Code in C# to evaluate string
    /// </summary>
    public string EvaluationString { get; set; } = string.Empty;
    /// <summary>
    /// Groups, to which indent should be evaulated
    /// </summary>
    public long GroupId { get; set; }
}