using System.ComponentModel;

namespace OrgPlusChartReader;

public class SelectedRecord : OrganizationChartRecord
{
    /// <summary>
    /// In a lines section, the zero-based number of the selected line segment. In the drawing shapes and
    /// connectors sections, the item's zero-based position in the program's drawing list, which the program
    /// ignores when reading: it selects the item most recently read.
    /// In a boxes section the value is leftover data (0x24, the size field of the box record written before it).
    /// </summary>
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Value { get; set; }

    internal SelectedRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        Value = ReadUInt16(0);
    }
}
