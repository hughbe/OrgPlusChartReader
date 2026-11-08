namespace OrgPlusChartReader;

/// <summary>
/// A section: a start marker (ID &gt;= 0x4000), the records and sections it contains,
/// and an end marker (start ID + 0x2000).
/// </summary>
public class OrganizationChartSubstream : OrganizationChartRecord
{
    private readonly List<OrganizationChartRecord> _records = new();

    internal OrganizationChartSubstream(OrganizationChartRecordId id) : base(id, Array.Empty<byte>())
    {
    }

    public IReadOnlyList<OrganizationChartRecord> Records => _records;

    internal void Add(OrganizationChartRecord record) => _records.Add(record);
}
