using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using ReactiveUI;
using RTextLogParser.Gui.DataPersistence;
using RTextLogParser.Gui.Models;
using RTextLogParser.Gui.Views;
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
    }

    public MainViewModel(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
        _windowActions = new MainWindowActions(LoadFileAsync, CancelLoadingFile);
        CustomizationPanelViewModel = new CustomizationPanelViewModel(_windowActions);
        ResetSavedStateCommand = ReactiveCommand.Create(ResetSavedState);
    }

    public CustomizationPanelViewModel CustomizationPanelViewModel { get; }
    public LogListViewModel LogListViewModel { get; } = new LogListViewModel();
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
                await LogListViewModel.LoadFileAsync(fileToLoad, _cancellationTokenSource.Token);
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

    private void ResetSavedState()
    {
        Log.Information("Resetting settings");
        AppState.Retrieve().Settings = new Settings();
    }
}