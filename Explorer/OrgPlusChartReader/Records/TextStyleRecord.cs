using System.ComponentModel;
using System.Drawing;

namespace OrgPlusChartReader;

/// <summary>
/// One property of a text style (0x0061). A text style section lists the properties that differ
/// from the defaults in the canvas section's text style section.
/// </summary>
public class TextStyleRecord : OrganizationChartRecord
{
    public TextStyleProperty Property { get; set; }

    /// <summary>
    /// The value as a 16-bit integer. Its meaning depends on <see cref="Property"/>.
    /// </summary>
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Value { get; set; }

    /// <summary>
    /// The text colour, for <see cref="TextStyleProperty.TextColor"/>. Unlike other colours in the file,
    /// it is stored in the order red, green, blue.
    /// </summary>
    public Color? TextColor { get; set; }

    internal TextStyleRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        Property = (TextStyleProperty)ReadUInt16(0);
        Value = ReadUInt16(2);
        if (Property == TextStyleProperty.TextColor)
        {
            TextColor = ReadRgbColor(2);
        }
    }

    /// <summary>
    /// For <see cref="TextStyleProperty.FontStyle"/>, the value as font style flags.
    /// </summary>
    public FontStyleFlags FontStyle => (FontStyleFlags)Value;

    /// <summary>
    /// For <see cref="TextStyleProperty.Alignment"/>, the value as an alignment.
    /// </summary>
    public TextAlignment Alignment => (TextAlignment)Value;

    public enum TextStyleProperty : ushort
    {
        /// <summary>Bold and italic flags. See <see cref="FontStyleFlags"/>.</summary>
        FontStyle = 0x01,

        /// <summary>The index of the font in the font table.</summary>
        FontIndex = 0x02,

        /// <summary>The text colour.</summary>
        TextColor = 0x03,

        Unknown0x05 = 0x05,
        Unknown0x06 = 0x06,

        /// <summary>The alignment. See <see cref="TextAlignment"/>.</summary>
        Alignment = 0x07,

        Unknown0x08 = 0x08,

        /// <summary>The font size in points.</summary>
        FontSize = 0x09,

        /// <summary>Probably the text shadow colour.</summary>
        Unknown0x0A = 0x0A,

        /// <summary>Probably the text background colour.</summary>
        Unknown0x0B = 0x0B,

        Unknown0x0C = 0x0C,
        Unknown0x0D = 0x0D,
        Unknown0x0E = 0x0E,
    }

    [Flags]
    public enum FontStyleFlags : ushort
    {
        Normal = 0x00,
        Bold = 0x01,
        Italic = 0x02,
    }

    public enum TextAlignment : ushort
    {
        Left = 0x00,
        Center = 0x01,
        Right = 0x02,
    }
}
