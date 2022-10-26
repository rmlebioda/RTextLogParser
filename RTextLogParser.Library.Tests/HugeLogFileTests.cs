using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;

namespace RTextLogParser.Library.Tests;

[TestFixture]
public class HugeLogFileTests
{
    private static string ThisDirectoryPath =>
        Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.FullName;
    private static string DataPath => Path.Combine(ThisDirectoryPath, "Data");
    private static string HugeFileLogPath => Path.Combine(DataPath, "HugeLogFile", "HugeLogFile.txt");
    private static readonly Regex LogRegex = new Regex(@"\[(.+?)\]\s*(\w+)\s*(.)(\s+)(.*?)(?=\r?\n\[)", RegexOptions.Singleline);


    public HugeLogFileTests()
    {
        // this is done only to satisfy "cannot be null" requirement from C#,
        // will be overwritten once in method with attribute OneTimeSetUp 
        _parser = new LogParser(HugeFileLogPath, LogRegex);
    }

    private LogParser _parser;
    
    private void Initialize()
    {
        _parser = new LogParser(HugeFileLogPath, LogRegex);
    }
    
    [OneTimeSetUp]
    public void Init()
    {
        Initialize();
    }

    [Test]
    public async Task ShouldHaveMillionLogs()
    {
        Assert.AreEqual((int)1e6, await _parser.GetLogLinesAsync());
    }

    [Test]
    public async Task ShouldReadFirstThreeLogs()
    {
        Assert.AreEqual("[2020-05-06T00:00:00.000000] WARNING |  Another test message", (await _parser.ReadNextLogAsync())!.Log);
        Assert.AreEqual("[2020-05-06T00:00:26.801288] ERROR   |  Duis aute irure dolor in reprehenderit in voluptate", (await _parser.ReadNextLogAsync())!.Log);
        Assert.AreEqual("[2020-05-06T00:00:27.840942] WARNING |  Pariatur nesciunt velit est. Molestias et a voluptatem quam est", (await _parser.ReadNextLogAsync())!.Log);
    }
}
