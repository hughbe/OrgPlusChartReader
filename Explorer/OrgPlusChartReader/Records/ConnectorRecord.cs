using System.ComponentModel;

namespace OrgPlusChartReader;

/// <summary>
/// A user-drawn connector between two boxes (0x0055). It is followed by its <see cref="LineRecord"/>.
/// </summary>
public class ConnectorRecord : OrganizationChartRecord
{
    public ushort Size { get; set; }

    /// <summary>The one-based index of the box the connector starts at.</summary>
    public ushort FromBoxIndex { get; set; }

    /// <summary>The index of the group that contains the start box.</summary>
    public ushort FromGroupIndex { get; set; }

    /// <summary>Probably the side of the start box the connector is attached to (3 or 4 in the samples).</summary>
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort FromAnchor { get; set; }

    /// <summary>The one-based index of the box the connector ends at.</summary>
    public ushort ToBoxIndex { get; set; }

    /// <summary>The index of the group that contains the end box.</summary>
    public ushort ToGroupIndex { get; set; }

    /// <summary>Probably the side of the end box the connector is attached to (3 or 4 in the samples).</summary>
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort ToAnchor { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public uint Unknown1 { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public uint Unknown2 { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public uint Unknown3 { get; set; }

    internal ConnectorRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        int offset = 0;

        Size = ReadUInt16(offset);
        offset += 2;

        FromBoxIndex = ReadUInt16(offset);
        offset += 2;

        FromGroupIndex = ReadUInt16(offset);
        offset += 2;

        FromAnchor = ReadUInt16(offset);
        offset += 2;

        ToBoxIndex = ReadUInt16(offset);
        offset += 2;

        ToGroupIndex = ReadUInt16(offset);
        offset += 2;

        ToAnchor = ReadUInt16(offset);
        offset += 2;

        Unknown1 = ReadUInt32(offset);
        offset += 4;

        Unknown2 = ReadUInt32(offset);
        offset += 4;

        Unknown3 = ReadUInt32(offset);
    }
}
