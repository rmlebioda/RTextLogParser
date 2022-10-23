using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using RTextLogParser.Library.Utils;

namespace RTextLogParser.Library;

/// <summary>
/// Class responsible for fast loading logs into memory based on custom regex.
/// </summary>
public class LogParser
{
    private const int BufferSize = 4096;
    
    private List<LogElement> _listOfSingularLogs = new List<LogElement>();
    private Regex _logRegex;
    private string _logPath;
    private FileStream? _logFileStream; 
    private StringBuilder _readStringBuffer = new StringBuilder();
    private readonly byte[] _buffer = new byte[BufferSize];
    private Encoding? _fileEncoding;
    private int _returnedLogLines;
    private bool _didEncounterEoF;

    /// <summary>
    /// Initializes class for reading.
    /// </summary>
    /// <param name="logPath">Path to log file</param>
    /// <param name="logRegex">Regex matching singular log</param>
    public LogParser(string logPath, Regex logRegex)
    {
        _logRegex = logRegex;
        _logPath = logPath;
    }

    private void EnsureFileIsOpened()
    {
        _fileEncoding ??= EncodingUtils.GetEncoding(_logPath);
        _logFileStream ??= new FileStream(_logPath, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize,
            FileOptions.Asynchronous);
    }

    private void FindLogsInBuffer()
    {
        int? lastCharacterOfAnyMatches = null;
        var buffer = _readStringBuffer.ToString();
        foreach (Match match in _logRegex.Matches(buffer))
        {
            if (match.Success)
            {
                var lastMatchGroup = match.Groups[match.Groups.Count - 1];
                var thisMatchStartIndex = match.Groups[0].Index;
                lastCharacterOfAnyMatches = lastMatchGroup.Index + lastMatchGroup.Length;
                Debug.Assert((lastCharacterOfAnyMatches - thisMatchStartIndex) <= match.Value.Length, 
                    $"Length of match '{match.Value}' is smaller than last group end index {lastCharacterOfAnyMatches - thisMatchStartIndex}");
                // _listOfSingularLogs.Add(match.Value.Substring(0, lastCharacterOfAnyMatches.Value - thisMatchStartIndex));
                var value = match.Value.Substring(0, lastCharacterOfAnyMatches.Value - thisMatchStartIndex);
                _listOfSingularLogs.Add(new LogElement(value, ));
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
            lastReadBytesCount = await _logFileStream!.ReadAsync(_buffer);
            _readStringBuffer.Append(lastReadBytesCount == BufferSize
                ? _fileEncoding!.GetString(_buffer)
                : _fileEncoding!.GetString(_buffer.AsSpan(0, lastReadBytesCount)));
            FindLogsInBuffer();
        } while (cancellationToken?.IsCancellationRequested != true &&
                 (!firstMatchOnly || _listOfSingularLogs.Count == startLogsCount) && lastReadBytesCount != 0);

        if (lastReadBytesCount == 0 && _readStringBuffer.Length > 0)
        {
            _listOfSingularLogs.Add(_readStringBuffer.ToString().Trim());
            _readStringBuffer.Clear();
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
        
        return cancellationToken is {IsCancellationRequested: true}
            ? null
            : _listOfSingularLogs.Count;
    }

    /// <summary>
    /// Gets next log in file, or null if file has been read or cancellation token was requested.
    /// </summary>
    /// <param name="cancellationToken">Cancellation of reading the file</param>
    /// <returns>Log or null in case of end of file or cancellation token request</returns>
    public async Task<string?> ReadNextLogAsync(CancellationToken? cancellationToken = null)
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

    public async IAsyncEnumerable<string> GetLogsAsync(CancellationToken? cancellationToken = null)
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
}
