using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DynamicData;
using RTextLogParser.Library;
using Serilog;

namespace RTextLogParser.Gui.ViewModels;

public class LogListViewModel : ViewModelBase
{
    private static readonly Regex LogRegex = new(@"\[(.+?)\]\s*(\w+)\s*(.)(\s+)(.*?)(?=\r?\n\[)",
        RegexOptions.Singleline);

    public ObservableCollection<SingularLogViewModel> LogsViewModels { get; } = new();

    public async Task LoadFileAsync(string filePath, CancellationToken? cancellationToken = null)
    {
        LogsViewModels.Clear();
        var parser = new LogParser(filePath, LogRegex);
        var logsVm = new List<SingularLogViewModel>();
        var stopwatch = Stopwatch.StartNew();
        await foreach (var log in parser.GetLogsAsync(cancellationToken))
        {
            logsVm.Add(new SingularLogViewModel(log));
            if (stopwatch.ElapsedMilliseconds > 1000)
                AddPendingLogs();
        }

        AddPendingLogs();

        void AddPendingLogs()
        {
            Log.Debug("Elapsed {ElapsedMs} ms, adding {Elements} elements",
                stopwatch!.ElapsedMilliseconds, logsVm.Count);
            LogsViewModels.AddRange(logsVm!);
            logsVm!.Clear();
            stopwatch.Restart();
        }
    }
}