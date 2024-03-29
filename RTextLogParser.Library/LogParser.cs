﻿using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using RTextLogParser.Library.Core;
using RTextLogParser.Library.Exceptions;
using RTextLogParser.Library.Utils;

namespace RTextLogParser.Library;

/// <summary>
/// Class responsible for fast loading logs into memory based on custom regex.
/// </summary>
public class LogParser
{
    private const int BufferSize = 4096;

    private List<LogElement> _listOfSingularLogs = new();
    private Regex _logRegex;
    private Regex? _lastLogRegex;
    private string? _logPath;
    private FileStream? _logFileStream;
    private Stream? _logStream;
    private StringBuilder _readStringBuffer = new();
    private readonly byte[] _buffer = new byte[BufferSize];
    private Encoding? _encoding;
    private int _returnedLogLines;
    private bool _didEncounterEoF;
    private IndentEvaluationSettings? _indentEvaluationSettings;
    private Script<long>? _evaluationScript;

    private const long IndentNotAvailableValue = -1;
    
    /// <summary>
    /// Initializes class for reading.
    /// </summary>
    /// <param name="logPath">Path to log file</param>
    /// <param name="logRegex">Regex matching singular log</param>
    public LogParser(string logPath, Regex logRegex, IndentEvaluationSettings? indentEvaluationSettings = null, Regex? lastLogRegex = null)
    {
        _logRegex = logRegex;
        _logPath = logPath;
        _lastLogRegex = lastLogRegex;
        _indentEvaluationSettings = indentEvaluationSettings;
        InitEvaluationScript();
    }

    public LogParser(Stream logStream, Regex logRegex, IndentEvaluationSettings? indentEvaluationSettings = null, Regex? lastLogRegex = null, Encoding? encoding = null)
    {
        _logStream = logStream;
        _logRegex = logRegex;
        _lastLogRegex = lastLogRegex;
        _indentEvaluationSettings = indentEvaluationSettings;
        _encoding = encoding ?? Encoding.UTF8;
        InitEvaluationScript();
    }

    private void InitEvaluationScript()
    {
        if (_indentEvaluationSettings is null)
            return;

        var stopwatch = new Stopwatch();
        _evaluationScript = CSharpScript
            .Create<long>(_indentEvaluationSettings!.EvaluationString, globalsType: typeof(InputScript))
            .WithOptions(ScriptOptions.Default
                .WithImports("System.Linq")
                .AddReferences(typeof(System.Linq.Enumerable).Assembly));
        _evaluationScript.Compile();
    }

    private void EnsureFileIsOpened()
    {
        if (_logPath is null)
            return;;
        
        _encoding ??= EncodingUtils.GetEncoding(_logPath);
        _logFileStream ??= new FileStream(_logPath, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize,
            FileOptions.Asynchronous);
        _logStream = _logFileStream;
    }

    private async Task FindLogsInBuffer(bool useLastLogRegex = false)
    {
        int? lastCharacterOfAnyMatches = null;
        var buffer = _readStringBuffer.ToString();
        var regex = useLastLogRegex ? _lastLogRegex ?? _logRegex : _logRegex;
        foreach (Match match in regex.Matches(buffer))
        {
            if (match.Success)
            {
                var lastMatchGroup = match.Groups[match.Groups.Count - 1];
                var thisMatchStartIndex = match.Groups[0].Index;
                lastCharacterOfAnyMatches = lastMatchGroup.Index + lastMatchGroup.Length;
                if ((lastCharacterOfAnyMatches - thisMatchStartIndex) > match.Value.Length)
                    throw new InvalidRegexException(
                        $"Matched string length was smaller, than last matched group. This can happen, if you do capturing group inside lookaheads. Please rewrite regex, so that no matching group is inside lookaheads. Matched string: '{match.Value}', matched last group: '{match.Groups[match.Groups.Count - 1].Value}' at position {lastMatchGroup.Index}.");
                var logValue = match.Value.Substring(0, lastCharacterOfAnyMatches.Value - thisMatchStartIndex);
                var groups = match.Groups.Cast<Group>().Skip(1).Select(group => group.Value).ToArray();
                var indents = await EvaluateIndents(groups);
                _listOfSingularLogs.Add(new LogElement(logValue, groups, indents));
            }
        }

        if (lastCharacterOfAnyMatches != null)
        {
            _readStringBuffer.Clear();
            _readStringBuffer.Append(buffer.Substring(lastCharacterOfAnyMatches.Value));
        }
    }

    private async Task ReadFileAsync(bool firstMatchOnly, CancellationToken? cancellationToken = null)
    {
        EnsureFileIsOpened();
        var startLogsCount = _listOfSingularLogs.Count;

        var lastReadBytesCount = -1;
        do
        {
            lastReadBytesCount = await _logStream!.ReadAsync(_buffer);
            _readStringBuffer.Append(lastReadBytesCount == BufferSize
                ? _encoding!.GetString(_buffer)
                : _encoding!.GetString(_buffer.AsSpan(0, lastReadBytesCount)));
            await FindLogsInBuffer();
        } while (cancellationToken?.IsCancellationRequested != true &&
                 (!firstMatchOnly || _listOfSingularLogs.Count == startLogsCount) && lastReadBytesCount != 0);

        if (lastReadBytesCount == 0 && _readStringBuffer.Length > 0)
        {
            await FindLogsInBuffer(true);
            _didEncounterEoF = true;
        }
    }

    /// <summary>
    /// Returns amount of logs in the file.
    /// </summary>
    /// <param name="cancellationToken">Cancellation</param>
    /// <returns></returns>
    public async Task<int?> GetLogLinesAsync(CancellationToken? cancellationToken = null)
    {
        if (!_didEncounterEoF)
            await ReadFileAsync(false, cancellationToken);

        return cancellationToken is { IsCancellationRequested: true }
            ? null
            : _listOfSingularLogs.Count;
    }

    /// <summary>
    /// Gets next log in file, or null if file has been read or cancellation token was requested.
    /// </summary>
    /// <param name="cancellationToken">Cancellation of reading the file</param>
    /// <returns>Log or null in case of end of file or cancellation token request</returns>
    public async Task<LogElement?> ReadNextLogAsync(CancellationToken? cancellationToken = null)
    {
        if (_returnedLogLines <= _listOfSingularLogs.Count && !_didEncounterEoF)
            await ReadFileAsync(true, cancellationToken);

        if (cancellationToken?.IsCancellationRequested != true && _listOfSingularLogs.Count > _returnedLogLines)
        {
            _returnedLogLines++;
            return _listOfSingularLogs[_returnedLogLines - 1];
        }

        return null;
    }

    /// <summary>
    /// Resets returned log counter, next log will be the first one from file (this does not load whole file from beginning).
    /// </summary>
    public void ReadFromBeginning()
    {
        _returnedLogLines = 0;
    }

    public async IAsyncEnumerable<LogElement> GetLogsAsync(CancellationToken? cancellationToken = null)
    {
        ReadFromBeginning();

        while (true)
        {
            var log = await ReadNextLogAsync(cancellationToken);
            if (log is null)
                break;

            yield return log;

            if (cancellationToken?.IsCancellationRequested == true)
                break;
        }
    }

    private async Task<long> EvaluateIndents(string[] groups)
    {
        if (_indentEvaluationSettings is null || _indentEvaluationSettings.GroupId > groups.Length)
            return IndentNotAvailableValue;

        return (await _evaluationScript!.RunAsync(new InputScript
            { Input = groups[_indentEvaluationSettings.GroupId] })).ReturnValue;
    }
}