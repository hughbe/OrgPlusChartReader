using System.ComponentModel;

namespace OrgPlusChartReader;

public class SelectedRecord : OrganizationChartRecord
{
    /// <summary>
    /// In the drawing shapes and connectors sections, the zero-based index of the selected item.
    /// In a boxes section the value is leftover data (0x24, the size field of the box record written before it).
    /// </summary>
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Value { get; set; }

    internal SelectedRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        Value = ReadUInt16(0);
    }
}
