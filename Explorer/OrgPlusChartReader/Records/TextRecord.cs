namespace OrgPlusChartReader;

public class TextRecord : OrganizationChartRecord
{
    public string Value{ get; set; }

    internal TextRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        Value = ReadString(0, RecordSize);
    }
}
