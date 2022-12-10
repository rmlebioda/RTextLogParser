using System;
using ReactiveUI;
using RTextLogParser.Library;

namespace RTextLogParser.Gui.ViewModels;

public class SingularLogViewModel : ViewModelBase
{
    private LogElement _logElement;

    public SingularLogViewModel()
    {
        _logElement = new LogElement($"Test log{Environment.NewLine}Second line of log",
            new string[] { "first group" });
    }

    public SingularLogViewModel(LogElement logElement)
    {
        _logElement = logElement;
    }

    public void ToggledExpandLog()
    {
        LogMaxLines = _logMaxLines == MaxLinesNonExpanded ? MaxLinesNoLimit : MaxLinesNonExpanded;
    }

    public string Log => _logElement.Log;
    private const int MaxLinesNonExpanded = 1;
    private const int MaxLinesNoLimit = 0;
    private int _logMaxLines = MaxLinesNonExpanded;

    public int LogMaxLines
    {
        get => _logMaxLines;
        set
        {
            this.RaiseAndSetIfChanged(ref _logMaxLines, value);
            this.RaisePropertyChanged(nameof(IsExpanded));
        }
    }

    public bool IsExpanded => LogMaxLines > 1 && LogMaxLines == MaxLinesNonExpanded;
}
