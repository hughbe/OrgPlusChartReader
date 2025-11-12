namespace OrgPlusChartReader;

public class TextRecord : OrganizationChartRecord
{
    public string Value{ get; set; }

    public TextRecord(OrganizationChartRecordId id, BinaryReader reader) : base(id, reader)
    {
        Value = ReadASCIIString(0, RecordSize);
    }
}
