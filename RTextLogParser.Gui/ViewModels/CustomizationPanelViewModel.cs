using System;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using ReactiveUI;
using RTextLogParser.Gui.DataPersistence;
using RTextLogParser.Gui.Models;

namespace RTextLogParser.Gui.ViewModels;

public class CustomizationPanelViewModel : ViewModelBase
{
    private readonly MainWindowActions _windowActions;
    public AppState AppState { get; }

    public CustomizationPanelViewModel()
    {
        if (!Design.IsDesignMode)
            throw new ApplicationException("Parameterless constructor of this ViewModel should be called only by designer");
        _windowActions = new MainWindowActions(() => Task.CompletedTask, () => { });
        ChosenFileCommand = ReactiveCommand.CreateFromTask(_windowActions.LoadFileAsync);
        ChosenFileCommand.IsExecuting.ToProperty(this, x => x.IsBusy, out _isBusy);
        SettingsCommand = ReactiveCommand.Create(RunSettings);
        StopLoading = ReactiveCommand.Create(_windowActions.CancelLoadingFile);
        AppState = new AppState();
    }

    public CustomizationPanelViewModel(MainWindowActions windowActions)
    {
        _windowActions = windowActions;
        ChosenFileCommand = ReactiveCommand.CreateFromTask(_windowActions.LoadFileAsync);
        ChosenFileCommand.IsExecuting.ToProperty(this, x => x.IsBusy, out _isBusy);
        SettingsCommand = ReactiveCommand.Create(RunSettings);
        StopLoading = ReactiveCommand.Create(_windowActions.CancelLoadingFile);
        AppState = AppState.Retrieve();
    }

    internal void UpdateFilePath(string path)
    {
        SelectedFile = path;
    }

    private void RunSettings()
    {
        AppState.Retrieve().SetSettingsView();
    }
    
    public string ChooseLogFileText => "Choose log file";
    public ReactiveCommand<Unit, Unit> ChosenFileCommand { get; }
    public ReactiveCommand<Unit, Unit> SettingsCommand { get; }
    public ReactiveCommand<Unit, Unit> StopLoading { get; }
    
    private string _selectedFile = string.Empty;

    public string SelectedFile
    {
        get => _selectedFile;
        set => this.RaiseAndSetIfChanged(ref _selectedFile, value);
    }

    public string SettingsText => "Settings";
    public string StopText => "Stop";
    
    private readonly ObservableAsPropertyHelper<bool> _isBusy;
    public bool IsBusy => _isBusy.Value;
}