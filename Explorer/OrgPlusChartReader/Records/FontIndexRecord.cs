using System.ComponentModel;

namespace OrgPlusChartReader;

public class FontIndexRecord : OrganizationChartRecord
{
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Index { get; set; }

    /// <summary>
    /// 0 for most fonts; 0xA1 for Century Gothic. Possibly the character set or pitch and family.
    /// </summary>
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown { get; set; }

    internal FontIndexRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        Index = ReadUInt16(0);
        Unknown = ReadUInt16(2);
    }
}
