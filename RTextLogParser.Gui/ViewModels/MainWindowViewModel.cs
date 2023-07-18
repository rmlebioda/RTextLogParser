using System;
using ReactiveUI;
using RTextLogParser.Gui.DataPersistence;
using RTextLogParser.Gui.Views;

namespace RTextLogParser.Gui.ViewModels;

public class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly MainWindow _mainWindow;
    public AppState AppState { get; } = AppState.Retrieve();
    public ViewModelBase? _currentPageViewModel;

    public ViewModelBase? CurrentPageViewModel
    {
        get => _currentPageViewModel;
        set => this.RaiseAndSetIfChanged(ref _currentPageViewModel, value);
    }

    [Obsolete("Do not call, to be used only by designer")]
    /// <summary>
    /// Do not call, to be used only by designer
    /// </summary>
    public MainWindowViewModel()
    {
        _mainWindow = new MainWindow();
    }
    
    public MainWindowViewModel(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
        Initialize();
        AppState.Retrieve().SetMainView(_mainWindow);
    }

    private void Initialize()
    {
        AppState.CurrentViewModelChanged += CurrentViewModelChanged;
    }

    private void CurrentViewModelChanged(ViewModelBase? newViewModel)
    {
        CurrentPageViewModel = newViewModel;
    }

    public void Dispose()
    {
        AppState.CurrentViewModelChanged -= CurrentViewModelChanged;
    }
}