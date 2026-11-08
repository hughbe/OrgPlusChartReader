using System.ComponentModel;

namespace OrgPlusChartReader;

public class FontIndexRecord : OrganizationChartRecord
{
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Index { get; set; }

    internal FontIndexRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        Index = ReadUInt16(0);
    }
}
