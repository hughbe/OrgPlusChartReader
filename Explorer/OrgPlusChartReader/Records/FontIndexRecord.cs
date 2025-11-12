using System.ComponentModel;

namespace OrgPlusChartReader;

public class FontIndexRecord : OrganizationChartRecord
{
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Index { get; set; }

    public FontIndexRecord(OrganizationChartRecordId id, BinaryReader reader) : base(id, reader)
    {
        Index = ReadUInt16(0);
    }
}
