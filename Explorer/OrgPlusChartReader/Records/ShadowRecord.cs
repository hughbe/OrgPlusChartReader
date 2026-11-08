using System.Drawing;

namespace OrgPlusChartReader;

/// <summary>
/// The shadow of a box or shape (0x0092). The line shadow (0x0093) and record 0x0096 in the canvas
/// section have the same layout.
/// </summary>
public class ShadowRecord : OrganizationChartRecord
{
    public ushort Size { get; set; }

    public ShadowType Type { get; set; }

    /// <summary>
    /// The vertical offset of the shadow: positive is down, negative is up.
    /// </summary>
    public short OffsetY { get; set; }

    /// <summary>
    /// The horizontal offset of the shadow: positive is right, negative is left.
    /// </summary>
    public short OffsetX { get; set; }

    public Color Color { get; set; }

    /// <summary>
    /// A second colour, white in every sample. Possibly the highlight of a bevelled shadow.
    /// </summary>
    public Color SecondaryColor { get; set; }

    internal ShadowRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        int offset = 0;

        Size = ReadUInt16(offset);
        offset += 2;

        Type = (ShadowType)ReadUInt16(offset);
        offset += 2;

        OffsetY = ReadInt16(offset);
        offset += 2;

        OffsetX = ReadInt16(offset);
        offset += 2;

        Color = ReadColor(offset);
        offset += 4;

        SecondaryColor = ReadColor(offset);
    }

    public enum ShadowType : ushort
    {
        None = 0x00,
        SideAndBottom = 0x01,
        SideAndBottomBevel = 0x02,
        TopOrBottom = 0x03,
        LeftBottomRight = 0x04,
        ThreeD = 0x07
    }
}
