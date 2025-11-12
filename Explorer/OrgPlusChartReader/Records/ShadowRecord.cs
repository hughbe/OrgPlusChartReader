using System.Drawing;

namespace OrgPlusChartReader;

public class ShadowRecord : OrganizationChartRecord
{
    public ushort Size { get; set; }

    public ShadowType Type { get; set; }

    public short Height { get; set; }

    public short Width { get; set; }

    public Color FillColor { get; set; }

    public Color Color { get; set; }

    public ShadowRecord(OrganizationChartRecordId id, BinaryReader reader) : base(id, reader)
    {
        int offset = 0;
        Size = ReadUInt16(offset);
        offset += 2;
        Type = (ShadowType)ReadUInt16(offset);
        offset += 2;
        Height = ReadInt16(offset);
        offset += 2;
        Width = ReadInt16(offset);
        offset += 2;
        FillColor = ReadColor(offset);
        offset += 4;
        Color = ReadColor(offset);
    }

    public enum ShadowType
    {
        None,
        SideAndBottom = 0x01,
        SideAndBottomBevel = 0x02,
        TopOrBottom = 0x03,
        LeftBottomRight = 0x04
    }
}
