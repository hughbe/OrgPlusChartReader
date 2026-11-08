using System.ComponentModel;

namespace OrgPlusChartReader;

public class FooterRecord : OrganizationChartRecord
{
    public ushort Size { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public uint Crc { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public uint FileSize { get; set; }

    internal FooterRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        int offset = 0;
        Size = ReadUInt16(offset);
        offset += 2;
        Crc = ReadUInt32(offset);
        offset += 4;
        FileSize = ReadUInt32(offset);
    }
}
