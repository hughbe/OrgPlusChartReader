using System.ComponentModel;

namespace OrgPlusChartReader;

public class ConnectorRecord : OrganizationChartRecord
{
    public ushort Size { get; set; }

    public ushort OriginIndex { get; set; }

    public ushort OriginLevel { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown3 { get; set; }

    public ushort TargetIndex { get; set; }

    public ushort TargetLevel { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown4 { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown5 { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown6 { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown7 { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown8 { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown9 { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown10 { get; set; }

    public ConnectorRecord(OrganizationChartRecordId id, BinaryReader reader) : base(id, reader)
    {
        int offset = 0;
        Size = ReadUInt16(offset);
        offset += 2;
        OriginIndex = ReadUInt16(offset);
        offset += 2;
        OriginLevel = ReadUInt16(offset);
        offset += 2;
        Unknown3 = ReadUInt16(offset);
        offset += 2;
        TargetIndex = ReadUInt16(offset);
        offset += 2;
        TargetLevel = ReadUInt16(offset);
        offset += 2;
        Unknown4 = ReadUInt16(offset);
        offset += 2;
        Unknown5 = ReadUInt16(offset);
        offset += 2;
        Unknown6 = ReadUInt16(offset);
        offset += 2;
        Unknown7 = ReadUInt16(offset);
        offset += 2;
        Unknown8 = ReadUInt16(offset);
        offset += 2;
        Unknown9 = ReadUInt16(offset);
        offset += 2;
        Unknown10 = ReadUInt16(offset);
    }
}
