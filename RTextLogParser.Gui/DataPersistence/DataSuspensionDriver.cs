using System;
using System.Reactive;
using Akavache;
using Newtonsoft.Json;
using ReactiveUI;
using Serilog;

namespace RTextLogParser.Gui.DataPersistence;

public class DataSuspensionDriver<TAppState> : ISuspensionDriver where TAppState : class
{
    private const string AppStateKey = "appState";
  
    public DataSuspensionDriver() => BlobCache.ApplicationName = "RTextLogParser";

    public static DataSuspensionDriver<AppState> AppStateNewInstance => new DataSuspensionDriver<AppState>();

    public IObservable<Unit> InvalidateState()
    {
        Log.Warning("Invalidating state");
        return BlobCache.UserAccount.InvalidateObject<TAppState>(AppStateKey);
    }

    public IObservable<object> LoadState()
    {
        Log.Debug("Loading application state");
        var state = BlobCache.UserAccount.GetObject<TAppState>(AppStateKey);
        Log.Verbose("Loaded state: {State}", JsonConvert.SerializeObject(state));
        return state;
    }

    public IObservable<Unit> SaveState(object state)
    {
        Log.Debug("Saving application state: {State}", JsonConvert.SerializeObject(state));
        return BlobCache.UserAccount.InsertObject(AppStateKey, (TAppState)state);
    }
}