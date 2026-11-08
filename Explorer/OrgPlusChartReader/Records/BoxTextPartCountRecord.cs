namespace OrgPlusChartReader;

public class BoxTextPartCountRecord : OrganizationChartRecord
{
    public ushort Count { get; set; }

    internal BoxTextPartCountRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        Count = ReadUInt16(0);
    }
}
