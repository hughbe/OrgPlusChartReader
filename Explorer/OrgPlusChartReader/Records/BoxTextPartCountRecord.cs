using System.IO;

namespace OrgPlusChartReader;

public class BoxTextPartCountRecord : OrganizationChartRecord
{
    public ushort Count { get; set; }

    public BoxTextPartCountRecord(OrganizationChartRecordId id, BinaryReader reader) : base(id, reader)
    {
        Count = ReadUInt16(0);
    }
}
