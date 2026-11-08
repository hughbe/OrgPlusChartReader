namespace OrgPlusChartReader;

public class TitlePartIndexRecord : OrganizationChartRecord
{
    public ushort Index { get; set; }

    internal TitlePartIndexRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        Index = ReadUInt16(0);
    }
}
