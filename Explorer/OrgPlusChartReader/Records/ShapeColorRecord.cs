using System.ComponentModel;
using System.Drawing;

namespace OrgPlusChartReader;

public class ShapeColorRecord : OrganizationChartRecord
{
    public ushort Size { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown1 { get; set; }

    public Color Color { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown2 { get; set; }

    public ShapeColorRecord(OrganizationChartRecordId id, BinaryReader reader) : base(id, reader)
    {
        int offset = 0;
        Size = ReadUInt16(offset);
        offset += 2;
        Unknown1 = ReadUInt16(offset);
        offset += 2;
        Color = ReadColor(offset);
        offset += 4;
        Unknown2 = ReadUInt16(offset);
    }
}
