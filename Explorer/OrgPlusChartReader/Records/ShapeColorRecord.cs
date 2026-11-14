using System.ComponentModel;
using System.Drawing;

namespace OrgPlusChartReader;

public class ShapeColorRecord : OrganizationChartRecord
{
    public ushort Size { get; set; }

    /// <summary>Always 0. The program ignores it when reading.</summary>
    public ushort Reserved { get; set; }

    public Color Color { get; set; }

    /// <summary>
    /// The high byte of the program's internal colour value. 0xFF means the default colour,
    /// in which case the colour's fourth byte is also 1.
    /// </summary>
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort ColorHighByte { get; set; }

    internal ShapeColorRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        int offset = 0;
        Size = ReadUInt16(offset);
        offset += 2;
        Reserved = ReadUInt16(offset);
        offset += 2;
        Color = ReadColor(offset);
        offset += 4;
        ColorHighByte = ReadUInt16(offset);
    }
}
