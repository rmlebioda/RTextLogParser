using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RTextLogParser.Gui.Views;

public partial class LogListView : UserControl
{
    public LogListView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}