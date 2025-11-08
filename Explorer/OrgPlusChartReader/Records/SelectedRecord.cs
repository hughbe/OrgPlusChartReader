using System.ComponentModel;
using System.IO;

namespace OrgPlusChartReader;

public class SelectedRecord : OrganizationChartRecord
{
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown { get; set; }

    public SelectedRecord(OrganizationChartRecordId id, BinaryReader reader) : base(id, reader)
    {
        Unknown = ReadUInt16(0);
    }
}
