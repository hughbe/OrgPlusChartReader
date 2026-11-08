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
    /// The group's number. In every sample this is its zero-based position in the file and also its depth
    /// in the tree, so which of the two it really is is unknown.
    /// </summary>
    public ushort GroupIndex { get; set; }

    /// <summary>
    /// A second copy of <see cref="GroupIndex"/>: the program writes the same field twice.
    /// </summary>
    public ushort GroupIndexCopy { get; set; }

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
    /// Non-zero (2 in the samples) for a group of assistants; otherwise 0.
    /// </summary>
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort AssistantIndicator { get; set; }

    /// <summary>
    /// Group flag 0x20.
    /// </summary>
    public bool Flag0x20 { get; set; }

    /// <summary>
    /// Group flag 0x40.
    /// </summary>
    public bool Flag0x40 { get; set; }

    /// <summary>
    /// Whether the group uses the co-manager layout.
    /// </summary>
    public bool IsCoManager { get; set; }

    internal GroupRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        Size = ReadUInt16(0);
        GroupIndex = ReadUInt16(2);
        GroupIndexCopy = ReadUInt16(4);
        ParentGroupIndex = ReadInt16(6);
        ParentBoxIndex = ReadUInt16(8);
        Count = ReadUInt16(10);
        AssistantIndicator = ReadUInt16(12);
        Flag0x20 = ReadByte(14) != 0;
        Flag0x40 = ReadByte(15) != 0;
        IsCoManager = ReadByte(16) != 0;
    }
}
