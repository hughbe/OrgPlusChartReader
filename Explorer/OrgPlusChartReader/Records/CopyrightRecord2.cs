using System.ComponentModel;

namespace OrgPlusChartReader;

public class CopyrightRecord2 : OrganizationChartRecord
{
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Size { get; set; }

    public string Value { get; set; }

    internal CopyrightRecord2(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        int offset = 0;

        // Size (2 bytes): total size of the record.
        Size = ReadUInt16(offset);
        offset += 2;

        // Value (variable): ASCII encoded copyright text.
        Value = ReadString(offset, Size - 2);
        offset += Size - 2;
    }
}