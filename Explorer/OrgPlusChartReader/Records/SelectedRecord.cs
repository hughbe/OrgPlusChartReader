using System.ComponentModel;

namespace OrgPlusChartReader;

public class SelectedRecord : OrganizationChartRecord
{
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown { get; set; }

    internal SelectedRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        Unknown = ReadUInt16(0);
    }
}
