using System.ComponentModel;

namespace OrgPlusChartReader;

/// <summary>
/// Links a drawing shape to a box (0x0097). No sample contains one; the layout comes from the program's
/// save code.
/// </summary>
public class ShapeBoxLinkRecord : OrganizationChartRecord
{
    public ushort Size { get; set; }

    /// <summary>
    /// Probably the group that contains the box.
    /// </summary>
    public ushort GroupIndex { get; set; }

    /// <summary>
    /// The one-based index of the box (see <see cref="BoxIndexRecord"/>).
    /// </summary>
    public ushort BoxIndex { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public uint Unknown1 { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public uint Unknown2 { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public uint Unknown3 { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public uint Unknown4 { get; set; }

    internal ShapeBoxLinkRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        Size = ReadUInt16(0);
        GroupIndex = ReadUInt16(2);
        BoxIndex = ReadUInt16(4);
        Unknown1 = ReadUInt32(6);
        Unknown2 = ReadUInt32(10);
        Unknown3 = ReadUInt32(14);
        Unknown4 = ReadUInt32(18);
    }
}
