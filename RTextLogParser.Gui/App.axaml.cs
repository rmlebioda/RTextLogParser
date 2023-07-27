using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Newtonsoft.Json;
using ReactiveUI;
using RTextLogParser.Gui.DataPersistence;
using RTextLogParser.Gui.Models;
using RTextLogParser.Gui.ViewModels;
using RTextLogParser.Gui.Views;
using Serilog;

namespace RTextLogParser.Gui
{
    public partial class App : Application
    {
        private AutoSuspendHelper? _autoSuspendHelper;
        
        public override void Initialize()
        {
            _autoSuspendHelper = new AutoSuspendHelper(ApplicationLifetime!);
            var suspensionDriver = DataSuspensionDriver<AppState>.AppStateNewInstance;
            RxApp.SuspensionHost.CreateNewAppState = CreateNewAppState;
            RxApp.SuspensionHost.SetupDefaultSuspendResume(suspensionDriver);
            RxApp.SuspensionHost.AppState ??= LoadAppState(suspensionDriver);
            AvaloniaXamlLoader.Load(this);
        }

        private object CreateNewAppState()
        {
            Log.Information("Creating new app state");
            return new AppState();
        }

        private object LoadAppState(DataSuspensionDriver<AppState> suspensionDriver)
        {
            try
            {
                return suspensionDriver.LoadState().Wait();
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                return CreateNewAppState();
            }
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                Log.Debug("Serialized settings: {Settings}", JsonConvert.SerializeObject(AppState.Retrieve().Settings));
                var mainWindow = new MainWindow();
                var mainWindowViewModel = new MainWindowViewModel(mainWindow);
                mainWindow.DataContext = mainWindowViewModel;
                desktop.MainWindow = mainWindow;
                var appState = AppState.Retrieve();
                appState.SetApp(this);
                if (appState.Settings?.Theme is { } theme)
                {
                    appState.SetAppMode(theme);
                }
                else
                {
                    appState.SetAppMode(ThemeMode.Default);
                }
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}