using System.ComponentModel;

namespace OrgPlusChartReader;

public class SelectedRecord : OrganizationChartRecord
{
    /// <summary>
    /// In the drawing shapes and connectors sections, the zero-based index of the selected item.
    /// In a boxes section, 0x24 in every sample.
    /// </summary>
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Value { get; set; }

    internal SelectedRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        Value = ReadUInt16(0);
    }
}
