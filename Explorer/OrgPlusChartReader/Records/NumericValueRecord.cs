namespace OrgPlusChartReader;

/// <summary>
/// The numeric value of a field (0x0070), written only when the field's text parses as a number.
/// </summary>
public class NumericValueRecord : OrganizationChartRecord
{
    public double Value { get; set; }

    internal NumericValueRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        ReadUInt32(4); // Ensures all 8 bytes are present.
        Value = BitConverter.ToDouble(Bytes, 0);
    }
}
