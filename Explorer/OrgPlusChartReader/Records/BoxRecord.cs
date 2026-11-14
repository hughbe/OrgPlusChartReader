using System.ComponentModel;
using System.Drawing;

namespace OrgPlusChartReader;

/// <summary>
/// A box (0x0051) in a boxes section. It follows the box's <see cref="BoxIndexRecord"/>.
/// </summary>
public class BoxRecord : OrganizationChartRecord
{
    public ushort Size { get; set; }

    public BoxType Type { get; set; }

    /// <summary>
    /// Whether the box is hidden (box flag 0x04, set by the Hide Box command). A box of type
    /// <see cref="BoxType.Hidden"/> is hidden too.
    /// </summary>
    public bool IsHidden { get; set; }

    /// <summary>Whether the branch below the box is hidden (box flag 0x10, set by the Hide Branch command).</summary>
    public bool IsBranchHidden { get; set; }

    /// <summary>
    /// Box flag 0x01. Set on every ordinary box written by Microsoft Organization Chart, which sets it
    /// ("Change the size of some boxes") on charts from other editions; clear on a silent position.
    /// </summary>
    public bool Flag0x01 { get; set; }

    /// <summary>Box flag 0x02. See <see cref="Flag0x01"/>.</summary>
    public bool Flag0x02 { get; set; }

    /// <summary>
    /// Box flag 0x800 (offset 14): when the chart is opened, the box's amount fields stop counting in
    /// calculations. Written as 0 by Microsoft Organization Chart.
    /// </summary>
    public bool AmountsNotCounted { get; set; }

    /// <summary>
    /// Box flag 0x200 (offset 18): the box is excluded from calculations, as with
    /// <see cref="OrganizationChartRecordId.ExcludeFromCalculations"/>. Written as 0 by Microsoft Organization Chart.
    /// </summary>
    public bool ExcludedFromCalculations { get; set; }

    /// <summary>Extra horizontal space before the box, in view units. Reset to 0 when the group's style changes.</summary>
    public uint SpaceBefore { get; set; }

    /// <summary>A second layout parameter of the group's style for this box, in view units. Reset with <see cref="SpaceBefore"/>.</summary>
    public uint LayoutParameter2 { get; set; }

    public Color FillColor { get; set; }

    /// <summary>
    /// The id of the box template (<see cref="BoxTemplateRecord.TemplateId"/>) that says which text fields
    /// the box shows, or 0xFFFF for a box with no text.
    /// </summary>
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort TemplateId { get; set; }

    /// <summary>
    /// The high byte of the program's internal fill colour value. 0xFF means the default colour.
    /// </summary>
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort ColorHighByte { get; set; }

    internal BoxRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        Size = ReadUInt16(0);
        Type = (BoxType)ReadUInt16(2);
        IsHidden = ReadUInt16(6) != 0 || Type == BoxType.Hidden;
        IsBranchHidden = ReadUInt16(8) != 0;
        Flag0x01 = ReadUInt16(10) != 0;
        Flag0x02 = ReadUInt16(12) != 0;
        AmountsNotCounted = ReadUInt16(14) != 0;
        ExcludedFromCalculations = ReadUInt16(18) != 0;
        SpaceBefore = ReadUInt32(20);
        LayoutParameter2 = ReadUInt32(24);
        FillColor = ReadColor(28);
        TemplateId = ReadUInt16(32);
        ColorHighByte = ReadUInt16(34);
    }

    public enum BoxType : ushort
    {
        Normal = 0x00,
        CoManager = 0x01,
        Assistant = 0x02,

        /// <summary>A hidden box. Never written by Microsoft Organization Chart, which reads it as a normal box with <see cref="IsHidden"/> set.</summary>
        Hidden = 0x03,


        /// <summary>
        /// A silent position: an invisible box with no text and no template. It ends a group of assistants.
        /// (A co-manager group has one too, but that one is not written to the file.)
        /// </summary>
        SilentPosition = 0x04,
    }
}
