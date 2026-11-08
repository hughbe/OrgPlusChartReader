using System.ComponentModel;

namespace OrgPlusChartReader;

/// <summary>
/// Layout settings for a group (0x0042). It follows the group's <see cref="GroupRecord"/>.
/// </summary>
public class GroupLayoutRecord : OrganizationChartRecord
{
    public ushort Size { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public uint Unknown1 { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public uint Unknown2 { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown3 { get; set; }

    /// <summary>
    /// The group's layout style, using the same codes as <see cref="GlobalInfoRecord.GroupsStyle"/>.
    /// </summary>
    public ushort Style { get; set; }

    /// <summary>
    /// A second copy of <see cref="Style"/>: the program writes the same field twice.
    /// </summary>
    public ushort StyleCopy { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown4 { get; set; }

    internal GroupLayoutRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        Size = ReadUInt16(0);
        Unknown1 = ReadUInt32(2);
        Unknown2 = ReadUInt32(6);
        Unknown3 = ReadUInt16(10);
        Style = ReadUInt16(12);
        StyleCopy = ReadUInt16(14);
        Unknown4 = ReadUInt16(16);
    }
}
