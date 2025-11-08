using System.ComponentModel;
using System.IO;

namespace OrgPlusChartReader;

public class CopyrightRecord2 : OrganizationChartRecord
{
    [TypeConverter(typeof(HexTypeConverter))]
    public int Size { get; set; }

    public string Value { get; set; }

    public CopyrightRecord2(OrganizationChartRecordId id, BinaryReader reader) : base(id, reader)
    {
        int offset = 0;

        // Size (2 bytes): total size of the record.
        Size = ReadUInt16(offset);
        offset += 2;

        // Value (variable): ASCII encoded copyright text.
        Value = ReadASCIIString(offset, Size - 2);
        offset += Size - 2;
    }
}