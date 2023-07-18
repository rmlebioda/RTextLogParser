using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Interactivity;
using Newtonsoft.Json;
using ReactiveUI;
using RTextLogParser.Gui.DataPersistence;
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
    public string LightText { get; init; } = "Light";
    public string DarkText { get; init; } = "Dark";
    public string RegexGroupsTitle { get; init; } = "Regular expression groups definition";
    public string AddRegexGroupText { get; init; } = "Add new group";
    public string DeleteSelectedRegexGroupText { get; init; } = "Delete selected group";
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
    public ReactiveCommand<Unit, Unit> CustomFileLoggerCommand { get; }
    public ReactiveCommand<Unit, Unit> LightModeCommand { get; }
    public ReactiveCommand<Unit, Unit> DarkModeCommand { get; }
    public ReactiveCommand<Unit, Unit> AddNewRegexGroupCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteSelectedRegexGroupCommand { get; }

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


    public SettingsViewModel()
    {
        SaveCommand = ReactiveCommand.Create(Save);
        CancelCommand = ReactiveCommand.Create(Cancel);
        CustomFileLoggerCommand = ReactiveCommand.Create(CustomFileLoggerPreset);
        LightModeCommand = ReactiveCommand.Create(LightModeChecked);
        DarkModeCommand = ReactiveCommand.Create(DarkModeChecked);
        AddNewRegexGroupCommand = ReactiveCommand.Create(AddNewRegexGroup);
        DeleteSelectedRegexGroupCommand = ReactiveCommand.Create(DeleteSelectedRegexGroup);
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
        LookupRegex = @"\[(.+?)\]\s*(\w+)\s*(.)(\s+)(.*?)(?=\r?\n\[)";
    }

    private void SaveSettings()
    {
        AppState.Retrieve().Settings = CurrentSettings;
    }

    private void LightModeChecked()
    {
        SetAppMode(dark: false);
    }

    private void DarkModeChecked()
    {
        SetAppMode(dark: true);
    }

    private void SetAppMode(bool dark)
    {
        AppState.Retrieve().SetAppMode(dark);
    }
}