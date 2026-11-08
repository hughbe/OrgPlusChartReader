using System.Drawing;

namespace OrgPlusChartReader.Tests;

public class OrganizationChartTests
{
    private static string SamplesRoot => AppContext.BaseDirectory;

    private static string SamplePath(string relativePath) => Path.Combine(SamplesRoot, relativePath);

    private static byte[] ReadSample(string relativePath) => File.ReadAllBytes(SamplePath(relativePath));

    public static IEnumerable<object[]> SampleFiles()
    {
        // Search the whole sample tree, including nested folders and files in the root.
        var extensions = new[] { ".opx", ".ops" };
        return Directory.GetFiles(SamplesRoot, "*", SearchOption.AllDirectories)
            .Where(path => extensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
            .Select(path => Path.GetRelativePath(SamplesRoot, path))
            .OrderBy(path => path, StringComparer.Ordinal)
            .Select(path => new object[] { path });
    }

    private static IEnumerable<OrganizationChartRecord> Flatten(IEnumerable<OrganizationChartRecord> records)
    {
        foreach (var record in records)
        {
            yield return record;
            if (record is OrganizationChartSubstream substream)
            {
                foreach (var child in Flatten(substream.Records))
                {
                    yield return child;
                }
            }
        }
    }

    [Fact]
    public void SampleFiles_AreDiscovered()
    {
        Assert.True(SampleFiles().Count() >= 100);
    }

    [Theory]
    [MemberData(nameof(SampleFiles))]
    public void Ctor_SampleFile_Parses(string path)
    {
        using var stream = File.OpenRead(SamplePath(path));
        var chart = new OrganizationChart(stream);

        // Every file is a single root section followed by the footer.
        Assert.Equal(OrganizationChartRecordId.ParentSection, chart.Records.OfType<OrganizationChartSubstream>().Single().Id);
        Assert.Same(chart.Footer, chart.Records[chart.Records.Count - 1]);
        Assert.Equal(OrganizationChartRecordId.Footer, chart.Footer.Id);
    }

    [Theory]
    [MemberData(nameof(SampleFiles))]
    public void Ctor_SampleFile_RecordsAreSpecialized(string path)
    {
        var chart = new OrganizationChart(ReadSample(path));
        foreach (var record in Flatten(chart.Records))
        {
            Assert.True(Enum.IsDefined(typeof(OrganizationChartRecordId), record.Id) || record is OrganizationChartSubstream, $"{record.Id}");
            Assert.Equal(record.Bytes.Length, record.RecordSize);
        }
    }

    [Fact]
    public void Ctor_SampleFile_FooterFileSizeMatches()
    {
        byte[] data = ReadSample("Lots.opx");
        var chart = new OrganizationChart(data);
        Assert.Equal((uint)(data.Length - 14), chart.Footer.FileSize);
        Assert.Empty(chart.SlackBytes);
    }

    [Theory]
    [InlineData("Simple.opx")]
    [InlineData("Box/Name1.opx")]
    public void Ctor_FileWithStaleBytesBeforeFooter_ExposesSlackBytes(string path)
    {
        byte[] data = ReadSample(path);
        var chart = new OrganizationChart(data);
        Assert.NotEmpty(chart.SlackBytes);
        Assert.Equal((uint)(data.Length - 14), chart.Footer.FileSize);
    }

    [Theory]
    [InlineData("Border/RedBorder.opx", 255, 0, 0)]
    [InlineData("Border/YellowBorder.opx", 255, 255, 0)]
    public void Ctor_BorderColor_IsDecoded(string path, int r, int g, int b)
    {
        var chart = new OrganizationChart(ReadSample(path));
        var border = Flatten(chart.Records).OfType<BorderRecord>().First(x => x.Color != Color.FromArgb(0, 0, 0));
        Assert.Equal(Color.FromArgb(r, g, b), border.Color);
    }

    [Theory]
    [InlineData("Border/NoBorder.opx", BorderRecord.BorderStyle.None)]
    [InlineData("Border/SingleBorder2.opx", BorderRecord.BorderStyle.Single2)]
    [InlineData("Border/HairlineBorder.opx", BorderRecord.BorderStyle.Hairline)]
    [InlineData("Border/DoubleBorder1.opx", BorderRecord.BorderStyle.Double1)]
    [InlineData("Border/DoubleBorder5.opx", BorderRecord.BorderStyle.Double5)]
    public void Ctor_BorderStyle_IsDecoded(string path, BorderRecord.BorderStyle expected)
    {
        var chart = new OrganizationChart(ReadSample(path));
        Assert.Equal(expected, Flatten(chart.Records).OfType<BorderRecord>().First().Style);
    }

    [Theory]
    [InlineData("Lines/Style/Solid.opx", LineStyle.Solid)]
    [InlineData("Lines/Style/Dashed.opx", LineStyle.Dashed)]
    [InlineData("Lines/Style/DashedWide.opx", LineStyle.DashedWide)]
    public void Ctor_LineStyle_IsDecoded(string path, LineStyle expected)
    {
        var chart = new OrganizationChart(ReadSample(path));
        Assert.Contains(Flatten(chart.Records).OfType<LineRecord>(), x => x.Style == expected);
    }

    [Fact]
    public void Ctor_TextRecords_AreTrimmedOfNulTerminator()
    {
        var chart = new OrganizationChart(ReadSample("Lots.opx"));
        var texts = Flatten(chart.Records).OfType<TextRecord>().Select(x => x.Value).ToList();
        Assert.Contains("Name", texts);
        Assert.DoesNotContain(texts, x => x.Contains('\0'));
        Assert.DoesNotContain(Flatten(chart.Records).OfType<FontNameRecord>(), x => x.Value.Contains('\0'));
    }

    [Fact]
    public void Ctor_ShadowTypes_AreDecoded()
    {
        var none = new OrganizationChart(ReadSample("Shadow/ShadowNone.opx"));
        Assert.All(Flatten(none.Records).OfType<ShadowRecord>(), x => Assert.Equal(ShadowRecord.ShadowType.None, x.Type));

        var bevel = new OrganizationChart(ReadSample("Shadow/ShadowLeftBottomBevel.opx"));
        Assert.Contains(Flatten(bevel.Records).OfType<ShadowRecord>(), x => x.Type == ShadowRecord.ShadowType.SideAndBottomBevel);
    }

    [Fact]
    public void Ctor_Stream_IsNotClosed()
    {
        using var stream = new MemoryStream(ReadSample("Lots.opx"));
        _ = new OrganizationChart(stream);
        Assert.True(stream.CanRead);
    }

    [Fact]
    public void Ctor_NonSeekableStream_Parses()
    {
        byte[] data = ReadSample("Lots.opx");
        using var stream = new NonSeekableStream(data);
        var chart = new OrganizationChart(stream);
        Assert.Equal(new OrganizationChart(data).Records.Count, chart.Records.Count);
    }

    [Fact]
    public void Ctor_NullStream_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>("stream", () => new OrganizationChart((Stream)null!));
    }

    [Fact]
    public void Ctor_NullBytes_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>("data", () => new OrganizationChart((byte[])null!));
    }

    [Theory]
    [InlineData(new byte[0])]
    [InlineData(new byte[] { (byte)'U', (byte)'O' })]
    [InlineData(new byte[] { (byte)'N', (byte)'O', (byte)'P', (byte)'E', 0, 0 })]
    public void Ctor_InvalidSignature_ThrowsInvalidDataException(byte[] data)
    {
        Assert.Throws<InvalidDataException>(() => new OrganizationChart(data));
    }

    [Fact]
    public void Ctor_EveryTruncatedPrefix_ThrowsInvalidDataException()
    {
        // Lots.opx has no stale bytes, so no proper prefix of it is a valid chart.
        byte[] data = ReadSample("Lots.opx");
        for (int length = 0; length < data.Length; length++)
        {
            byte[] prefix = data.AsSpan(0, length).ToArray();
            Assert.Throws<InvalidDataException>(() => new OrganizationChart(prefix));
        }
    }

    [Theory]
    [InlineData("Lots.opx")]
    [InlineData("Simple.opx")]
    public void Ctor_TruncatedPrefix_OnlyEverThrowsInvalidDataException(string path)
    {
        byte[] data = ReadSample(path);
        for (int length = 0; length < data.Length; length++)
        {
            byte[] prefix = data.AsSpan(0, length).ToArray();
            try
            {
                _ = new OrganizationChart(prefix);
            }
            catch (InvalidDataException)
            {
            }
        }
    }

    [Fact]
    public void Ctor_CorruptedBytes_OnlyEverThrowsInvalidDataException()
    {
        byte[] original = ReadSample("Lots.opx");
        var random = new Random(12345);
        for (int i = 0; i < 5000; i++)
        {
            byte[] data = (byte[])original.Clone();
            int mutations = random.Next(1, 4);
            for (int j = 0; j < mutations; j++)
            {
                data[random.Next(data.Length)] = (byte)random.Next(256);
            }

            try
            {
                _ = new OrganizationChart(data);
            }
            catch (InvalidDataException)
            {
            }
        }
    }

    [Fact]
    public void Ctor_FooterWithWrongFileSize_ThrowsInvalidDataException()
    {
        byte[] data = ReadSample("Lots.opx");
        data[data.Length - 4] ^= 0xFF; // Corrupt the file size in the footer.
        Assert.Throws<InvalidDataException>(() => new OrganizationChart(data));
    }

    [Fact]
    public void Ctor_UnclosedSection_ThrowsInvalidDataException()
    {
        byte[] data = ReadSample("Lots.opx");
        // Remove the root section's end marker (0x6001), which immediately precedes the footer.
        int markerOffset = data.Length - 14 - 2;
        Assert.Equal(0x6001, BitConverter.ToUInt16(data, markerOffset));
        byte[] modified = data.Take(markerOffset).Concat(data.Skip(markerOffset + 2)).ToArray();
        Assert.Throws<InvalidDataException>(() => new OrganizationChart(modified));
    }

    [Fact]
    public void Ctor_MismatchedSectionEnd_ThrowsInvalidDataException()
    {
        byte[] data = ReadSample("Lots.opx");
        int markerOffset = data.Length - 14 - 2;
        data[markerOffset + 1] = 0x7F; // 0x7F01 does not close 0x4001.
        Assert.Throws<InvalidDataException>(() => new OrganizationChart(data));
    }

    [Fact]
    public void Create_ShortData_ThrowsInvalidDataException()
    {
        Assert.Throws<InvalidDataException>(() => OrganizationChartRecord.Create(OrganizationChartRecordId.Border, new byte[3]));
        Assert.Throws<InvalidDataException>(() => OrganizationChartRecord.Create(OrganizationChartRecordId.Footer, new byte[0]));
    }

    [Fact]
    public void Create_SectionMarker_ThrowsInvalidDataException()
    {
        Assert.Throws<InvalidDataException>(() => OrganizationChartRecord.Create(OrganizationChartRecordId.ParentSection, new byte[0]));
    }

    [Fact]
    public void Create_UnknownId_ThrowsInvalidDataException()
    {
        Assert.Throws<InvalidDataException>(() => OrganizationChartRecord.Create((OrganizationChartRecordId)0x0777, new byte[0]));
    }

    [Fact]
    public void Create_ValidData_ReturnsSpecializedRecord()
    {
        var record = OrganizationChartRecord.Create(OrganizationChartRecordId.BoxIndex, new byte[] { 2, 0 });
        Assert.Equal(2, Assert.IsType<BoxIndexRecord>(record).Index);
    }

    [Fact]
    public void HexTypeConverter_RoundTrips()
    {
        var converter = new HexTypeConverter();
        Assert.Equal("0x00FF", converter.ConvertToString((ushort)0xFF));
        Assert.Equal("0x000000FF", converter.ConvertToString((uint)0xFF));
        Assert.Equal("0xFF", converter.ConvertToString((byte)0xFF));
        Assert.Equal(0xFFu, converter.ConvertFromString("0xFF"));
    }

    private sealed class NonSeekableStream : MemoryStream
    {
        public NonSeekableStream(byte[] buffer) : base(buffer)
        {
        }

        public override bool CanSeek => false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }
    }
}
