using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;

namespace RTextLogParser.Library.Tests;

[TestFixture]
public class SimpleLogTests
{
    private static string ThisDirectoryPath =>
        Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.FullName;

    private static string DataPath => Path.Combine(ThisDirectoryPath, "Data");
    private static string SimpleFileLogPath => Path.Combine(DataPath, "SimpleLogFile", "SimpleLogFile.txt");

    private static readonly Regex LogRegex =
        //new Regex(@"\[(.+?)\]\s*(\w+)\s*(.)(\s+)(.*?)(?=\r?\n\[)", RegexOptions.Singleline);
        new Regex(@"\[(.+?)\]\s*(\w+)\s*(.)(\s+)(.*?)(?=(\r?\n\[)|$)", RegexOptions.Singleline);

    private const int LogRegexGroupCount = 5;

    private LogParser _parser;

    public SimpleLogTests()
    {
        _parser = new LogParser(SimpleFileLogPath, LogRegex);
    }

    private void Initialize()
    {
        _parser = new LogParser(SimpleFileLogPath, LogRegex);
    }

    [SetUp]
    public void Setup()
    {
        Initialize();
    }

    [Test]
    public async Task ShouldHaveSevenLogs()
    {
        Assert.AreEqual(7, await _parser.GetLogLinesAsync());
    }

    [Test]
    public async Task ShouldReadAllLines()
    {
        await ValidateFirstLine();
        Assert.AreEqual("[2019-01-23T12:34:56.123456789] INFO    | Log1", (await _parser.ReadNextLogAsync())!.Log);
        Assert.AreEqual("[2019-01-23T12:34:56.123456789] WARNING | Log2", (await _parser.ReadNextLogAsync())!.Log);
        Assert.AreEqual("[2019-01-23T12:34:56.123456789] DEBUG   | Log3", (await _parser.ReadNextLogAsync())!.Log);
        Assert.AreEqual("[2019-01-23T12:34:56.123456789] SCOPE   | Scope", (await _parser.ReadNextLogAsync())!.Log);
        await ValidateSixthLine();
        await ValidateSeventhLine();
        Assert.AreEqual(null, await _parser.ReadNextLogAsync());
    }

    private async Task ValidateFirstLine()
    {
        var log = (await _parser.ReadNextLogAsync())!;
        Assert.AreEqual("[2019-01-23T12:34:56.123456789] ERROR   | Log", log.Log);
        Assert.AreEqual(LogRegexGroupCount, log.RegexGroups.Length);
        Assert.AreEqual("2019-01-23T12:34:56.123456789", log.RegexGroups[0]);
        Assert.AreEqual("ERROR", log.RegexGroups[1]);
        Assert.AreEqual("|", log.RegexGroups[2]);
        Assert.AreEqual(" ", log.RegexGroups[3]);
        Assert.AreEqual("Log", log.RegexGroups[4]);
    }

    private async Task ValidateSixthLine()
    {
        var log = (await _parser.ReadNextLogAsync())!;
        Assert.AreEqual("[2019-01-23T12:34:56.123456789] ERROR   |   Log4", log.Log);
        Assert.AreEqual(LogRegexGroupCount, log.RegexGroups.Length);
        Assert.AreEqual("2019-01-23T12:34:56.123456789", log.RegexGroups[0]);
        Assert.AreEqual("ERROR", log.RegexGroups[1]);
        Assert.AreEqual("|", log.RegexGroups[2]);
        Assert.AreEqual("   ", log.RegexGroups[3]);
        Assert.AreEqual("Log4", log.RegexGroups[4]);
    }

    private async Task ValidateSeventhLine()
    {
        var log = (await _parser.ReadNextLogAsync())!;
        Assert.AreEqual("[2019-01-23T12:34:56.123456789] INFO    | Log5 after scope", log.Log);
        Assert.AreEqual(LogRegexGroupCount, log.RegexGroups.Length);
        Assert.AreEqual("2019-01-23T12:34:56.123456789", log.RegexGroups[0]);
        Assert.AreEqual("INFO", log.RegexGroups[1]);
        Assert.AreEqual("|", log.RegexGroups[2]);
        Assert.AreEqual(" ", log.RegexGroups[3]);
        Assert.AreEqual("Log5 after scope", log.RegexGroups[4]);
    }
}