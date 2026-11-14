using System.ComponentModel;

namespace OrgPlusChartReader;

/// <summary>
/// A drawing shape (0x0083) in the drawing shapes section. The position is stored as the four edges
/// of the shape's bounding box, in page coordinates.
/// </summary>
public class ShapeRecord : OrganizationChartRecord
{
    public ushort Size { get; set; }

    public ShapeType Type { get; set; }

    public uint Top { get; set; }

    public uint Left { get; set; }

    public uint Right { get; set; }

    public uint Bottom { get; set; }

    /// <summary>
    /// For free lines, which diagonal of the bounding box is drawn (1 in every sample); otherwise 0.
    /// </summary>
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort DiagonalDirection { get; set; }

    internal ShapeRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        int offset = 0;

        Size = ReadUInt16(offset);
        offset += 2;

        Type = (ShapeType)ReadUInt16(offset);
        offset += 2;

        Top = ReadUInt32(offset);
        offset += 4;

        Left = ReadUInt32(offset);
        offset += 4;

        Right = ReadUInt32(offset);
        offset += 4;

        Bottom = ReadUInt32(offset);
        offset += 4;

        DiagonalDirection = ReadUInt16(offset);
    }

    public enum ShapeType : ushort
    {
        Rectangle = 0x01,
        Text = 0x02,
        HorizontalLine = 0x03,
        VerticalLine = 0x04,
        FreeLine = 0x05
    }
}
