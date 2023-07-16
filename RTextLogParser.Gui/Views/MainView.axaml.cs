using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using RTextLogParser.Gui.DataPersistence;

namespace RTextLogParser.Gui.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}