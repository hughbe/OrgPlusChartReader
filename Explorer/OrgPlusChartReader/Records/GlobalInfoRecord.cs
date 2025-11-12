using System.ComponentModel;

namespace OrgPlusChartReader;

public class GlobalInfoRecord : OrganizationChartRecord
{
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown1 { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown2 { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown3 { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown4 { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown5 { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown6 { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown7 { get; set; }

    public GlobalGroupsStyle GroupsStyle { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown8 { get; set; }

    public GlobalInfoRecord(OrganizationChartRecordId id, BinaryReader reader) : base(id, reader)
    {
        int offset = 0;
        Unknown1 = ReadUInt16(offset);
        offset += 2;
        Unknown2 = ReadUInt16(offset);
        offset += 2;
        Unknown3 = ReadUInt16(offset);
        offset += 2;
        Unknown4 = ReadUInt16(offset);
        offset += 2;
        Unknown5 = ReadUInt16(offset);
        offset += 2;
        Unknown6 = ReadUInt16(offset);
        offset += 2;
        Unknown7 = ReadUInt16(offset);
        offset += 2;
        GroupsStyle = (GlobalGroupsStyle)ReadUInt16(offset);
        offset += 2;
        Unknown8 = ReadUInt16(offset);
    }

    public enum GlobalGroupsStyle: ushort
    {
        Style1 = 0x00,
        Style2 = 0x02,
        Style3 = 0x03,
        Style4 = 0x04,
        Style5 = 0x05,
        CoManager = 0x22,
    }
}
