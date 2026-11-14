using System.ComponentModel;

namespace OrgPlusChartReader;

public class GlobalInfoRecord : OrganizationChartRecord
{
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown1 { get; set; }

    /// <summary>
    /// 1 when the program draws a small marker in the corner of every box that has boxes below it.
    /// Microsoft Organization Chart always hides the markers and writes 0.
    /// </summary>
    public ushort ShowBranchMarkers { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    /// <summary>
    /// 1 when page-break and guide lines are drawn on screen (the program stores the inverse of a hide flag).
    /// </summary>
    public ushort ShowPageBreaks { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown4 { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown5 { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown6 { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    /// <summary>
    /// The zoom in tenths of a percent: 0 is Actual Size, a positive value a fixed zoom, and a negative value
    /// Size to Window at the absolute value.
    /// </summary>
    public short Magnification { get; set; }

    public GlobalGroupsStyle GroupsStyle { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort GroupStyleParameter { get; set; }

    /// <summary>
    /// The default style for assistant groups, using the same codes as <see cref="GroupsStyle"/> (25 in the samples).
    /// </summary>
    public ushort DefaultAssistantStyle { get; set; }

    /// <summary>The group id of the current box, or -1 when there is none.</summary>
    public short CurrentBoxGroupId { get; set; }

    /// <summary>The zero-based position of the current box within its group, or -1.</summary>
    public short CurrentBoxPosition { get; set; }

    /// <summary>
    /// 1 when a box is selected (its boxes section contains a <see cref="SelectedRecord"/>), otherwise 0.
    /// </summary>
    public ushort BoxSelected { get; set; }

    /// <summary>The chart window's horizontal scroll position.</summary>
    public ushort HorizontalScrollPosition { get; set; }

    /// <summary>The chart window's vertical scroll position.</summary>
    public ushort VerticalScrollPosition { get; set; }

    internal GlobalInfoRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        int offset = 0;
        Unknown1 = ReadUInt16(offset);
        offset += 2;
        ShowBranchMarkers = ReadUInt16(offset);
        offset += 2;
        ShowPageBreaks = ReadUInt16(offset);
        offset += 2;
        Unknown4 = ReadUInt16(offset);
        offset += 2;
        Unknown5 = ReadUInt16(offset);
        offset += 2;
        Unknown6 = ReadUInt16(offset);
        offset += 2;
        Magnification = ReadInt16(offset);
        offset += 2;
        GroupsStyle = (GlobalGroupsStyle)ReadUInt16(offset);
        offset += 2;
        GroupStyleParameter = ReadUInt16(offset);
        offset += 2;

        DefaultAssistantStyle = ReadUInt16(offset);
        offset += 2;

        CurrentBoxGroupId = ReadInt16(offset);
        offset += 2;

        CurrentBoxPosition = ReadInt16(offset);
        offset += 2;

        BoxSelected = ReadUInt16(offset);

        // Bytes 26-29 are never written by the program and contain leftover data.
        HorizontalScrollPosition = ReadUInt16(30);
        VerticalScrollPosition = ReadUInt16(32);
    }

    public enum GlobalGroupsStyle: ushort
    {
        Style1 = 0x00,
        Style2 = 0x02,
        Style3 = 0x03,
        Style4 = 0x04,
        Style5 = 0x05,
        Style6 = 0x06,
        CoManager = 0x22,
    }
}
