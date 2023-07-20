namespace RTextLogParser.Gui.ViewModels.TestIndentEvaluation;

public class TestIndentEvaluationViewModel : ViewModelBase
{
    public string LookupRegex { get; set; }

    public TestIndentEvaluationViewModel()
    {
        LookupRegex = @"\w+";
    }

    public TestIndentEvaluationViewModel(string lookupRegex)
    {
        LookupRegex = lookupRegex;
    }
}