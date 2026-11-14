namespace OrgPlusChartReader;

/// <summary>
/// The size of a box and the spacing of its group (0x0054). The canvas section holds the default;
/// a box only has its own record when it differs from that default. All values are in view units.
/// </summary>
public class BoxLayoutRecord : OrganizationChartRecord
{
    public ushort Size { get; set; }

    /// <summary>The box's width (180, 1.25 inch, in the samples). Probably a minimum: the box grows to fit its text.</summary>
    public ushort BoxWidth { get; set; }

    /// <summary>The box's height (36, 0.25 inch, or 180 in the samples).</summary>
    public ushort BoxHeight { get; set; }

    /// <summary>The vertical gap from the parent box down to the horizontal connecting line.</summary>
    public ushort ParentToLineGap { get; set; }

    /// <summary>The vertical gap from the horizontal connecting line down to the group's boxes.</summary>
    public ushort LineToBoxGap { get; set; }

    /// <summary>The horizontal gap between neighbouring boxes, and between neighbouring branches.</summary>
    public ushort HorizontalGap { get; set; }

    /// <summary>The vertical gap between boxes that a layout style stacks in a column.</summary>
    public ushort VerticalGap { get; set; }

    /// <summary>Never set by the program when it writes a box's record: leftover data. Ignored on read.</summary>
    public ushort Leftover { get; set; }

    internal BoxLayoutRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        Size = ReadUInt16(0);
        BoxWidth = ReadUInt16(6);
        BoxHeight = ReadUInt16(8);
        ParentToLineGap = ReadUInt16(10);
        LineToBoxGap = ReadUInt16(12);
        HorizontalGap = ReadUInt16(14);
        VerticalGap = ReadUInt16(16);
        Leftover = ReadUInt16(18);
    }
}
