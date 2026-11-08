using System.ComponentModel;
using System.Drawing;

namespace OrgPlusChartReader;

public class BodyInfoRecord : OrganizationChartRecord
{
    public ushort Size { get; set; }

    /// <summary>
    /// The number of boxes in the chart.
    /// </summary>
    public ushort BoxCount { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown2 { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown3 { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown4 { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown5 { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown6 { get; set; }

    /// <summary>
    /// The character set of the file's text. Other values make the program refuse the file.
    /// </summary>
    public TextCharacterSet CharacterSet { get; set; }

    public Color BackgroundColor { get; set; }

    /// <summary>
    /// The high byte of the program's internal colour value. 0xFF means the default colour,
    /// in which case the colour's fourth byte is also 1.
    /// </summary>
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort ColorHighByte { get; set; }

    internal BodyInfoRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        int offset = 0;
        Size = ReadUInt16(offset);
        offset += 2;
        BoxCount = ReadUInt16(offset);
        offset += 2;
        Unknown2 = ReadUInt16(offset);
        offset += 2;
        Unknown3 = ReadUInt16(offset);
        offset += 2;
        Unknown4 = ReadUInt16(offset);
        offset += 2;
        Unknown5 = ReadUInt16(offset);
        offset += 2;
        Unknown6 = ReadUInt16(offset);
        offset += 2;
        CharacterSet = (TextCharacterSet)ReadUInt16(offset);
        offset += 2;
        BackgroundColor = ReadColor(offset);

        ColorHighByte = ReadUInt16(32);
    }

    public enum TextCharacterSet : ushort
    {
        /// <summary>Text is in the OEM (DOS) code page.</summary>
        Oem = 0,

        /// <summary>Text is in the ANSI (Windows-1252) code page.</summary>
        Ansi = 1,
    }
}
