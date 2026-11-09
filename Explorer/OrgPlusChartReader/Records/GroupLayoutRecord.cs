using System.ComponentModel;

namespace OrgPlusChartReader;

/// <summary>
/// Layout settings for a group (0x0042). It follows the group's <see cref="GroupRecord"/>.
/// </summary>
public class GroupLayoutRecord : OrganizationChartRecord
{
    public ushort Size { get; set; }

    /// <summary>A layout parameter of the group's style, in view units. Reset to 0 when the style changes.</summary>
    public uint Measurement1 { get; set; }

    /// <summary>A second layout parameter of the group's style, in view units. Reset to 0 when the style changes.</summary>
    public uint Measurement2 { get; set; }

    /// <summary>
    /// The number of segments the group's connecting lines are drawn with: the most entries the group's
    /// lines section can have. The program recalculates it during layout and ignores it when reading.
    /// </summary>
    public ushort LineSegmentCount { get; set; }

    /// <summary>
    /// The group's layout style, using the same codes as <see cref="GlobalInfoRecord.GroupsStyle"/>.
    /// </summary>
    public ushort Style { get; set; }

    /// <summary>
    /// The layout style of the group's assistant part. The 1995 program writes the same value as <see cref="Style"/>.
    /// </summary>
    public ushort AssistantStyle { get; set; }

    /// <summary>
    /// A third layout parameter of the group's style. Only set for two styles, where it is 1 when the
    /// group's previous style was one of three particular (probably horizontal) styles.
    /// </summary>
    public ushort StyleParameter { get; set; }

    internal GroupLayoutRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        Size = ReadUInt16(0);
        Measurement1 = ReadUInt32(2);
        Measurement2 = ReadUInt32(6);
        LineSegmentCount = ReadUInt16(10);
        Style = ReadUInt16(12);
        AssistantStyle = ReadUInt16(14);
        StyleParameter = ReadUInt16(16);
    }
}
