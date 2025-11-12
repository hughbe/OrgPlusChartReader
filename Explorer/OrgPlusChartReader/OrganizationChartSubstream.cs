namespace OrgPlusChartReader;

public class OrganizationChartSubstream : OrganizationChartRecord
{
    public OrganizationChartSubstream(OrganizationChartRecordId id, BinaryReader reader) : base(id, reader)
    {
    }

    public List<OrganizationChartRecord> Records { get; } = [];
}