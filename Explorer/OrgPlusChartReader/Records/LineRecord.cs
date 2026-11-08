using System.ComponentModel;
using System.Drawing;

namespace OrgPlusChartReader;

public class LineRecord : OrganizationChartRecord
{
    public ushort Size { get; set; }

    public LineStyle Style { get; set; }

    public ushort Thickness { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown1 { get; set; }

    public Color Color { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown2 { get; set; }

    internal LineRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        int offset = 0;
        Size = ReadUInt16(offset);
        offset += 2;
        Style = (LineStyle)ReadUInt16(offset);
        offset += 2;
        Thickness = ReadUInt16(offset);
        offset += 2;
        Unknown1 = ReadUInt16(offset);
        offset += 2;
        Color = ReadColor(offset);
        offset += 4;
        Unknown2 = ReadUInt16(offset);
    }
}
