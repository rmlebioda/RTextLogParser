using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RTextLogParser.Library;

namespace RTextLogParser.Gui.ViewModels;

public class LogListViewModel
{
    public ObservableCollection<SingularLogViewModel> LogsViewModels { get; } = new ObservableCollection<SingularLogViewModel>();

    private static readonly Regex LogRegex = new Regex(@"\[(.+?)\]\s*(\w+)\s*(.)(\s+)(.*?)(?=\r?\n\[)", RegexOptions.Singleline);

    public LogListViewModel()
    {
    }

    public async Task LoadFileAsync(string filePath)
    {
        LogsViewModels.Clear();
        var parser = new LogParser(filePath, LogRegex);
        await foreach (var log in parser.GetLogsAsync())
        {
            LogsViewModels.Add(new SingularLogViewModel(log));
        }
    }
}