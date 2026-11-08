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

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown1 { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown2 { get; set; }

    public Color FillColor { get; set; }

    /// <summary>
    /// The id of the box template (<see cref="BoxTemplateRecord.TemplateId"/>) that says which text fields
    /// the box shows, or 0xFFFF for a box with no text.
    /// </summary>
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort TemplateId { get; set; }

    internal BoxRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        Size = ReadUInt16(0);
        Type = (BoxType)ReadUInt16(2);
        Unknown1 = ReadUInt16(10);
        Unknown2 = ReadUInt16(12);
        FillColor = ReadColor(28);
        TemplateId = ReadUInt16(32);
    }

    public enum BoxType : ushort
    {
        Normal = 0x00,
        Assistant = 0x02,

        /// <summary>
        /// A box with no text and no template. Its purpose is unknown.
        /// </summary>
        Unknown0x04 = 0x04,
    }
}
