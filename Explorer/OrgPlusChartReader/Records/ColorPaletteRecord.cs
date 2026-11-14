using System.Drawing;

namespace OrgPlusChartReader;

/// <summary>
/// The chart's colour palette (0x0090): the colours offered by the Color dialog.
/// </summary>
public class ColorPaletteRecord : OrganizationChartRecord
{
    public ushort Size { get; set; }

    public IReadOnlyList<Color> Colors { get; set; }

    /// <summary>
    /// For each colour, whether it is still one of the program's 16 standard colours.
    /// </summary>
    public IReadOnlyList<bool> IsStandard { get; set; }

    internal ColorPaletteRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        Size = ReadUInt16(0);
        int count = ReadUInt16(2);
        var colors = new List<Color>();
        var standard = new List<bool>();
        for (int i = 0; i < count; i++)
        {
            colors.Add(ReadColor(4 + i * 4));
            standard.Add(ReadByte(7 + i * 4) != 0);
        }

        Colors = colors;
        IsStandard = standard;
    }
}
