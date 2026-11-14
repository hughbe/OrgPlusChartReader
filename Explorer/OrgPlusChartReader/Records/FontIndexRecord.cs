using System.ComponentModel;

namespace OrgPlusChartReader;

public class FontIndexRecord : OrganizationChartRecord
{
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Index { get; set; }

    /// <summary>
    /// The font's Windows character set (<c>LOGFONT.lfCharSet</c>), for example 0 for ANSI.
    /// The program only writes this record on Windows 95 and later.
    /// </summary>
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort CharacterSet { get; set; }

    internal FontIndexRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        Index = ReadUInt16(0);
        CharacterSet = ReadUInt16(2);
    }
}
