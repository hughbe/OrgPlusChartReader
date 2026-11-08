using System.ComponentModel;

namespace OrgPlusChartReader;

/// <summary>
/// The password record (0x0001), 144 bytes. A non-zero <see cref="PasswordFlag"/> means the chart is
/// password-protected; Microsoft Organization Chart refuses to open such files. The rest of the record is
/// not written by the program and holds leftover data.
/// </summary>
public class PasswordRecord : OrganizationChartRecord
{
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Size { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort PasswordFlag { get; set; }

    public bool IsPasswordProtected => PasswordFlag != 0;

    internal PasswordRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        Size = ReadUInt16(0);
        PasswordFlag = ReadUInt16(2);
    }
}
