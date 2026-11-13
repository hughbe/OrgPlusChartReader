using System.Text;

namespace OrgPlusChartReader;

public class OrganizationChartRecord
{
    // Text in .opx files was written by a Windows application, so it is assumed to be in the
    // Windows-1252 ANSI code page. (All of the sample files only contain ASCII.)
    private static readonly Encoding s_encoding = CodePagesEncodingProvider.Instance.GetEncoding(1252)!;

    public OrganizationChartRecordId Id { get; }

    public int RecordSize => Bytes.Length;

    public byte[] Bytes { get; }

    internal OrganizationChartRecord(OrganizationChartRecordId id, byte[] bytes)
    {
        Id = id;
        Bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));
    }

    /// <summary>
    /// Creates the most specific record type for the given ID from the record's raw data
    /// (excluding the 2-byte ID and 2-byte length).
    /// </summary>
    /// <exception cref="InvalidDataException">The ID is a section marker, is unknown, or the data is too short for the record type.</exception>
    public static OrganizationChartRecord Create(OrganizationChartRecordId id, byte[] bytes)
    {
        if (bytes == null)
        {
            throw new ArgumentNullException(nameof(bytes));
        }

        return id switch
        {
            OrganizationChartRecordId.Copyright => new CopyrightRecord(id, bytes),
            OrganizationChartRecordId.Password => new PasswordRecord(id, bytes),
            OrganizationChartRecordId.ViewUnits => new ViewUnitsRecord(id, bytes),
            OrganizationChartRecordId.PageSetup => new PageSetupRecord(id, bytes),
            OrganizationChartRecordId.NumberFormat => new NumberFormatRecord(id, bytes),
            OrganizationChartRecordId.Formula => new FormulaRecord(id, bytes),
            OrganizationChartRecordId.NumericValue => new NumericValueRecord(id, bytes),
            OrganizationChartRecordId.ColorPalette => new ColorPaletteRecord(id, bytes),
            OrganizationChartRecordId.Unknown0x0002 => new Unknown0x0002Record(id, bytes),
            OrganizationChartRecordId.Unknown0x0003 => new Unknown0x0003Record(id, bytes),
            OrganizationChartRecordId.BoxTextPartCount => new BoxTextPartCountRecord(id, bytes),
            OrganizationChartRecordId.BoxTextPartIndex => new BoxTextPartIndexRecord(id, bytes),
            OrganizationChartRecordId.BoxTextPartType => new BoxTextPartTypeRecord(id, bytes),
            OrganizationChartRecordId.BodyInfo => new BodyInfoRecord(id, bytes),
            OrganizationChartRecordId.FontName => new FontNameRecord(id, bytes),
            OrganizationChartRecordId.TitlePartIndex => new TitlePartIndexRecord(id, bytes),
            OrganizationChartRecordId.Group => new GroupRecord(id, bytes),
            OrganizationChartRecordId.Box => new BoxRecord(id, bytes),
            OrganizationChartRecordId.BoxTemplate => new BoxTemplateRecord(id, bytes),
            OrganizationChartRecordId.TextStyle => new TextStyleRecord(id, bytes),
            OrganizationChartRecordId.BoxIndex => new BoxIndexRecord(id, bytes),
            OrganizationChartRecordId.Border => new BorderRecord(id, bytes),
            OrganizationChartRecordId.Connector => new ConnectorRecord(id, bytes),
            OrganizationChartRecordId.Line => new LineRecord(id, bytes),
            OrganizationChartRecordId.Shape => new ShapeRecord(id, bytes),
            OrganizationChartRecordId.ShapeColor => new ShapeColorRecord(id, bytes),
            OrganizationChartRecordId.Selected => new SelectedRecord(id, bytes),
            OrganizationChartRecordId.Shadow => new ShadowRecord(id, bytes),
            OrganizationChartRecordId.LineShadow => new ShadowRecord(id, bytes),
            // The border and shadow of the chart frame, drawn around the chart at the page margins.
            OrganizationChartRecordId.ChartFrameBorder => new BorderRecord(id, bytes),
            OrganizationChartRecordId.ChartFrameShadow => new ShadowRecord(id, bytes),
            OrganizationChartRecordId.GroupLayout => new GroupLayoutRecord(id, bytes),
            OrganizationChartRecordId.BoxLayout => new BoxLayoutRecord(id, bytes),
            OrganizationChartRecordId.RootGroup => new RootGroupRecord(id, bytes),
            OrganizationChartRecordId.PrintTimeCode => new PrintTimeCodeRecord(id, bytes),
            OrganizationChartRecordId.Arrow => new ArrowRecord(id, bytes),
            OrganizationChartRecordId.Anchor => new AnchorRecord(id, bytes),
            OrganizationChartRecordId.FontIndex => new FontIndexRecord(id, bytes),
            OrganizationChartRecordId.Text => new TextRecord(id, bytes),
            OrganizationChartRecordId.GlobalInfo => new GlobalInfoRecord(id, bytes),
            OrganizationChartRecordId.Footer => new FooterRecord(id, bytes),
            _ when (ushort)id >= 0x4000 =>
                throw new InvalidDataException($"0x{(ushort)id:X4} is a section marker, not a record."),
            // Known IDs that are not yet specialized use the base record.
            _ when Enum.IsDefined(typeof(OrganizationChartRecordId), id) => new OrganizationChartRecord(id, bytes),
            // Preserved records: Org Plus writes its object n (0 to 999) back as record 0x1000 + n.
            _ when (ushort)id is >= 0x1000 and <= 0x13E7 => new OrganizationChartRecord(id, bytes),
            _ => throw new InvalidDataException($"Unknown record ID: 0x{(ushort)id:X4}"),
        };
    }

    private void EnsureAvailable(int offset, int count)
    {
        if (offset < 0 || count < 0 || offset > Bytes.Length - count)
        {
            throw new InvalidDataException(
                $"Record 0x{(ushort)Id:X4} is too short: needs {count} byte(s) at offset {offset} but has {Bytes.Length} byte(s).");
        }
    }

    protected byte ReadByte(int offset)
    {
        EnsureAvailable(offset, 1);
        return Bytes[offset];
    }

    protected ushort ReadUInt16(int offset)
    {
        EnsureAvailable(offset, 2);
        return BitConverter.ToUInt16(Bytes, offset);
    }

    protected short ReadInt16(int offset)
    {
        EnsureAvailable(offset, 2);
        return BitConverter.ToInt16(Bytes, offset);
    }

    protected uint ReadUInt32(int offset)
    {
        EnsureAvailable(offset, 4);
        return BitConverter.ToUInt32(Bytes, offset);
    }

    /// <summary>
    /// Reads a 4-byte colour. The bytes are stored in the order red, blue, green, flag.
    /// The flag byte is 1 for the application's default colour and 0 for a colour the user picked.
    /// </summary>
    protected System.Drawing.Color ReadColor(int offset)
    {
        EnsureAvailable(offset, 4);
        byte red = Bytes[offset];
        byte blue = Bytes[offset + 1];
        byte green = Bytes[offset + 2];
        // byte _ = Bytes[offset + 3]; // ignored
        return System.Drawing.Color.FromArgb(red, green, blue);
    }

    /// <summary>
    /// Reads a 3-byte colour in the order red, green, blue. Only text colours use this order.
    /// </summary>
    protected System.Drawing.Color ReadRgbColor(int offset)
    {
        EnsureAvailable(offset, 3);
        return System.Drawing.Color.FromArgb(Bytes[offset], Bytes[offset + 1], Bytes[offset + 2]);
    }

    /// <summary>
    /// Reads a fixed-length string, dropping any trailing NUL terminators.
    /// </summary>
    protected string ReadString(int offset, int length)
    {
        EnsureAvailable(offset, length);
        while (length > 0 && Bytes[offset + length - 1] == 0)
        {
            length--;
        }

        return s_encoding.GetString(Bytes, offset, length);
    }

    protected string ReadNullTerminatedString(int offset, out int bytesRead)
    {
        EnsureAvailable(offset, 0);
        int endOffset = offset;
        while (endOffset < Bytes.Length && Bytes[endOffset] != 0)
        {
            endOffset++;
        }

        bytesRead = endOffset - offset + 1; // +1 for null terminator
        return s_encoding.GetString(Bytes, offset, endOffset - offset);
    }
}
