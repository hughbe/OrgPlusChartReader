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

    /// <summary>
    /// The box's shape. Microsoft Organization Chart always writes <see cref="BoxShape.Rectangle"/>.
    /// </summary>
    public BoxShape Shape { get; set; }

    /// <summary>
    /// A summary of the shadow record that follows, which replaces it when the program reads the file.
    /// Which edges have a shadow, as a sum: a base of 1 or 2 from the shadow type, plus 10 for the left
    /// edge, 20 for the right, 40 for the top and 80 for the bottom. 0 means no shadow.
    /// </summary>
    public ushort ShadowEdges { get; set; }

    public LineStyle LineStyle { get; set; }

    public Color Color { get; set; }

    /// <summary>
    /// The high byte of the program's internal colour value. 0xFF means the default colour.
    /// The program ignores it when reading.
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

        Shape = (BoxShape)ReadUInt16(offset);
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

    public enum BoxShape : ushort
    {
        Rectangle = 0,

        /// <summary>A rounded rectangle whose corner radius is a quarter of the box's shorter side.</summary>
        Rounded = 1,

        /// <summary>A rectangle with each corner cut off diagonally, a fifth of the shorter side along each edge.</summary>
        Octagon = 2,
    }

    /// <summary>
    /// Border styles. A border is an outer line, a gap and an inner line, with widths in view units (half
    /// points). The program computes the value from a pen code (1500 + outer width + 10 x inner width):
    /// 0 for no border, 0x0A for a hairline, 1 for the default width, otherwise 10 + (code - 1500) / 2.
    /// When reading it also accepts 2 (as 1), 3 (1 with a dashed line) and 4 or 5 (as 0x16).
    /// </summary>
    public enum BorderStyle : ushort
    {
        None = 0x00,
        /// <summary>A single 1 point line (width 2): the default.</summary>
        Single1 = 0x01,
        Hairline = 0x0A,
        Single2 = 0x0C,
        Single3 = 0x0D,
        Single4 = 0x0E,
        /// <summary>Double, outer 2, inner 4.</summary>
        Double1 = 0x1F,
        /// <summary>Double, outer 4, inner 2.</summary>
        Double2 = 0x16,
        /// <summary>Double, outer 2, inner 6.</summary>
        Double3 = 0x29,
        /// <summary>Double, outer 6, inner 2.</summary>
        Double4 = 0x17,
        /// <summary>Double, outer 2, inner 8.</summary>
        Double5 = 0x33,
        /// <summary>Double, outer 8, inner 2.</summary>
        Double6 = 0x18
    }
}
