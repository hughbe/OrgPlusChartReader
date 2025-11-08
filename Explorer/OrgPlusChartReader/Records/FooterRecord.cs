using System.ComponentModel;
using System.IO;

namespace OrgPlusChartReader;

public class FooterRecord : OrganizationChartRecord
{
    public ushort Size { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public uint Crc { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public uint FileSize { get; set; }

    public FooterRecord(OrganizationChartRecordId id, BinaryReader reader) : base(id, reader)
    {
        int offset = 0;
        Size = ReadUInt16(offset);
        offset += 2;
        Crc = ReadUInt32(offset);
        offset += 4;
        FileSize = ReadUInt32(offset);
    }
}
