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

    /// <summary>Box flag 0x04: the flag set by the program's show/hide position command.</summary>
    public bool Flag0x04 { get; set; }

    /// <summary>Box flag 0x10.</summary>
    public bool Flag0x10 { get; set; }

    /// <summary>Box flag 0x01: managed by the show/hide branch command; set on every visible box in the samples.</summary>
    public bool Flag0x01 { get; set; }

    /// <summary>Box flag 0x02: managed by the show/hide branch command; set on every visible box in the samples.</summary>
    public bool Flag0x02 { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public uint Unknown1 { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public uint Unknown2 { get; set; }

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
        Flag0x04 = ReadUInt16(6) != 0;
        Flag0x10 = ReadUInt16(8) != 0;
        Flag0x01 = ReadUInt16(10) != 0;
        Flag0x02 = ReadUInt16(12) != 0;
        Unknown1 = ReadUInt32(20);
        Unknown2 = ReadUInt32(24);
        FillColor = ReadColor(28);
        TemplateId = ReadUInt16(32);
        ColorHighByte = ReadUInt16(34);
    }

    public enum BoxType : ushort
    {
        Normal = 0x00,
        CoManager = 0x01,
        Assistant = 0x02,

        /// <summary>
        /// A box with box flag 0x80 set. It has no text and no template; its purpose is unknown.
        /// </summary>
        Unknown0x04 = 0x04,
    }
}
