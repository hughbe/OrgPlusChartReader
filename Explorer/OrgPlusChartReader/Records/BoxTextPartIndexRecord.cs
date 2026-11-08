namespace OrgPlusChartReader;

public class BoxTextPartIndexRecord : OrganizationChartRecord
{
    public BoxTextPartIndex Index { get; set; }

    internal BoxTextPartIndexRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
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
