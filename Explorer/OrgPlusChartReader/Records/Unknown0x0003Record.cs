using System.ComponentModel;

namespace OrgPlusChartReader;

/// <summary>
/// A version record (0x0003). The program always writes version 1001 and the date 26 December 1992.
/// </summary>
public class Unknown0x0003Record : OrganizationChartRecord
{
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Size { get; set; }

    public ushort Version { get; set; }

    public byte Day { get; set; }

    public byte Month { get; set; }

    /// <summary>The year, as two digits (92).</summary>
    public ushort Year { get; set; }

    internal Unknown0x0003Record(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        Size = ReadUInt16(0);
        Version = ReadUInt16(2);
        Day = ReadByte(4);
        Month = ReadByte(5);
        Year = ReadUInt16(6);
    }
}
