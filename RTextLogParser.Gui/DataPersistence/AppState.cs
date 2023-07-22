using System.Linq;
using System.Runtime.Serialization;
using Avalonia;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using ReactiveUI;
using RTextLogParser.Gui.Models;
using RTextLogParser.Gui.ViewModels;
using RTextLogParser.Gui.Views;

namespace RTextLogParser.Gui.DataPersistence;

[DataContract]
public class AppState
{
    #region Persistence fields
    [DataMember] private Settings? _settings = null;
    #endregion

    public Settings? Settings
    {
        get => _settings;
        set
        {
            _settings = value;
            SettingsChanged?.Invoke(_settings);
        }
    }

    public bool AreSettingsSet => Settings != null;

    private ViewModelBase? _currentPageViewModel;
    private App? _app;
    public ViewModelBase? CurrentPageViewModel
    {
        get => _currentPageViewModel;
        set
        {
            _currentPageViewModel = value;
            CurrentViewModelChanged?.Invoke(_currentPageViewModel);
        }
    }

    public void SetApp(App app)
    {
        _app = app;
    }
    
    public static AppState Retrieve() => RxApp.SuspensionHost.GetAppState<AppState>();

    private MainViewModel? _mainViewModel;
    private MainWindow? _mainWindow;
    public MainWindow? MainWindow => _mainWindow;
    private SettingsViewModel? _settingsViewModel;

    public delegate void CurrentViewModelChange(ViewModelBase? newViewModel);
    public event CurrentViewModelChange CurrentViewModelChanged;
    public delegate void SettingsChange(Settings? newSettings);
    public event SettingsChange SettingsChanged;
    
    public MainViewModel SetMainView(MainWindow mainWindow)
    {
        _mainWindow ??= mainWindow;
        return SetMainView();
    }

    public MainViewModel SetMainView()
    {
        _mainViewModel ??= new MainViewModel(_mainWindow!);
        CurrentPageViewModel = _mainViewModel;
        return _mainViewModel;
    }

    public SettingsViewModel SetSettingsView()
    {
        _settingsViewModel ??= new SettingsViewModel();
        _settingsViewModel.ReloadSettings();
        CurrentPageViewModel = _settingsViewModel;
        return _settingsViewModel;
    }

    public void ReloadSettings()
    {
        Apply(Settings!);
    }

    private void Apply(Settings settings)
    {
        Settings = settings;
        SetAppMode(settings.Theme);
    }

    public void SetAppMode(ThemeMode theme)
    {
        _app!.RequestedThemeVariant = theme switch
        {
            ThemeMode.Dark => ThemeVariant.Dark,
            ThemeMode.Light => ThemeVariant.Light,
            _ => ThemeVariant.Default
        };
    }
}