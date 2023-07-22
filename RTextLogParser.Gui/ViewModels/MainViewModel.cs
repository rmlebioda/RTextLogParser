using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using ReactiveUI;
using RTextLogParser.Gui.DataPersistence;
using RTextLogParser.Gui.Models;
using RTextLogParser.Gui.Views;
using RTextLogParser.Library;
using RTextLogParser.Library.Core;
using Serilog;

namespace RTextLogParser.Gui.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly MainWindow _mainWindow;
    private readonly MainWindowActions _windowActions;

    public ReactiveCommand<Unit, Unit> ResetSavedStateCommand { get; }
    
    [Obsolete("Do not call, to be used only by designer")]
    /// <summary>
    /// Do not call, to be used only by designer
    /// </summary>
    /// <exception cref="ApplicationException">When called</exception>
    public MainViewModel()
    {
        if (!Design.IsDesignMode)
            throw new ApplicationException("Parameterless constructor of this ViewModel should be called only by designer");
        _mainWindow = new MainWindow();
        _windowActions = new MainWindowActions(LoadFileAsync, CancelLoadingFile);
        CustomizationPanelViewModel = new CustomizationPanelViewModel(_windowActions);
        ResetSavedStateCommand = ReactiveCommand.Create(ResetSavedState);
        TreeDataGridSource = CreateTreeDataGridSource();
    }

    public MainViewModel(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
        _windowActions = new MainWindowActions(LoadFileAsync, CancelLoadingFile);
        CustomizationPanelViewModel = new CustomizationPanelViewModel(_windowActions);
        ResetSavedStateCommand = ReactiveCommand.Create(ResetSavedState);
        TreeDataGridSource = CreateTreeDataGridSource();
    }

    private HierarchicalTreeDataGridSource<LogElementExtended> CreateTreeDataGridSource()
    {
        var source = new HierarchicalTreeDataGridSource<LogElementExtended>(LogsSource);

        var regexGroups = AppState.Retrieve().Settings!.RegexGroups;
        foreach (var column in regexGroups.OrderBy(group => group.FieldIndex))
        {
            if (!column.IsEnabled)
                continue;
            if (column.IsExpanding)
            {
                source.Columns.Add(new HierarchicalExpanderColumn<LogElementExtended>(
                    new TextColumn<LogElementExtended, string>(column.GroupTitle,
                        log => log.RegexGroups[column.FieldIndex]), log => log.Children));
            }
            else
            {
                source.Columns.Add(new TextColumn<LogElementExtended, string>(column.GroupTitle,
                    log => log.RegexGroups[column.FieldIndex]));
            }
        }
        
        return source;
    }

    public CustomizationPanelViewModel CustomizationPanelViewModel { get; }
    public HierarchicalTreeDataGridSource<LogElementExtended> TreeDataGridSource { get; private set; }
    private CancellationTokenSource? _cancellationTokenSource;

    private void CancelLoadingFile()
    {
        Log.Information("Sending cancellation to loading file procedure");
        _cancellationTokenSource?.Cancel();
    }
    
    private async Task LoadFileAsync()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        Log.Information("Selecting new file");
        var newDialog = new OpenFileDialog
        {
            AllowMultiple = false,
            Title = "Select log file"
        };
        var files = await newDialog.ShowAsync(_mainWindow);
        if (files is { Length: > 0 })
        {
            var fileToLoad = files.First();
            try
            {
                Debug.Assert(files.Length == 1, "Expected only one file selected");
                Log.Information("Selected {filesCount} files: {filesArray}",
                    files.Length, string.Join(", ", files));
                CustomizationPanelViewModel.UpdateFilePath(fileToLoad);
                // await LogListViewModel.LoadFileAsync(fileToLoad, _cancellationTokenSource.Token);
                await LoadFileAsync(fileToLoad, _cancellationTokenSource.Token);
            }
            catch (Exception e)
            {
                Log.Error("Failed to load file {filePath} because of error: {e}", 
                    fileToLoad, e.ToString());
                throw;
            }
        }
        else
        {
            Log.Information("File was not selected");
        }
    }

    public class LogElementExtended : LogElement
    {
        public ObservableCollection<LogElementExtended> Children = new();
        
        public LogElementExtended(LogElement logElement) :
            base(logElement.Log, logElement.RegexGroups, logElement.Indent)
        {
        }
    }

    public ObservableCollection<LogElementExtended> LogsSource { get; set; } = new();

    public async Task LoadFileAsync(string filePath, CancellationToken? cancellationToken = null)
    {
        LogsSource.Clear();
        var settings = AppState.Retrieve().Settings!;
        var evaluationSettings = new IndentEvaluationSettings()
        {
            EvaluationString = settings.IndentLevelEvaluation,
            GroupId = settings.IndentGroupId
        };
        var parser = new LogParser(filePath, new Regex(settings.LookupRegex), evaluationSettings);
        var stopwatch = Stopwatch.StartNew();
        var pendingLogs = new List<LogElement>();
        await foreach (var log in parser.GetLogsAsync(cancellationToken))
        {
            pendingLogs.Add(log);
            if (stopwatch.ElapsedMilliseconds > 1000)
                AddPendingLogs();
        }

        AddPendingLogs();

        void AddPendingLogs()
        {
            Log.Debug("Elapsed {ElapsedMs} ms, adding {Elements} elements",
                stopwatch!.ElapsedMilliseconds, pendingLogs.Count);
            stopwatch.Restart();
            foreach (var log in pendingLogs)
            {
                if (LogsSource.Any() && log.Indent > LogsSource.Last().Indent)
                {
                    LogsSource.Last().Children.Add(new LogElementExtended(log));
                }
                else if (FindLastParentForIndent(log.Indent) is { } logElement)
                {
                    logElement.Children.Add(new LogElementExtended(log));
                }
                else
                {
                    LogsSource.Add(new LogElementExtended(log));
                }
            }
            // this.RaisePropertyChanged(nameof(TreeDataGridSource));
            // TreeDataGridSource = CreateTreeDataGridSource();
            pendingLogs.Clear();
            Log.Debug("Adding took {ElapsedMs} ms", stopwatch!.ElapsedMilliseconds);
            stopwatch.Restart();
        }

        LogElementExtended? FindLastParentForIndent(long indent)
        {
            return LogsSource.LastOrDefault(logElement => logElement.Indent < indent);
        }
    }

    private void ResetSavedState()
    {
        Log.Information("Resetting settings");
        AppState.Retrieve().Settings = new Settings();
    }
}
