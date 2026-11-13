using System.ComponentModel;

namespace OrgPlusChartReader;

/// <summary>
/// A version record (0x0002). The program writes version 1010 and the date 26 December 1992.
/// </summary>
public class Unknown0x0002Record : OrganizationChartRecord
{
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Size { get; set; }

    /// <summary>
    /// Which program wrote the file. Microsoft Organization Chart always writes 4. Any other value (the
    /// style file sample has 2) makes it treat the chart as one from another Org Plus edition and convert it.
    /// </summary>
    public DocumentKind Kind { get; set; }

    public ushort Version { get; set; }

    public byte Day { get; set; }

    public byte Month { get; set; }

    /// <summary>The year, as two digits (92).</summary>
    public ushort Year { get; set; }

    internal Unknown0x0002Record(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        Size = ReadUInt16(0);
        Kind = (DocumentKind)ReadUInt16(2);
        Version = ReadUInt16(4);
        Day = ReadByte(6);
        Month = ReadByte(7);
        Year = ReadUInt16(8);
    }

    public enum DocumentKind : ushort
    {
        Style = 0x02,
        Chart = 0x04,
    }
}
