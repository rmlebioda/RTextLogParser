using System.Linq;
using System.Runtime.Serialization;
using Avalonia.Themes.Fluent;
using ReactiveUI;
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
    
    public void Apply(Settings settings)
    {
        Settings = settings;
        SetAppMode(settings.DarkMode);
    }

    public void SetAppMode(bool dark)
    {
        var fluentTheme = _app?.Styles.OfType<FluentTheme>().FirstOrDefault();
        if (fluentTheme is not null)
        {
            fluentTheme.Mode = dark ? FluentThemeMode.Dark : FluentThemeMode.Light;
        }
    }
}