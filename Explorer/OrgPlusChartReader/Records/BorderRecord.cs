using System.ComponentModel;
using System.Drawing;
using System.IO;

namespace OrgPlusChartReader;

public class BorderRecord : OrganizationChartRecord
{
    public ushort Size { get; set; }

    public BorderStyle Style { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown1 { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown2 { get; set; }

    public LineStyle LineStyle { get; set; }

    public Color Color { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown3 { get; set; }

    public BorderRecord(OrganizationChartRecordId id, BinaryReader reader) : base(id, reader)
    {
        int offset = 0;
        Size = ReadUInt16(offset);
        offset += 2;
        Style = (BorderStyle)ReadUInt16(offset);
        offset += 2;
        Unknown1 = ReadUInt16(offset);
        offset += 2;
        Unknown2 = ReadUInt16(offset);
        offset += 2;
        LineStyle = (LineStyle)ReadUInt16(offset);
        offset += 2;
        Color = ReadColor(offset);
        offset += 4;
        Unknown3 = ReadUInt16(offset);
    }

    public enum BorderStyle : ushort
    {
        None = 0x00,
        Single1 = 0x01,
        Hairline = 0x0A,
        Single2 = 0x0C,
        Single3 = 0x0D,
        Single4 = 0x0E,
        Double1 = 0x1F,
        Double2 = 0x16,
        Double3 = 0x29,
        Double4 = 0x17,
        Double5 = 0x33,
        Double6 = 0x18
    }
}
