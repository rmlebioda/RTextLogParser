using System;
using System.Reactive;
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
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
    public ReactiveCommand<Unit, Unit> CustomFileLoggerCommand { get; }
    public ReactiveCommand<Unit, Unit> LightModeCommand { get; }
    public ReactiveCommand<Unit, Unit> DarkModeCommand { get; }

    private Settings CurrentSettings { get; set; }

    public SettingsViewModel()
    {
        SaveCommand = ReactiveCommand.Create(Save);
        CancelCommand = ReactiveCommand.Create(Cancel);
        CustomFileLoggerCommand = ReactiveCommand.Create(CustomFileLoggerPreset);
        LightModeCommand = ReactiveCommand.Create(LightModeChecked);
        DarkModeCommand = ReactiveCommand.Create(DarkModeChecked);
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

    private void CustomFileLoggerPreset()
    {
        CurrentSettings.LookupRegex = @"\[(.+?)\]\s*(\w+)\s*(.)(\s+)(.*?)(?=\r?\n\[)";
        this.RaisePropertyChanged(nameof(CurrentSettings));
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