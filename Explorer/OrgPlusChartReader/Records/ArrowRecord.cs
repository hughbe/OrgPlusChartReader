using System.ComponentModel;

namespace OrgPlusChartReader;

/// <summary>
/// An arrowhead on a line (0x0094). It follows the line it belongs to. No sample contains one; the layout
/// comes from the program's save code.
/// </summary>
public class ArrowRecord : OrganizationChartRecord
{
    public ushort Size { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown1 { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown2 { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown3 { get; set; }

    /// <summary>
    /// Which end of the line the arrowhead is on (0 or 1).
    /// </summary>
    public ushort End { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown4 { get; set; }

    /// <summary>
    /// The item the line belongs to, as in <see cref="LineRecord.ItemIndex"/>.
    /// </summary>
    public ushort ItemIndex { get; set; }

    internal ArrowRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        Size = ReadUInt16(0);
        Unknown1 = ReadUInt16(2);
        Unknown2 = ReadUInt16(4);
        Unknown3 = ReadUInt16(6);
        End = ReadUInt16(8);
        Unknown4 = ReadUInt16(10);
        ItemIndex = ReadUInt16(12);
    }
}
