using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RTextLogParser.Gui.Views.TestIndentEvaluation;

public partial class TestIndentEvaluationWindow : Window
{
    public TestIndentEvaluationWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}