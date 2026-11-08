using System.ComponentModel;

namespace OrgPlusChartReader;

public class CopyrightRecord : OrganizationChartRecord
{
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown { get; set; }

    public string Value { get; set; }

    internal CopyrightRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        int offset = 0;

        // Unknown (2 bytes).
        Unknown = ReadUInt16(offset);
        offset += 2;

        // Value (variable): null-terminated ASCII encoded copyright text.
        Value = ReadNullTerminatedString(offset, out int bytesRead);
        offset += bytesRead;
    }
}