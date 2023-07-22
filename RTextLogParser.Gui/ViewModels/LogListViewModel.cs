using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DynamicData;
using RTextLogParser.Gui.DataPersistence;
using RTextLogParser.Library;
using RTextLogParser.Library.Core;
using Serilog;

namespace RTextLogParser.Gui.ViewModels;

public class LogListViewModel : ViewModelBase
{
    public ObservableCollection<SingularLogViewModel> LogsViewModels { get; } = new();

    public async Task LoadFileAsync(string filePath, CancellationToken? cancellationToken = null)
    {
        LogsViewModels.Clear();
        var settings = AppState.Retrieve().Settings!;
        var evaluationSettings = new IndentEvaluationSettings()
        {
            EvaluationString = settings.IndentLevelEvaluation,
            GroupId = settings.IndentGroupId
        };
        var parser = new LogParser(filePath, new Regex(settings.LookupRegex), evaluationSettings);
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
            stopwatch.Restart();
            foreach (var logsVmGroup in logsVm!.Chunk(10000))
            {
                LogsViewModels.AddRange(logsVmGroup);
            }
            logsVm!.Clear();
            Log.Debug("Adding took {ElapsedMs} ms", stopwatch!.ElapsedMilliseconds);
            stopwatch.Restart();
        }
    }
}