using System;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using ReactiveUI;
using RTextLogParser.Gui.DataPersistence;
using RTextLogParser.Gui.Models;

namespace RTextLogParser.Gui.ViewModels;

public class CustomizationPanelViewModel : ViewModelBase, IDisposable
{
    private readonly MainWindowActions _windowActions;

    public CustomizationPanelViewModel()
    {
        if (!Design.IsDesignMode)
            throw new ApplicationException("Parameterless constructor of this ViewModel should be called only by designer");
        _windowActions = new MainWindowActions(() => Task.CompletedTask, () => { });
        ChosenFileCommand = ReactiveCommand.CreateFromTask(_windowActions.LoadFileAsync);
        ChosenFileCommand.IsExecuting.ToProperty(this, x => x.IsBusy, out _isBusy);
        SettingsCommand = ReactiveCommand.Create(RunSettings);
        StopLoading = ReactiveCommand.Create(_windowActions.CancelLoadingFile);
        Initialize();
    }

    public CustomizationPanelViewModel(MainWindowActions windowActions)
    {
        _windowActions = windowActions;
        ChosenFileCommand = ReactiveCommand.CreateFromTask(_windowActions.LoadFileAsync);
        ChosenFileCommand.IsExecuting.ToProperty(this, x => x.IsBusy, out _isBusy);
        SettingsCommand = ReactiveCommand.Create(RunSettings);
        StopLoading = ReactiveCommand.Create(_windowActions.CancelLoadingFile);
        Initialize();
    }

    private void Initialize()
    {
        AppState.Retrieve().SettingsChanged += SettingsChanged;
        SettingsChanged(AppState.Retrieve().Settings);
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
    
    public bool AreSettingsSet { get; set; }

    public void SettingsChanged(Settings? settings)
    {
        AreSettingsSet = settings is not null;
    }
    
    public void Dispose()
    {
        AppState.Retrieve().SettingsChanged -= SettingsChanged;
    }
}