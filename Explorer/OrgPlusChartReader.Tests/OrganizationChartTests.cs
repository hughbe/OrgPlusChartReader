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
        foreach (var filePath in Directory.GetFiles(folderName, "*"))
        {
            using var stream = File.OpenRead(filePath);
            var chart = new OrganizationChart(stream);

            foreach (var record in chart.Records)
            {
                
            }
        }
    }

    [Fact]
    public void Ctor_NullStream_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>("stream", () => new OrganizationChart(null!));
    }
}
