using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

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
}