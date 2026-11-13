namespace OrgPlusChartReader;

/// <summary>
/// The id of the root group (0x0024): the <see cref="GroupRecord.GroupId"/> of the group the groups
/// section starts from.
/// </summary>
public class RootGroupRecord : OrganizationChartRecord
{
    public short GroupId { get; set; }

    internal RootGroupRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        GroupId = ReadInt16(0);
    }
}
