using System.ComponentModel;

namespace OrgPlusChartReader;

public class FontNameRecord : OrganizationChartRecord
{
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Index { get; set; }
    public string Value { get; set; }

    internal FontNameRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        Index = ReadUInt16(0);
        Value = ReadString(2, RecordSize - 2);
    }
}
