using System;
using System.Threading.Tasks;

namespace RTextLogParser.Gui.Models;

public class MainWindowActions
{
    public readonly Func<Task> LoadFileAsync;
    public readonly Action CancelLoadingFile;

    public MainWindowActions(Func<Task> loadFileAsync, Action cancelLoadingFile)
    {
        LoadFileAsync = loadFileAsync;
        CancelLoadingFile = cancelLoadingFile;
    }
}