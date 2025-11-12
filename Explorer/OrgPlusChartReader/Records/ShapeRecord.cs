using System.ComponentModel;

namespace OrgPlusChartReader;

public class ShapeRecord : OrganizationChartRecord
{
    public ushort Size { get; set; }

    public ShapeType Type { get; set; }

    public uint Y { get; set; }

    public uint X { get; set; }

    public uint Width { get; set; }

    public uint Height { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown2 { get; set; }

    public ShapeRecord(OrganizationChartRecordId id, BinaryReader reader) : base(id, reader)
    {
        int offset = 0;
        Size = ReadUInt16(offset);
        offset += 2;
        Type = (ShapeType)ReadUInt16(offset);
        offset += 2;
        Y = ReadUInt32(offset);
        offset += 4;
        X = ReadUInt32(offset);
        offset += 4;
        Width = ReadUInt32(offset);
        offset += 4;
        Height = ReadUInt32(offset);
        offset += 4;
        Unknown2 = ReadUInt16(offset);
    }

    public enum ShapeType
    {
        Rectangle = 0x01,
        Text = 0x02,
        HorizontalLine = 0x03,
        VerticalLine = 0x04,
        CustomLine = 0x05
    }
}
