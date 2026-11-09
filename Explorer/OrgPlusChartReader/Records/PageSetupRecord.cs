using System.ComponentModel;

namespace OrgPlusChartReader;

/// <summary>
/// Page and layout setup (0x0022). Measurements are in view units (see <see cref="ViewUnitsRecord"/>).
/// </summary>
public class PageSetupRecord : OrganizationChartRecord
{
    /// <summary>The size of the fixed part of the record (44).</summary>
    public ushort Size { get; set; }

    /// <summary>
    /// The layout direction. The program only supports 0 (top to bottom) and refuses other values.
    /// </summary>
    public ushort LayoutDirection { get; set; }

    public PageOrientation Orientation { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown1 { get; set; }

    /// <summary>Document flag 0x00040000.</summary>
    public bool Flag8 { get; set; }

    /// <summary>Document flag 0x00020000.</summary>
    public bool Flag10 { get; set; }

    /// <summary>The left margin.</summary>
    public uint LeftMargin { get; set; }

    /// <summary>The right margin.</summary>
    public uint RightMargin { get; set; }

    /// <summary>The top margin.</summary>
    public uint TopMargin { get; set; }

    /// <summary>The bottom margin.</summary>
    public uint BottomMargin { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public uint Unknown2 { get; set; }

    /// <summary>The paper size, as a Windows <c>DEVMODE</c> <c>dmPaperSize</c> value (1 = Letter).</summary>
    public ushort PaperSize { get; set; }

    /// <summary>
    /// Whether the chart prints to the default Windows printer rather than the printer saved in <see cref="Printer"/>.
    /// </summary>
    public bool UseDefaultPrinter { get; set; }

    /// <summary>The horizontal centre line of the chart's top box.</summary>
    public uint AnchorX { get; set; }

    /// <summary>The top of the chart.</summary>
    public uint AnchorY { get; set; }

    /// <summary>The printer as <c>device,driver,port</c>, or empty.</summary>
    public string Printer { get; set; }

    internal PageSetupRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        Size = ReadUInt16(0);
        LayoutDirection = ReadUInt16(2);
        Orientation = (PageOrientation)ReadUInt16(4);
        Unknown1 = ReadUInt16(6);
        Flag8 = ReadUInt16(8) != 0;
        Flag10 = ReadUInt16(10) != 0;
        LeftMargin = ReadUInt32(12);
        RightMargin = ReadUInt32(16);
        TopMargin = ReadUInt32(20);
        BottomMargin = ReadUInt32(24);
        Unknown2 = ReadUInt32(28);
        PaperSize = ReadUInt16(32);
        UseDefaultPrinter = ReadUInt16(34) != 0;
        AnchorX = ReadUInt32(36);
        AnchorY = ReadUInt32(40);
        Printer = ReadNullTerminatedString(44, out _);
    }

    public enum PageOrientation : ushort
    {
        Portrait = 0,
        Landscape = 1,
    }
}
