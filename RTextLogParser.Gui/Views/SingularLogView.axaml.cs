using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RTextLogParser.Gui.ViewModels;

namespace RTextLogParser.Gui.Views;

public partial class SingularLogView : UserControl
{
    public SingularLogView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void LogLabel_OnTapped(object? sender, RoutedEventArgs e)
    {
        (this.DataContext as SingularLogViewModel)?.ToggledExpandLog();
    }
}