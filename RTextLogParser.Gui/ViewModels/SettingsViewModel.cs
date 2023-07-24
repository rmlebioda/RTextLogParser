using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;
using ReactiveUI;
using RTextLogParser.Gui.DataPersistence;
using RTextLogParser.Gui.Models;
using RTextLogParser.Library;
using RTextLogParser.Library.Core;
using Serilog;

namespace RTextLogParser.Gui.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    public string SaveButtonText { get; init; } = "Save";
    public string CancelButtonText { get; init; } = "Cancel";
    public string PresetsText { get; init; } = "Presets:";
    public string CustomFileLoggerText { get; init; } = "Custom file logger";
    public string RegexText { get; init; } = "Line detection regular expression:";
    public string ThemeText { get; init; } = "Theme:";
    public string DefaultText { get; init; } = "Default";
    public string LightText { get; init; } = "Light";
    public string DarkText { get; init; } = "Dark";
    public string ListTypeText { get; init; } = "List type:";
    public string RegexGroupsTitle { get; init; } = "Regular expression groups definition";
    public string AddRegexGroupText { get; init; } = "Add new group";
    public string DeleteSelectedRegexGroupText { get; init; } = "Delete selected group";
    public string ScopeDetectionText { get; init; } = "Scope detection, group ID:";
    public string IndentEvaluationText { get; init; } = "Indentation C# evaluation code (input variable is called `Input`, example: `Input.Count(c => c == ' ')`):";
    public string IndentEvaluationTestButtonText { get; init; } = "Test";
    public string TestExampleInputText { get; init; } = "Verify settings with example data and press \"Test\" button:";
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
    public ReactiveCommand<Unit, Unit> CustomFileLoggerCommand { get; }
    public ReactiveCommand<Unit, Unit> DefaultModeCommand { get; }
    public ReactiveCommand<Unit, Unit> LightModeCommand { get; }
    public ReactiveCommand<Unit, Unit> DarkModeCommand { get; }
    public ReactiveCommand<Unit, Unit> AddNewRegexGroupCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteSelectedRegexGroupCommand { get; }
    public ReactiveCommand<Unit, Unit> IndentEvaluationTestCommand { get; }

    public ThemeMode[] AllThemes { get; } = (ThemeMode[])Enum.GetValues(typeof(ThemeMode));
    public ThemeMode SelectedTheme
    {
        get => CurrentSettings.Theme;
        set
        {
            CurrentSettings.Theme = value;
            SetAppMode(value);
        }
    }
    public ListType[] AllListTypes { get; } = (ListType[])Enum.GetValues(typeof(ListType));
    public ListType SelectedListType
    {
        get => CurrentSettings.ListType;
        set
        {
            CurrentSettings.ListType = value;
        }
    }
    
    private Settings _currentSettings { get; set; }

    private Settings CurrentSettings
    {
        get => _currentSettings;
        set
        {
            _currentSettings = value;
            LookupRegex = _currentSettings.LookupRegex;
            RegexGroups = new ObservableCollection<RegexGroupDefinition>(_currentSettings.RegexGroups);
        }
    }

    public string LookupRegex
    {
        get => CurrentSettings.LookupRegex;
        set
        {
            CurrentSettings.LookupRegex = value;
            this.RaisePropertyChanged();
        }
    }

    private ObservableCollection<RegexGroupDefinition> _regexGroups;
    public ObservableCollection<RegexGroupDefinition> RegexGroups
    {
        get => _regexGroups;
        set
        {
            _regexGroups = value;
            CurrentSettings.RegexGroups = value.ToList();
            this.RaisePropertyChanged();
            _regexGroups.CollectionChanged += RegexGroupChanged;
        }
    }
    public RegexGroupDefinition? SelectedRegexGroup { get; set; }

    private void RegexGroupChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        CurrentSettings.RegexGroups = _regexGroups.ToList();
    }

    public long IndentGroupId
    {
        get => CurrentSettings.IndentGroupId;
        set
        {
            CurrentSettings.IndentGroupId = value;
            this.RaisePropertyChanged();
        }
    }

    public string IndentLevelEvaluation
    {
        get => CurrentSettings.IndentLevelEvaluation;
        set
        {
            CurrentSettings.IndentLevelEvaluation = value;
            this.RaisePropertyChanged();
        }
    }
    
    public string TestExampleInput { get; set; }
    
    private ObservableCollection<TestInputResult> _testInputResults = new();
    public ObservableCollection<TestInputResult> TestInputResults
    {
        get => _testInputResults;
        set
        {
            _testInputResults = value;
            this.RaisePropertyChanged();
        }
    }

    public SettingsViewModel()
    {
        SaveCommand = ReactiveCommand.Create(Save);
        CancelCommand = ReactiveCommand.Create(Cancel);
        CustomFileLoggerCommand = ReactiveCommand.Create(CustomFileLoggerPreset);
        DefaultModeCommand = ReactiveCommand.Create(DefaultModeChecked);
        LightModeCommand = ReactiveCommand.Create(LightModeChecked);
        DarkModeCommand = ReactiveCommand.Create(DarkModeChecked);
        AddNewRegexGroupCommand = ReactiveCommand.Create(AddNewRegexGroup);
        DeleteSelectedRegexGroupCommand = ReactiveCommand.Create(DeleteSelectedRegexGroup);
        IndentEvaluationTestCommand = ReactiveCommand.CreateFromTask(IndentEvaluationTest);
    }

    private async Task IndentEvaluationTest()
    {
        TestInputResults.Clear();
        List<LogElement> logs;
        try
        {
            using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(TestExampleInput));
            var regex = new Regex(LookupRegex);
            var indentEvaluationSettings = new IndentEvaluationSettings()
            {
                EvaluationString = IndentLevelEvaluation,
                GroupId = IndentGroupId
            };
            var parser = new LogParser(memoryStream, regex, indentEvaluationSettings);
            logs = await parser.GetLogsAsync().ToListAsync();
        }
        catch (CompilationErrorException e)
        {
            TestInputResults.Add(TestInputResult.CreateExecutionFailure("Invalid indent detection: " + e));
            return;
        }
        catch (Exception e)
        {
            TestInputResults.Add(TestInputResult.CreateExecutionFailure(e.ToString()));
            return;
        }
        TestInputResults.Add(TestInputResult.CreateExecutionSuccess($"Read logs count: {logs.Count}"));
        AddLogsToResults(logs);
    }

    private void AddLogsToResults(List<LogElement> logElements)
    {
        for (int i = 0; i < logElements.Count; i++)
        {
            var log = logElements[i];
            var testType = "Match " + i;
            var firstMatch = new TestInputResult()
            {
                Type = testType,
                Field = "Full match",
                Status = TestInputResult.NotApplicable,
                Description = log.Log
            };
            TestInputResults.Add(firstMatch);

            var indents = new TestInputResult()
            {
                Type = testType,
                Field = "Indent",
                Status = TestInputResult.NotApplicable,
                Description = log.Indent.ToString()
            };
            TestInputResults.Add(indents);

            for (int j = 0; j < log.RegexGroups.Length; j++)
            {
                var match = new TestInputResult()
                {
                    Type = testType,
                    Field = j.ToString(),
                    Status = TestInputResult.NotApplicable,
                    Description = log.RegexGroups[j]
                };
                TestInputResults.Add(match);
            }
        }
    }
    
    public void ReloadSettings()
    {
        CurrentSettings = AppState.Retrieve().Settings?.Copy() ?? new Settings();
        Log.Debug("Settings data: {Settings}", JsonConvert.SerializeObject(CurrentSettings));
    }

    private void Save()
    {
        Log.Information("Saving made settings changes");
        SaveSettings();
        AppState.Retrieve().SetMainView();
    }

    private void Cancel()
    {
        Log.Information("Cancelling made settings changes");
        AppState.Retrieve().ReloadSettings();
        AppState.Retrieve().SetMainView();
    }

    private void AddNewRegexGroup()
    {
        RegexGroups.Add(new RegexGroupDefinition());
    }

    private void DeleteSelectedRegexGroup()
    {
        if (SelectedRegexGroup is not null)
            RegexGroups.Remove(SelectedRegexGroup);
    }

    private void CustomFileLoggerPreset()
    {
        LookupRegex = @"\[(.+?)\]\s*(\w+)\s*(.)(\s+)(.*?)(?:(?=\r?\n\[)|$)";
        RegexGroups = new ObservableCollection<RegexGroupDefinition>()
        {
            new()
            {
                FieldIndex = 0,
                GroupTitle = "Date",
                IsEnabled = true,
                IsExpanding = true
            },
            new()
            {
                FieldIndex = 1,
                GroupTitle = "Error level",
                IsEnabled = true,
                IsExpanding = false
            },
            new()
            {
                FieldIndex = 2,
                GroupTitle = "Separator",
                IsEnabled = true,
                IsExpanding = false
            },
            new()
            {
                FieldIndex = 3,
                GroupTitle = "Indents",
                IsEnabled = true,
                IsExpanding = false
            },
            new()
            {
                FieldIndex = 4,
                GroupTitle = "Details",
                IsEnabled = true,
                IsExpanding = false
            }
        };
        IndentGroupId = 3;
        IndentLevelEvaluation = "return Input.Count(c => c == ' ')/2;";
    }

    private void SaveSettings()
    {
        AppState.Retrieve().Settings = CurrentSettings;
    }

    private void DefaultModeChecked()
    {
        SetAppMode(ThemeMode.Default);
    }
    private void LightModeChecked()
    {
        SetAppMode(ThemeMode.Light);
    }

    private void DarkModeChecked()
    {
        SetAppMode(ThemeMode.Dark);
    }

    private void SetAppMode(ThemeMode theme)
    {
        AppState.Retrieve().SetAppMode(theme);
    }
}