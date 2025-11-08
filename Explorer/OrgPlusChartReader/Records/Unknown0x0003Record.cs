using System.ComponentModel;
using System.IO;

namespace OrgPlusChartReader;

public class Unknown0x0003Record : OrganizationChartRecord
{
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Size { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown1 { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown2 { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown3 { get; set; }

    public Unknown0x0003Record(OrganizationChartRecordId id, BinaryReader reader) : base(id, reader)
    {
        int offset = 0;

        // Size (2 bytes).
        Size = ReadUInt16(offset);
        offset += 2;

        // Unknown1 (2 bytes).
        Unknown1 = ReadUInt16(offset);
        offset += 2;

        // Unknown2 (2 bytes).
        Unknown2 = ReadUInt16(offset);
        offset += 2;

        // Unknown3 (2 bytes).
        Unknown3 = ReadUInt16(offset);
        offset += 2;
    }
}