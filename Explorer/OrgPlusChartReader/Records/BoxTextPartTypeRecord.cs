using System.IO;

namespace OrgPlusChartReader;

public class BoxTextPartTypeRecord : OrganizationChartRecord
{
    public BoxTextPartType Type { get; set; }
    
    public BoxTextPartTypeRecord(OrganizationChartRecordId id, BinaryReader reader) : base(id, reader)
    {
        Type = (BoxTextPartType)ReadUInt16(0);
    }

    public enum BoxTextPartType : ushort
    {
        TopLevel = 0x01,
        Comment = 0x02,
    }
}
