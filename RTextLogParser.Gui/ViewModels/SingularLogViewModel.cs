using System;
using ReactiveUI;
using RTextLogParser.Library;

namespace RTextLogParser.Gui.ViewModels;

public class SingularLogViewModel : ViewModelBase
{
    private readonly LogElement _logElement;
    private bool _isExpanded = false;
    public bool IsHidden { get; set; } = false;

    public bool IsExpanded
    {
        get => CanExpand && _isExpanded;
        set => _isExpanded = value;
    }
    
    public bool IsNotExpanded
    {
        get => CanExpand && !_isExpanded;
        set => _isExpanded = value;
    }


    public bool CanExpand { get; set; } = false;

    public SingularLogViewModel()
    {
        _logElement = new LogElement($"Test log{Environment.NewLine}Second line of log",
            new string[] { "first group" }, 1);
    }

    public SingularLogViewModel(LogElement logElement)
    {
        _logElement = logElement;
    }
    
    public SingularLogViewModel(LogElement logElement, bool canExpand)
    {
        _logElement = logElement;
        CanExpand = canExpand;
    }

    public string Log => _logElement.Log;
}
