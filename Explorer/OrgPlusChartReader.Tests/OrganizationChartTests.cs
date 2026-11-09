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
    [InlineData("Lines/Style/Dashed.opx", LineStyle.Dotted)]
    [InlineData("Lines/Style/DashedWide.opx", LineStyle.Dashed)]
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

    [Theory]
    [MemberData(nameof(SampleFiles))]
    public void Ctor_SampleFile_ChecksumIsValid(string path)
    {
        Assert.True(new OrganizationChart(ReadSample(path)).IsChecksumValid);
    }

    [Fact]
    public void Ctor_ModifiedByte_ChecksumIsInvalidButFileParses()
    {
        byte[] data = ReadSample("Lots.opx");
        int textOffset = IndexOf(data, System.Text.Encoding.ASCII.GetBytes("Comment1"));
        data[textOffset] = (byte)'c';

        var chart = new OrganizationChart(data);
        Assert.False(chart.IsChecksumValid);
    }

    [Fact]
    public void ComputeChecksums_ReturnsByteSumAndRotateXor()
    {
        // sum = 1 + 2 = 3; rotxor = rol(rol(0 ^ 1) ^ 2) = rol(2 ^ 2) = 0.
        Assert.Equal(((ushort)3, (ushort)0), FooterRecord.ComputeChecksums(new byte[] { 1, 2, 99 }, 2));
        Assert.Equal(((ushort)0x80, (ushort)0x0100), FooterRecord.ComputeChecksums(new byte[] { 0x80 }, 1));
        Assert.Throws<ArgumentNullException>("data", () => FooterRecord.ComputeChecksums(null!, 0));
        Assert.Throws<ArgumentOutOfRangeException>("count", () => FooterRecord.ComputeChecksums(new byte[1], 2));
    }

    [Theory]
    [InlineData("Rectangle/Rectangle.opx", 0x89u, 0xB9u, 0x20Eu, 0x205u)]
    [InlineData("Rectangle/ZeroX.opx", 0x60u, 0u, 0x175u, 0x11Bu)]
    [InlineData("Rectangle/ZeroY.opx", 0u, 0xAFu, 0x224u, 0xBBu)]
    [InlineData("Rectangle/Wider.opx", 0x60u, 0xF6u, 0x1C6u, 0x11Bu)]
    public void Ctor_ShapeEdges_AreDecoded(string path, uint top, uint left, uint right, uint bottom)
    {
        var chart = new OrganizationChart(ReadSample(path));
        var shape = Flatten(chart.Records).OfType<ShapeRecord>().First();
        Assert.Equal(ShapeRecord.ShapeType.Rectangle, shape.Type);
        Assert.Equal((top, left, right, bottom), (shape.Top, shape.Left, shape.Right, shape.Bottom));
    }

    [Fact]
    public void Ctor_StraightLines_HaveTwoUnitThickBoundingBoxes()
    {
        var shapes = Flatten(new OrganizationChart(ReadSample("Rectangle/Rectangle.opx")).Records).OfType<ShapeRecord>().ToList();
        var horizontal = shapes.Single(x => x.Type == ShapeRecord.ShapeType.HorizontalLine);
        var vertical = shapes.Single(x => x.Type == ShapeRecord.ShapeType.VerticalLine);
        Assert.Equal(2u, horizontal.Bottom - horizontal.Top);
        Assert.Equal(2u, vertical.Right - vertical.Left);
    }

    [Theory]
    [InlineData("Shadow/ShadowTop.opx", ShadowRecord.ShadowType.TopOrBottom, -4, 4)]
    [InlineData("Shadow/ShadowBottom.opx", ShadowRecord.ShadowType.TopOrBottom, 4, 4)]
    [InlineData("Shadow/ShadowLeftBottom.opx", ShadowRecord.ShadowType.SideAndBottom, 4, -4)]
    [InlineData("Shadow/ShadowRightBottom.opx", ShadowRecord.ShadowType.SideAndBottom, 4, 4)]
    public void Ctor_ShadowOffsets_AreDecoded(string path, ShadowRecord.ShadowType type, short offsetY, short offsetX)
    {
        var chart = new OrganizationChart(ReadSample(path));
        var shadow = Flatten(chart.Records).OfType<ShadowRecord>().First(x => x.Id == OrganizationChartRecordId.Shadow);
        Assert.Equal((type, offsetY, offsetX), (shadow.Type, shadow.OffsetY, shadow.OffsetX));
    }

    [Fact]
    public void Ctor_Groups_DescribeTheTree()
    {
        var chart = new OrganizationChart(ReadSample("Plus/Level2.opx"));
        var groups = Flatten(chart.Records).OfType<GroupRecord>().ToList();

        Assert.Equal(new short[] { -1, 0, 1 }, groups.Select(x => x.ParentGroupIndex));
        Assert.Equal(new ushort[] { 0, 1, 4 }, groups.Select(x => x.ParentBoxIndex));
        Assert.Equal(new ushort[] { 0, 1, 2 }, groups.Select(x => x.GroupId));
        Assert.All(groups, x => Assert.Equal(x.GroupId, x.AssistantPartGroupId));
        Assert.Equal(new ushort[] { 0, 0, 2 }, groups.Select(x => x.AssistantCount));

        var layouts = Flatten(chart.Records).OfType<GroupLayoutRecord>().ToList();
        Assert.Equal(new ushort[] { 0, 0, 25 }, layouts.Select(x => x.Style));
        Assert.All(layouts, x => Assert.Equal(x.Style, x.AssistantStyle));
        Assert.Equal(new ushort[] { 3, 1, 2 }, groups.Select(x => x.Count));

        var boxes = Flatten(chart.Records).OfType<BoxRecord>().ToList();
        Assert.Equal(BoxRecord.BoxType.Assistant, boxes[4].Type);
        Assert.Equal(6, Flatten(chart.Records).OfType<BodyInfoRecord>().Single().BoxCount);
    }

    [Theory]
    [MemberData(nameof(SampleFiles))]
    public void Ctor_SampleFile_BoxTemplatesExist(string path)
    {
        var chart = new OrganizationChart(ReadSample(path));
        Assert.All(Flatten(chart.Records).OfType<BoxTemplateRecord>(), x => Assert.Empty(x.UnparsedBytes));
        var templateIds = Flatten(chart.Records).OfType<BoxTemplateRecord>().Select(x => x.TemplateId).ToHashSet();
        foreach (var box in Flatten(chart.Records).OfType<BoxRecord>())
        {
            Assert.True(box.TemplateId == 0xFFFF || templateIds.Contains(box.TemplateId), $"Template 0x{box.TemplateId:X4}");
        }
    }

    [Fact]
    public void Ctor_BoxTemplate_ListsFieldsInTheMask()
    {
        var chart = new OrganizationChart(ReadSample("Lots.opx"));
        var templates = Flatten(chart.Records).OfType<BoxTemplateRecord>().ToList();
        Assert.Equal(new byte[] { 0 }, templates.First(x => x.TemplateId == 0x01).FieldIndices);
        Assert.Equal(new byte[] { 0, 1, 2, 3 }, templates.First(x => x.TemplateId == 0x0F).FieldIndices);

        // 04 01 00 06 00: marker, field 0, end of line, end.
        var nameOnly = templates.First(x => x.TemplateId == 0x01);
        Assert.Equal(
            new[] { BoxTemplateRecord.Opcode.MarkerA, BoxTemplateRecord.Opcode.Field, BoxTemplateRecord.Opcode.EndOfLine },
            nameOnly.Instructions.Select(x => x.Opcode));
        Assert.Empty(nameOnly.UnparsedBytes);
    }

    [Fact]
    public void Ctor_Connector_IsDecoded()
    {
        var chart = new OrganizationChart(ReadSample("Connectors/Co8.opx"));
        var connector = Flatten(chart.Records).OfType<ConnectorRecord>().Single();
        Assert.Equal((4, 1, 2, 0), (connector.FromBoxIndex, connector.FromGroupId, connector.ToBoxIndex, connector.ToGroupId));
        Assert.Equal((ConnectorRecord.Anchor.Right, ConnectorRecord.Anchor.Bottom), (connector.FromAnchor, connector.ToAnchor));
    }

    [Theory]
    [InlineData("Box/Title/Style/NarrowTitle.opx", "Arial Narrow")]
    [InlineData("Box/Title/Style/BlackTitle.opx", "Arial Black")]
    public void Ctor_TextStyleFontIndex_RefersToFontTable(string path, string fontName)
    {
        var records = Flatten(new OrganizationChart(ReadSample(path)).Records).ToList();
        var style = records.OfType<TextStyleRecord>().Last(x => x.Property == TextStyleRecord.TextStyleProperty.FontIndex);
        Assert.Equal(fontName, records.OfType<FontNameRecord>().Single(x => x.Index == style.Value).Value);
    }

    [Theory]
    [InlineData("Box/Title/Style/BoldTitle.opx", TextStyleRecord.FontStyleFlags.Bold)]
    [InlineData("Box/Title/Style/ItalicTitle.opx", TextStyleRecord.FontStyleFlags.Italic)]
    [InlineData("Box/Title/Style/BoldItalicTitle.opx", TextStyleRecord.FontStyleFlags.Bold | TextStyleRecord.FontStyleFlags.Italic)]
    public void Ctor_TextStyleFontStyle_IsDecoded(string path, TextStyleRecord.FontStyleFlags expected)
    {
        var records = Flatten(new OrganizationChart(ReadSample(path)).Records);
        Assert.Contains(records.OfType<TextStyleRecord>(), x => x.Property == TextStyleRecord.TextStyleProperty.FontStyle && x.FontStyle == expected);
    }

    [Theory]
    [InlineData("Box/Title/Color/RedTitle.opx", 255, 0, 0)]
    [InlineData("Box/Title/Style/GreenTitle.opx", 0, 255, 0)]
    public void Ctor_TextColor_IsDecodedAsRgb(string path, int r, int g, int b)
    {
        var records = Flatten(new OrganizationChart(ReadSample(path)).Records);
        Assert.Contains(records.OfType<TextStyleRecord>(), x => x.Property == TextStyleRecord.TextStyleProperty.TextColor && x.Color == Color.FromArgb(r, g, b));
    }

    [Theory]
    [InlineData("Box/TextLeft.opx", TextStyleRecord.TextAlignment.Left)]
    [InlineData("Box/TextCenter.opx", TextStyleRecord.TextAlignment.Center)]
    [InlineData("Box/TextRight.opx", TextStyleRecord.TextAlignment.Right)]
    public void Ctor_TextAlignment_IsDecoded(string path, TextStyleRecord.TextAlignment expected)
    {
        var chart = new OrganizationChart(ReadSample(path));
        var boxText = Flatten(chart.Records).OfType<OrganizationChartSubstream>().First(x => x.Id == OrganizationChartRecordId.BoxesSection);
        var alignment = Flatten(boxText.Records).OfType<TextStyleRecord>().First(x => x.Property == TextStyleRecord.TextStyleProperty.Alignment);
        Assert.Equal(expected, alignment.Alignment);
    }

    [Fact]
    public void Ctor_StyleFile_IsIdentified()
    {
        var style = new OrganizationChart(ReadSample("OPS/3DGOLD.OPS"));
        Assert.Equal(Unknown0x0002Record.DocumentKind.Style, Flatten(style.Records).OfType<Unknown0x0002Record>().Single().Kind);

        var chart = new OrganizationChart(ReadSample("Lots.opx"));
        Assert.Equal(Unknown0x0002Record.DocumentKind.Chart, Flatten(chart.Records).OfType<Unknown0x0002Record>().Single().Kind);
    }

    [Fact]
    public void Ctor_SelectedShape_RecordsItsIndex()
    {
        var chart = new OrganizationChart(ReadSample("Rectangle/Rectangle.opx"));
        Assert.Equal(2, Flatten(chart.Records).OfType<SelectedRecord>().Single().Value);
    }

    [Theory]
    [InlineData("Shadow/ShadowBottom.opx", 1, false, false, false, true)]
    [InlineData("Shadow/ShadowTop.opx", 1, false, false, true, false)]
    [InlineData("Shadow/ShadowLeftBottom.opx", 2, true, false, false, true)]
    [InlineData("Shadow/ShadowRightBottom.opx", 2, false, true, false, true)]
    [InlineData("Shadow/ShadowLeftBottomRight.opx", 1, true, true, false, true)]
    [InlineData("OPS/3DGOLD.OPS", 2, true, true, true, true)]
    public void Ctor_BorderShadowEdges_AreDecoded(string path, int shadowBase, bool left, bool right, bool top, bool bottom)
    {
        var chart = new OrganizationChart(ReadSample(path));
        var border = Flatten(chart.Records).OfType<BorderRecord>().First(x => x.Id == OrganizationChartRecordId.Border);
        Assert.Equal((shadowBase, left, right, top, bottom), (border.ShadowBase, border.HasLeftShadow, border.HasRightShadow, border.HasTopShadow, border.HasBottomShadow));
    }

    [Fact]
    public void Ctor_ShadowNone_HasNoShadowEdges()
    {
        var chart = new OrganizationChart(ReadSample("Shadow/ShadowNone.opx"));
        Assert.All(Flatten(chart.Records).OfType<BorderRecord>(), x => Assert.Equal(0, x.ShadowEdges));
    }

    [Fact]
    public void Ctor_TextStyleRun_HasCharacterPosition()
    {
        var chart = new OrganizationChart(ReadSample("Plus/Level2.opx"));
        Assert.Contains(Flatten(chart.Records).OfType<TextStyleRecord>(),
            x => x.Property == TextStyleRecord.TextStyleProperty.FontIndex && x.Value == 1 && x.CharacterPosition == 8);
    }

    [Fact]
    public void Ctor_VersionRecords_HaveVersionAndDate()
    {
        var records = Flatten(new OrganizationChart(ReadSample("Lots.opx")).Records).ToList();
        var a = records.OfType<Unknown0x0003Record>().Single();
        var b = records.OfType<Unknown0x0002Record>().Single();
        Assert.Equal((1001, 26, 12, 92), ((int)a.Version, (int)a.Day, (int)a.Month, (int)a.Year));
        Assert.Equal((1010, 26, 12, 92), ((int)b.Version, (int)b.Day, (int)b.Month, (int)b.Year));
    }

    [Fact]
    public void Ctor_CanvasDefaults_UseBorderAndShadowLayouts()
    {
        var canvas = Flatten(new OrganizationChart(ReadSample("Lots.opx")).Records)
            .OfType<OrganizationChartSubstream>().Single(x => x.Id == OrganizationChartRecordId.CanvasSection);
        Assert.IsType<BorderRecord>(canvas.Records.Single(x => x.Id == OrganizationChartRecordId.Unknown0x0095));
        Assert.IsType<ShadowRecord>(canvas.Records.Single(x => x.Id == OrganizationChartRecordId.Unknown0x0096));
        Assert.IsType<ShadowRecord>(canvas.Records.Single(x => x.Id == OrganizationChartRecordId.LineShadow));
    }

    [Fact]
    public void Ctor_DefaultFillColor_HasHighByte0xFF()
    {
        var chart = new OrganizationChart(ReadSample("TextBox/TextBox.opx"));
        Assert.Equal(0xFF, Flatten(chart.Records).OfType<ShapeColorRecord>().Single().ColorHighByte);

        var red = new OrganizationChart(ReadSample("TextBox/RedColor.opx"));
        Assert.Equal(0, Flatten(red.Records).OfType<ShapeColorRecord>().Single().ColorHighByte);
    }

    [Fact]
    public void Create_RecordsOnlyKnownFromTheProgram_AreDecoded()
    {
        var arrow = Assert.IsType<ArrowRecord>(OrganizationChartRecord.Create(OrganizationChartRecordId.Arrow,
            new byte[] { 14, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 3, 0 }));
        Assert.Equal((1, 3), ((int)arrow.End, (int)arrow.ItemIndex));

        var link = Assert.IsType<ShapeBoxLinkRecord>(OrganizationChartRecord.Create(OrganizationChartRecordId.ShapeBoxLink,
            new byte[] { 22, 0, 1, 0, 5, 0, 1, 0, 0, 0, 2, 0, 0, 0, 3, 0, 0, 0, 4, 0, 0, 0 }));
        Assert.Equal((1, 5, 1u, 4u), ((int)link.GroupIndex, (int)link.BoxIndex, link.Unknown1, link.Unknown4));

        Assert.IsType<OrganizationChartRecord>(OrganizationChartRecord.Create(OrganizationChartRecordId.PictureSize, new byte[40]));
    }

    [Fact]
    public void Ctor_SampleFile_HasViewUnitsPasswordFlagAndCharacterSet()
    {
        var records = Flatten(new OrganizationChart(ReadSample("Lots.opx")).Records).ToList();
        Assert.Equal(144, records.OfType<ViewUnitsRecord>().Single().UnitsPerInch);
        Assert.False(records.OfType<PasswordRecord>().Single().IsPasswordProtected);
        Assert.Equal(BodyInfoRecord.TextCharacterSet.Ansi, records.OfType<BodyInfoRecord>().Single().CharacterSet);
    }

    [Fact]
    public void Ctor_ExtendedRecordLength_IsRead()
    {
        // Rewrite Lots.opx's first text record (0x0060 "Name\0") with the 0xFFFF length escape.
        byte[] data = ReadSample("Lots.opx");
        byte[] original = { 0x60, 0x00, 0x05, 0x00, (byte)'N', (byte)'a', (byte)'m', (byte)'e', 0x00 };
        int offset = IndexOf(data, original);
        byte[] extended = { 0x60, 0x00, 0xFF, 0xFF, 0x05, 0x00, 0x00, 0x00, (byte)'N', (byte)'a', (byte)'m', (byte)'e', 0x00 };
        byte[] body = data.Take(offset).Concat(extended).Concat(data.Skip(offset + original.Length).Take(data.Length - 14 - offset - original.Length)).ToArray();

        // Rebuild the footer for the longer file.
        var (sum, rotateXor) = FooterRecord.ComputeChecksums(body, body.Length);
        byte[] footer = new byte[14];
        BitConverter.GetBytes((ushort)0x0FFF).CopyTo(footer, 0);
        BitConverter.GetBytes((ushort)10).CopyTo(footer, 2);
        BitConverter.GetBytes((ushort)10).CopyTo(footer, 4);
        BitConverter.GetBytes(sum).CopyTo(footer, 6);
        BitConverter.GetBytes(rotateXor).CopyTo(footer, 8);
        BitConverter.GetBytes((uint)body.Length).CopyTo(footer, 10);

        var chart = new OrganizationChart(body.Concat(footer).ToArray());
        Assert.True(chart.IsChecksumValid);
        Assert.Contains(Flatten(chart.Records).OfType<TextRecord>(), x => x.Value == "Name");
        Assert.Equal(
            Flatten(new OrganizationChart(data).Records).Count(),
            Flatten(chart.Records).Count());
    }

    [Fact]
    public void Ctor_PageSetup_IsDecoded()
    {
        var setup = Flatten(new OrganizationChart(ReadSample("OPS/3DGOLD.OPS")).Records).OfType<PageSetupRecord>().Single();
        Assert.Equal(PageSetupRecord.PageOrientation.Portrait, setup.Orientation);
        Assert.Equal(0, setup.LayoutDirection);
        Assert.Equal(1, setup.PaperSize);
        Assert.Equal("HP LaserJet 4/4M Plus PS 600,adobeps,LPT2:", setup.Printer);
        Assert.Equal(72u, setup.LeftMargin);

        var chart = Flatten(new OrganizationChart(ReadSample("Lots.opx")).Records).OfType<PageSetupRecord>().Single();
        Assert.True(chart.UseDefaultPrinter);
        Assert.Equal("", chart.Printer);
    }

    [Fact]
    public void Create_Formula_IsParsed()
    {
        byte[] text = { (byte)'@', (byte)'s', (byte)'u', (byte)'m', (byte)'(', (byte)'s', (byte)'u', (byte)'b', (byte)',', 0x01, 0x02, (byte)')', 0 };
        byte[] bytes = new byte[] { 0, 0, 0, 0, 0xFF }.Concat(text).ToArray();
        var formula = Assert.IsType<FormulaRecord>(OrganizationChartRecord.Create(OrganizationChartRecordId.Formula, bytes));
        Assert.Equal(("sum", "sub", (byte?)2), (formula.Function, formula.Scope, formula.FieldIndex));

        var predefined = Assert.IsType<FormulaRecord>(OrganizationChartRecord.Create(OrganizationChartRecordId.Formula, new byte[] { 0, 0, 3, 0, 1 }));
        Assert.Equal((3, 1), ((int)predefined.PredefinedCode, (int)predefined.PredefinedField));
        Assert.Null(predefined.Text);
    }

    [Fact]
    public void Create_NumberFormat_IsDecoded()
    {
        byte[] bytes = { 18, 0, 0, 0, (byte)'$', 0, 0, 0, (byte)'.', (byte)',', 1, 0, 2, 0, 0, 0, 1, 0 };
        var format = Assert.IsType<NumberFormatRecord>(OrganizationChartRecord.Create(OrganizationChartRecordId.NumberFormat, bytes));
        Assert.Equal(("$", '.', ',', true, 2, true),
            (format.CurrencySymbol, format.DecimalSeparator, format.ThousandsSeparator, format.CurrencySymbolFirst, (int)format.Digits, format.IsCurrency));
    }

    [Fact]
    public void Create_NumericValueAndPalette_AreDecoded()
    {
        var value = Assert.IsType<NumericValueRecord>(OrganizationChartRecord.Create(OrganizationChartRecordId.NumericValue, BitConverter.GetBytes(1234.5)));
        Assert.Equal(1234.5, value.Value);

        // Two colours: yellow (stored R, B, G) as a standard colour, and red.
        byte[] palette = { 12, 0, 2, 0, 0xFF, 0x00, 0xFF, 1, 0xFF, 0x00, 0x00, 0 };
        var record = Assert.IsType<ColorPaletteRecord>(OrganizationChartRecord.Create(OrganizationChartRecordId.ColorPalette, palette));
        Assert.Equal(new[] { Color.FromArgb(255, 255, 0), Color.FromArgb(255, 0, 0) }, record.Colors);
        Assert.Equal(new[] { true, false }, record.IsStandard);
    }

    private static int IndexOf(byte[] data, byte[] pattern)
    {
        for (int i = 0; i <= data.Length - pattern.Length; i++)
        {
            if (data.AsSpan(i, pattern.Length).SequenceEqual(pattern))
            {
                return i;
            }
        }

        throw new InvalidOperationException("Pattern not found.");
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
