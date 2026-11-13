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
    /// A second colour, white in every sample: the highlight of shadow types 5 to 9.
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

    /// <summary>
    /// Shadow types of a box or shape shadow (0 to 9). A line shadow (0x0093) uses different codes:
    /// 0, 1, 2, 7 and 8 for the program's line shadow types 0 to 4.
    /// </summary>
    public enum ShadowType : ushort
    {
        None = 0x00,

        /// <summary>A drop shadow: one filled copy of the shape, moved by the offsets.</summary>
        SideAndBottom = 0x01,

        /// <summary>A block: outlines of the shape stepped towards the offsets, so the box looks extruded.</summary>
        SideAndBottomBevel = 0x02,

        /// <summary>Tapered: outlines stepped up or down, each narrower than the last.</summary>
        TopOrBottom = 0x03,

        /// <summary>Flared: outlines stepped up or down, each wider than the last.</summary>
        LeftBottomRight = 0x04,

        /// <summary>Raised: stepped outlines in the shadow colour on one side and the secondary colour on the other.</summary>
        Raised = 0x05,

        /// <summary>Sunken: as <see cref="Raised"/> with the two colours swapped.</summary>
        Sunken = 0x06,

        /// <summary>A raised bevelled frame around the box (the "3D" style).</summary>
        ThreeD = 0x07,

        /// <summary>A sunken bevelled frame: as <see cref="ThreeD"/> with the two colours swapped.</summary>
        SunkenFrame = 0x08,

        /// <summary>A copy of the border moved by the offsets, in the shadow colours.</summary>
        Outline = 0x09
    }
}
