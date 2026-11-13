using System.ComponentModel;

namespace OrgPlusChartReader;

/// <summary>
/// A user-drawn connector between two boxes (0x0055), called an "aux line" in the program's menus. It is followed by its <see cref="LineRecord"/>.
/// </summary>
public class ConnectorRecord : OrganizationChartRecord
{
    public ushort Size { get; set; }

    /// <summary>The one-based index of the box the connector starts at.</summary>
    public ushort FromBoxIndex { get; set; }

    /// <summary>The id (<see cref="GroupRecord.GroupId"/>) of the group that contains the start box.</summary>
    public ushort FromGroupId { get; set; }

    /// <summary>The edge of the start box the connector leaves from.</summary>
    public Anchor FromAnchor { get; set; }

    /// <summary>The one-based index of the box the connector ends at.</summary>
    public ushort ToBoxIndex { get; set; }

    /// <summary>The id (<see cref="GroupRecord.GroupId"/>) of the group that contains the end box.</summary>
    public ushort ToGroupId { get; set; }

    /// <summary>The edge of the end box the connector arrives at.</summary>
    public Anchor ToAnchor { get; set; }

    /// <summary>
    /// The offsets, in view units, of the connector's (up to three) movable middle segments. Each is the
    /// distance of one segment from the edge it is measured from; the first is how far the line runs out
    /// from the start box before its first bend.
    /// </summary>
    public uint Distance1 { get; set; }

    /// <summary>See <see cref="Distance1"/>.</summary>
    public uint Distance2 { get; set; }

    /// <summary>See <see cref="Distance1"/>.</summary>
    public uint Distance3 { get; set; }

    internal ConnectorRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        int offset = 0;

        Size = ReadUInt16(offset);
        offset += 2;

        FromBoxIndex = ReadUInt16(offset);
        offset += 2;

        FromGroupId = ReadUInt16(offset);
        offset += 2;

        FromAnchor = (Anchor)ReadUInt16(offset);
        offset += 2;

        ToBoxIndex = ReadUInt16(offset);
        offset += 2;

        ToGroupId = ReadUInt16(offset);
        offset += 2;

        ToAnchor = (Anchor)ReadUInt16(offset);
        offset += 2;

        Distance1 = ReadUInt32(offset);
        offset += 4;

        Distance2 = ReadUInt32(offset);
        offset += 4;

        Distance3 = ReadUInt32(offset);
    }

    /// <summary>
    /// A box edge, in the order of a Windows RECT. The program draws the connector from the end with the
    /// higher-priority anchor: top, bottom, left, right, then any.
    /// </summary>
    public enum Anchor : ushort
    {
        Any = 0,
        Left = 1,
        Top = 2,
        Right = 3,
        Bottom = 4,
    }
}
