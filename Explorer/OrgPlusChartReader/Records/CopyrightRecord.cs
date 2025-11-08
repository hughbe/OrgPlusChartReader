using System.ComponentModel;
using System.IO;

namespace OrgPlusChartReader;

public class CopyrightRecord : OrganizationChartRecord
{
    [TypeConverter(typeof(HexTypeConverter))]
    public int Unknown { get; set; }

    public string Value { get; set; }

    public CopyrightRecord(OrganizationChartRecordId id, BinaryReader reader) : base(id, reader)
    {
        int offset = 0;

        // Unknown (2 bytes).
        Unknown = ReadUInt16(offset);
        offset += 2;

        // Value (variable): null-terminated ASCII encoded copyright text.
        Value = ReadNullTerminatedASCIIString(offset, out int bytesRead);
        offset += bytesRead;
    }
}