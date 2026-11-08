using System.ComponentModel;

namespace OrgPlusChartReader;

/// <summary>
/// A group (0x0041): a set of sibling boxes that share one parent box. Groups are written in pre-order,
/// so a parent group always comes before its children. The boxes follow in a <see cref="OrganizationChartRecordId.BoxesSection"/>.
/// </summary>
public class GroupRecord : OrganizationChartRecord
{
    public ushort Size { get; set; }

    /// <summary>
    /// The zero-based index of this group, in file order.
    /// </summary>
    public ushort GroupIndex { get; set; }

    /// <summary>
    /// The depth of the group in the chart's tree. The root group is 0.
    /// </summary>
    public ushort Depth { get; set; }

    /// <summary>
    /// The index of the parent group, or -1 for the root group.
    /// </summary>
    public short ParentGroupIndex { get; set; }

    /// <summary>
    /// The one-based index of the parent box (see <see cref="BoxIndexRecord"/>), or 0 for the root group.
    /// </summary>
    public ushort ParentBoxIndex { get; set; }

    /// <summary>
    /// The number of boxes in the group.
    /// </summary>
    public ushort Count { get; set; }

    /// <summary>
    /// The group's layout style. 0 means the chart default (<see cref="GlobalInfoRecord.GroupsStyle"/>);
    /// assistant groups use 2.
    /// </summary>
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort GroupStyle { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown1 { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public byte Unknown2 { get; set; }

    internal GroupRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        int offset = 0;

        Size = ReadUInt16(offset);
        offset += 2;

        GroupIndex = ReadUInt16(offset);
        offset += 2;

        Depth = ReadUInt16(offset);
        offset += 2;

        ParentGroupIndex = ReadInt16(offset);
        offset += 2;

        ParentBoxIndex = ReadUInt16(offset);
        offset += 2;

        Count = ReadUInt16(offset);
        offset += 2;

        GroupStyle = ReadUInt16(offset);
        offset += 2;

        Unknown1 = ReadUInt16(offset);
        offset += 2;

        Unknown2 = ReadByte(offset);
    }
}
