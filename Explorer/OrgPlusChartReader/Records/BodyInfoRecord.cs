using System.ComponentModel;
using System.Drawing;

namespace OrgPlusChartReader;

public class BodyInfoRecord : OrganizationChartRecord
{
    public ushort Size { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    /// <summary>
    /// The number of boxes in the chart.
    /// </summary>
    public ushort BoxCount { get; set; }

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

    public Color BackgroundColor { get; set; }

    /// <summary>
    /// 0x00FF when the background is the default colour, otherwise 0.
    /// </summary>
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort DefaultColorMarker { get; set; }

    internal BodyInfoRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        int offset = 0;
        Size = ReadUInt16(offset);
        offset += 2;
        BoxCount = ReadUInt16(offset);
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
        BackgroundColor = ReadColor(offset);

        DefaultColorMarker = ReadUInt16(32);
    }
}
