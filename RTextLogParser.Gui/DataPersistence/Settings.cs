using System.Collections.Generic;
using ReactiveUI;
using RTextLogParser.Gui.Models;
using Serilog;

namespace RTextLogParser.Gui.DataPersistence;

public class Settings
{
    public string LookupRegex { get; set; } = string.Empty;
    public ThemeMode Theme { get; set; } = ThemeMode.Default;
    public ListType ListType { get; set; } = ListType.ListBox;
    public long IndentGroupId { get; set; } = 0;
    public string IndentLevelEvaluation { get; set; } = string.Empty;
    public List<RegexGroupDefinition> RegexGroups { get; set; } = new();
}

public enum ThemeMode
{
    Default = 0,
    Light,
    Dark
}

public enum ListType
{
    ListBox = 0,
    TreeDataGrid
}
