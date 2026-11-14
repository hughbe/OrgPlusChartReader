using System.ComponentModel;

namespace OrgPlusChartReader;

/// <summary>
/// Layout settings for a group (0x0042). It follows the group's <see cref="GroupRecord"/>.
/// </summary>
public class GroupLayoutRecord : OrganizationChartRecord
{
    public ushort Size { get; set; }

    /// <summary>
    /// In view units. The position of the group's stem when <see cref="StemPosition"/> is 0xFFFF; otherwise
    /// a value the program stores but its layout code does not use. Reset to 0 when the style changes.
    /// </summary>
    public uint Measurement1 { get; set; }

    /// <summary>
    /// An extra drop, in view units: added to the vertical distance between the parent box and the group,
    /// on top of the two gaps in <see cref="BoxLayoutRecord"/>. Reset to 0 when the style changes.
    /// </summary>
    public uint ExtraDrop { get; set; }

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
    /// Where the line from the parent box meets the group: 0 centred, 1 at the first box, 2 at the last
    /// box, 0xFFFF at the position in <see cref="Measurement1"/> (measured from the left of the group).
    /// </summary>
    public ushort StemPosition { get; set; }

    internal GroupLayoutRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        Size = ReadUInt16(0);
        Measurement1 = ReadUInt32(2);
        ExtraDrop = ReadUInt32(6);
        LineSegmentCount = ReadUInt16(10);
        Style = ReadUInt16(12);
        AssistantStyle = ReadUInt16(14);
        StemPosition = ReadUInt16(16);
    }
}
