using System.IO;

namespace OrgPlusChartReader;

public class TitlePartIndexRecord : OrganizationChartRecord
{
    public ushort Index { get; set; }

    public TitlePartIndexRecord(OrganizationChartRecordId id, BinaryReader reader) : base(id, reader)
    {
        Index = ReadUInt16(0);
    }
}
