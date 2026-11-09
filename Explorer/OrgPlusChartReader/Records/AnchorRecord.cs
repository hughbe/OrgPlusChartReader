namespace OrgPlusChartReader;

/// <summary>
/// Anchors a drawing shape to a box (0x0097), so the shape moves with the box. It follows the shape's
/// other records. No sample contains one; the layout comes from the program's save code.
/// </summary>
public class AnchorRecord : OrganizationChartRecord
{
    public ushort Size { get; set; }

    /// <summary>The id of the box's group (see <see cref="GroupRecord.GroupId"/>).</summary>
    public ushort GroupId { get; set; }

    /// <summary>The one-based number of the box (see <see cref="BoxIndexRecord"/>).</summary>
    public ushort BoxIndex { get; set; }

    /// <summary>The horizontal offset of the shape's centre from the box's centre, in view units.</summary>
    public int OffsetX { get; set; }

    /// <summary>The vertical offset of the shape's centre from the box's centre, in view units.</summary>
    public int OffsetY { get; set; }

    /// <summary>The width of the box, less its inner margins, when the shape was anchored.</summary>
    public int BoxWidth { get; set; }

    /// <summary>The height of the box, less its inner margins, when the shape was anchored.</summary>
    public int BoxHeight { get; set; }

    internal AnchorRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        Size = ReadUInt16(0);
        GroupId = ReadUInt16(2);
        BoxIndex = ReadUInt16(4);
        OffsetX = (int)ReadUInt32(6);
        OffsetY = (int)ReadUInt32(10);
        BoxWidth = (int)ReadUInt32(14);
        BoxHeight = (int)ReadUInt32(18);
    }
}
