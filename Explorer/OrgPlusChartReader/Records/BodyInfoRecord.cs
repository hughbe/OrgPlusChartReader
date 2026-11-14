using System.ComponentModel;
using System.Drawing;

namespace OrgPlusChartReader;

public class BodyInfoRecord : OrganizationChartRecord
{
    public ushort Size { get; set; }

    /// <summary>
    /// The number of boxes in the chart. The program ignores it when reading.
    /// </summary>
    public ushort BoxCount { get; set; }

    /// <summary>
    /// The default currency symbol: up to 4 bytes, NUL-terminated. The program writes "$" and leaves the
    /// bytes after the NUL unset. Offsets 4 to 13 are the chart's default number format, laid out like part
    /// of <see cref="NumberFormatRecord"/>; the program ignores them when reading.
    /// </summary>
    public string CurrencySymbol { get; set; }

    /// <summary>The default decimal separator ('.').</summary>
    public char DecimalSeparator { get; set; }

    /// <summary>The default thousands separator (',').</summary>
    public char ThousandsSeparator { get; set; }

    /// <summary>Whether the currency symbol goes before the amount by default (always true).</summary>
    public bool CurrencySymbolFirst { get; set; }

    /// <summary>The default number of decimal digits (always 2).</summary>
    public ushort DecimalDigits { get; set; }

    /// <summary>
    /// The character set of the file's text. Other values make the program refuse the file.
    /// </summary>
    public TextCharacterSet CharacterSet { get; set; }

    public Color BackgroundColor { get; set; }

    /// <summary>
    /// A chart-wide spacing measurement in view units (22 in the samples): the chart's copy of one of the
    /// group spacing values. See <see cref="BoxLayoutRecord"/>.
    /// </summary>
    public uint Spacing { get; set; }

    /// <summary>
    /// The position of a draggable guide, in view units measured in from the page margins. Guide lines are drawn at this inset.
    /// </summary>
    public uint GuideInset1 { get; set; }

    /// <summary>The position of a second draggable guide. See <see cref="GuideInset1"/>.</summary>
    public uint GuideInset2 { get; set; }

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
        CurrencySymbol = ReadNullTerminatedString(offset, out _);
        if (CurrencySymbol.Length > 4)
        {
            CurrencySymbol = CurrencySymbol.Substring(0, 4);
        }

        offset += 4;
        DecimalSeparator = (char)ReadByte(offset);
        ThousandsSeparator = (char)ReadByte(offset + 1);
        offset += 2;
        CurrencySymbolFirst = ReadUInt16(offset) != 0;
        offset += 2;
        DecimalDigits = ReadUInt16(offset);
        offset += 2;
        CharacterSet = (TextCharacterSet)ReadUInt16(offset);
        offset += 2;
        BackgroundColor = ReadColor(offset);

        Spacing = ReadUInt32(20);
        GuideInset1 = ReadUInt32(24);
        GuideInset2 = ReadUInt32(28);
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
