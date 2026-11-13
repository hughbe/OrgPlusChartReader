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
    /// The group's id in the program's object table. Connectors (<see cref="ConnectorRecord"/>) refer to
    /// groups by this id. Ids are assigned when groups are created, so in a new chart they match file order.
    /// </summary>
    public ushort GroupId { get; set; }

    /// <summary>
    /// The id the program uses for the group's assistant part when it reads the file. The program writes
    /// <see cref="GroupId"/> here too.
    /// </summary>
    public ushort AssistantPartGroupId { get; set; }

    /// <summary>
    /// The <see cref="GroupId"/> of the parent group, or -1 for the root group.
    /// </summary>
    public short ParentGroupId { get; set; }

    /// <summary>
    /// The number of the parent box (see <see cref="BoxIndexRecord"/>), or 0 for the root group. For a group
    /// below co-managers this is the number of the co-manager group's first box.
    /// </summary>
    public ushort ParentBoxNumber { get; set; }

    /// <summary>
    /// The number of boxes in the group. A group of assistants counts the silent position that ends it;
    /// a co-manager group does not.
    /// </summary>
    public ushort Count { get; set; }

    /// <summary>
    /// How many of the group's <see cref="Count"/> boxes are assistants. Microsoft Organization Chart writes
    /// either 0 or <see cref="Count"/>.
    /// </summary>
    public ushort AssistantCount { get; set; }

    /// <summary>
    /// Group flag 0x20. When set, a silent position is allowed anywhere in the group.
    /// </summary>
    public bool Flag0x20 { get; set; }

    /// <summary>
    /// Group flag 0x40. The program ignores it when reading.
    /// </summary>
    public bool Flag0x40 { get; set; }

    /// <summary>
    /// Whether the group uses the co-manager layout.
    /// </summary>
    public bool IsCoManager { get; set; }

    internal GroupRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        Size = ReadUInt16(0);
        GroupId = ReadUInt16(2);
        AssistantPartGroupId = ReadUInt16(4);
        ParentGroupId = ReadInt16(6);
        ParentBoxNumber = ReadUInt16(8);
        Count = ReadUInt16(10);
        AssistantCount = ReadUInt16(12);
        Flag0x20 = ReadByte(14) != 0;
        Flag0x40 = ReadByte(15) != 0;
        IsCoManager = ReadByte(16) != 0;
    }
}
