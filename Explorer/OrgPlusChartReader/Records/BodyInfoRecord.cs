using System.ComponentModel;
using System.Drawing;

namespace OrgPlusChartReader;

public class BodyInfoRecord : OrganizationChartRecord
{
    public ushort Size { get; set; }

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

    public Color Color { get; set; }

    public BodyInfoRecord(OrganizationChartRecordId id, BinaryReader reader) : base(id, reader)
    {
        int offset = 0;
        Size = ReadUInt16(offset);
        offset += 2;
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
        Color = ReadColor(offset);
    }
}
