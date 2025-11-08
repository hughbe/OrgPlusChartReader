using System.IO;

namespace OrgPlusChartReader;

public class BoxTextPartIndexRecord : OrganizationChartRecord
{
    public BoxTextPartIndex Index { get; set; }

    public BoxTextPartIndexRecord(OrganizationChartRecordId id, BinaryReader reader) : base(id, reader)
    {
        Index = (BoxTextPartIndex)ReadUInt16(0);
    }

    public enum BoxTextPartIndex : ushort
    {
        Name = 0x00,
        Title = 0x01,
        Comment1 = 0x02,
        Comment2 = 0x03
    }
}
