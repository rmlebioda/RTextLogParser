using System;
using System.Threading.Tasks;

namespace RTextLogParser.Gui.Models;

public class MainWindowActions
{
    public readonly Func<Task> LoadFileAsync;

    public MainWindowActions(Func<Task> loadFileAsync)
    {
        LoadFileAsync = loadFileAsync;
    }
}