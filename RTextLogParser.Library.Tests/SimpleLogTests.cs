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
    private static readonly Regex LogRegex = new Regex(@"\[(.+?)\]\s*(\w+)\s*(.)(\s+)(.*?)(?=\r?\n\[)", RegexOptions.Singleline);

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
        Assert.AreEqual("[2019-01-23T12:34:56.123456789] ERROR   | Log", await _parser.ReadNextLogAsync());
        Assert.AreEqual("[2019-01-23T12:34:56.123456789] INFO    | Log1", await _parser.ReadNextLogAsync());
        Assert.AreEqual("[2019-01-23T12:34:56.123456789] WARNING | Log2", await _parser.ReadNextLogAsync());
        Assert.AreEqual("[2019-01-23T12:34:56.123456789] DEBUG   | Log3", await _parser.ReadNextLogAsync());
        Assert.AreEqual("[2019-01-23T12:34:56.123456789] SCOPE   | Scope", await _parser.ReadNextLogAsync());
        Assert.AreEqual("[2019-01-23T12:34:56.123456789] ERROR   |   Log4", await _parser.ReadNextLogAsync());
        Assert.AreEqual("[2019-01-23T12:34:56.123456789] INFO    | Log5 after scope", await _parser.ReadNextLogAsync());
        Assert.AreEqual(null, await _parser.ReadNextLogAsync());
    }
}