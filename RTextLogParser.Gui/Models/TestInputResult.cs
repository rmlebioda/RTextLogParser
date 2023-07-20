namespace RTextLogParser.Gui.Models;

public class TestInputResult
{
    public string Type { get; set; }
    public string Field { get; set; }
    public string Status { get; set; }
    public string Description { get; set; }


    public const string TypeExecution = "Execution";

    public const string StatusFailure = "FAILED";
    public const string StatusSuccess = "SUCCESS";
        
    public const string NotApplicable = "N/A";
    
    
    public static TestInputResult CreateExecutionFailure(string description)
    {
        return new TestInputResult()
        {
            Type = TypeExecution,
            Description = description,
            Field = NotApplicable,
            Status = StatusFailure
        };
    }

    public static TestInputResult CreateExecutionSuccess(string description)
    {
        return new TestInputResult()
        {
            Type = TypeExecution,
            Description = description,
            Field = NotApplicable,
            Status = StatusSuccess
        };
    }
}