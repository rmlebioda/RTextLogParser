using ReactiveUI;
using Serilog;

namespace RTextLogParser.Gui.DataPersistence;

public class Settings
{
    internal readonly static bool IsDarkModeDefault = true;
    public string LookupRegex { get; set; } = string.Empty;
    public bool DarkMode { get; set; } = IsDarkModeDefault;
}