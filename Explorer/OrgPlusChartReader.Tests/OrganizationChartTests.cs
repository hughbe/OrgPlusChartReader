using System.Diagnostics;

namespace OrgPlusChartReader.Tests;

public class OrganizationChartTests
{
    [Theory]
    [InlineData("Border")]
    [InlineData("Box")]
    [InlineData("Connectors")]
    [InlineData("Global")]
    [InlineData("Layout")]
    [InlineData("Lines")]
    [InlineData("OPS")]
    [InlineData("Plus")]
    [InlineData("Rectangle")]
    [InlineData("Shadow")]
    [InlineData("TextBox")]
    public void Ctor_ValidStream_CreatesChart(string folderName)
    {
        foreach (var path in Directory.GetFiles(folderName, "*"))
        {
            using var stream = File.OpenRead(path);
            var chart = new OrganizationChart(stream);
            DumpChart(Path.GetFileName(path), chart);
        }
    }

    private static void DumpChart(string path, OrganizationChart chart)
    {
        Debug.WriteLine($"Organization Chart for {path}:");
        foreach (var record in chart.Records)
        {
            DumpRecord(record, 1);
        }
    }

    private static void DumpRecord(OrganizationChartRecord record, int indent)
    {
        var indentString = new string(' ', indent * 2);
        if (record is OrganizationChartSubstream substream)
        {
            Debug.WriteLine($"{indentString}Substream: {substream.Id}");
            foreach (var childRecord in substream.Records)
            {
                DumpRecord(childRecord, indent + 1);
            }
        }
        else
        {
            Debug.WriteLine($"{indentString}Record: {record.Id} Size: {record.RecordSize}");
        }
    }

    [Fact]
    public void Ctor_NullStream_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>("stream", () => new OrganizationChart(null!));
    }
}
