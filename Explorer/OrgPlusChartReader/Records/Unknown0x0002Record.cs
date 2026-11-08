using System.ComponentModel;

namespace OrgPlusChartReader;

public class Unknown0x0002Record : OrganizationChartRecord
{
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Size { get; set; }

    public DocumentKind Kind { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown2 { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown3 { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown4 { get; set; }

    internal Unknown0x0002Record(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        int offset = 0;

        // Size (2 bytes).
        Size = ReadUInt16(offset);
        offset += 2;

        // Kind (2 bytes).
        Kind = (DocumentKind)ReadUInt16(offset);
        offset += 2;

        // Unknown2 (2 bytes).
        Unknown2 = ReadUInt16(offset);
        offset += 2;

        // Unknown3 (2 bytes).
        Unknown3 = ReadUInt16(offset);
        offset += 2;

        // Unknown4 (2 bytes).
        Unknown4 = ReadUInt16(offset);
        offset += 2;
    }

    public enum DocumentKind : ushort
    {
        Style = 0x02,
        Chart = 0x04,
    }
}
