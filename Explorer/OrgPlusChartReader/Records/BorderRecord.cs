using System.ComponentModel;
using System.Drawing;

namespace OrgPlusChartReader;

/// <summary>
/// The border of a box or shape (0x0053). Record 0x0095 in the canvas section has the same layout.
/// </summary>
public class BorderRecord : OrganizationChartRecord
{
    public ushort Size { get; set; }

    public BorderStyle Style { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown { get; set; }

    /// <summary>
    /// Which edges have a shadow, as a sum: a base of 1 or 2 from the shadow type, plus 10 for the left
    /// edge, 20 for the right, 40 for the top and 80 for the bottom. 0 means no shadow.
    /// </summary>
    public ushort ShadowEdges { get; set; }

    public LineStyle LineStyle { get; set; }

    public Color Color { get; set; }

    /// <summary>
    /// The high byte of the program's internal colour value. 0xFF means the default colour.
    /// </summary>
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort ColorHighByte { get; set; }

    internal BorderRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        int offset = 0;

        Size = ReadUInt16(offset);
        offset += 2;

        Style = (BorderStyle)ReadUInt16(offset);
        offset += 2;

        Unknown = ReadUInt16(offset);
        offset += 2;

        ShadowEdges = ReadUInt16(offset);
        offset += 2;

        LineStyle = (LineStyle)ReadUInt16(offset);
        offset += 2;

        Color = ReadColor(offset);
        offset += 4;

        ColorHighByte = ReadUInt16(offset);
    }

    /// <summary>
    /// The base part of <see cref="ShadowEdges"/> (1 or 2), which depends on the shadow type.
    /// </summary>
    public int ShadowBase => ShadowEdges % 10;

    public bool HasLeftShadow => (ShadowEdges / 10 & 1) != 0;

    public bool HasRightShadow => (ShadowEdges / 10 & 2) != 0;

    public bool HasTopShadow => (ShadowEdges / 10 & 4) != 0;

    public bool HasBottomShadow => (ShadowEdges / 10 & 8) != 0;

    /// <summary>
    /// Border styles. The program computes the value from a pen code: 0 for no border, 0x0A for a hairline,
    /// 1 for the default width, otherwise 10 + width / 2.
    /// </summary>
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
