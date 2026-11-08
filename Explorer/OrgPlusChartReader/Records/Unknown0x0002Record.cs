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
    /// 4 in charts and 2 in the style file sample. The 1995 ORGCHART.EXE save routine always writes 4.
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
