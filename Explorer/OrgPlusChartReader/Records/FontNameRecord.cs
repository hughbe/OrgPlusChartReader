using System.ComponentModel;

namespace OrgPlusChartReader;

public class FontNameRecord : OrganizationChartRecord
{
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Index { get; set; }
    public string Value { get; set; }

    public FontNameRecord(OrganizationChartRecordId id, BinaryReader reader) : base(id, reader)
    {
        Index = ReadUInt16(0);
        Value = ReadASCIIString(2, RecordSize - 2);
    }
}
