using System.ComponentModel;
using System.IO;

namespace OrgPlusChartReader;

public class BoxIndexRecord : OrganizationChartRecord
{
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Index { get; set; }
   
    public BoxIndexRecord(OrganizationChartRecordId id, BinaryReader reader) : base(id, reader)
    {
        Index = ReadUInt16(0);
    }
}
