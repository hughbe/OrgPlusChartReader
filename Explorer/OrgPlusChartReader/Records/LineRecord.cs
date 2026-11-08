using System.ComponentModel;
using System.Drawing;

namespace OrgPlusChartReader;

/// <summary>
/// A line (0x0056): the default line in the canvas section, a group's connecting line in a lines section,
/// a connector's line, or a drawing shape's line.
/// </summary>
public class LineRecord : OrganizationChartRecord
{
    public ushort Size { get; set; }

    public LineStyle Style { get; set; }

    /// <summary>
    /// The pen width: 1 for a hairline, otherwise twice the thickness chosen in the UI (2, 4, … 12).
    /// </summary>
    public ushort Thickness { get; set; }

    /// <summary>
    /// The item the line belongs to. In a group's lines section this is the zero-based position of the
    /// box within the group; elsewhere it is 0.
    /// </summary>
    public ushort ItemIndex { get; set; }

    public Color Color { get; set; }

    /// <summary>
    /// The high byte of the program's internal colour value. 0xFF means the default colour.
    /// </summary>
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort ColorHighByte { get; set; }

    internal LineRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        int offset = 0;

        Size = ReadUInt16(offset);
        offset += 2;

        Style = (LineStyle)ReadUInt16(offset);
        offset += 2;

        Thickness = ReadUInt16(offset);
        offset += 2;

        ItemIndex = ReadUInt16(offset);
        offset += 2;

        Color = ReadColor(offset);
        offset += 4;

        ColorHighByte = ReadUInt16(offset);
    }
}
