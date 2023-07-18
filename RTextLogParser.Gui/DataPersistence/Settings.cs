using System.Collections.Generic;
using ReactiveUI;
using Serilog;

namespace RTextLogParser.Gui.DataPersistence;

public class Settings
{
    internal readonly static bool IsDarkModeDefault = true;
    public string LookupRegex { get; set; } = string.Empty;
    public bool DarkMode { get; set; } = IsDarkModeDefault;
    public long IndentGroupId { get; set; } = 0;
    public string IndentLevelEvaluation { get; set; } = string.Empty;
    public List<RegexGroupDefinition> RegexGroups { get; set; } = new();
}