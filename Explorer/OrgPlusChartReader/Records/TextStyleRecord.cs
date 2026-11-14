using System.ComponentModel;
using System.Drawing;

namespace OrgPlusChartReader;

/// <summary>
/// One property of a text style (0x0061). Text is rich: per-run properties (font, size, style, colour
/// and the text shadow) apply from <see cref="CharacterPosition"/> onwards. A text style section lists only
/// the properties that differ from the defaults in the canvas section's text style section.
/// </summary>
public class TextStyleRecord : OrganizationChartRecord
{
    public TextStyleProperty Property { get; set; }

    /// <summary>
    /// The value. Its meaning depends on <see cref="Property"/>.
    /// </summary>
    [TypeConverter(typeof(HexTypeConverter))]
    public uint Value { get; set; }

    /// <summary>
    /// For per-run properties, the zero-based character position where the run starts. 0 for paragraph properties.
    /// </summary>
    public ushort CharacterPosition { get; set; }

    /// <summary>
    /// The colour, for <see cref="TextStyleProperty.TextColor"/>, <see cref="TextStyleProperty.TextShadowColor"/>
    /// and <see cref="TextStyleProperty.TextShadowSecondaryColor"/>. Unlike shape colours, text colours are
    /// stored in the order red, green, blue.
    /// </summary>
    public Color? Color { get; set; }

    internal TextStyleRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        Property = (TextStyleProperty)ReadUInt16(0);
        Value = ReadUInt32(2);
        CharacterPosition = ReadUInt16(6);
        if (Property is TextStyleProperty.TextColor or TextStyleProperty.TextShadowColor or TextStyleProperty.TextShadowSecondaryColor)
        {
            Color = ReadRgbColor(2);
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
        /// <summary>Per run: bold and italic flags. See <see cref="FontStyleFlags"/>.</summary>
        FontStyle = 0x01,

        /// <summary>Per run: the index of the font in the font table.</summary>
        FontIndex = 0x02,

        /// <summary>Per run: the text colour.</summary>
        TextColor = 0x03,

        /// <summary>
        /// A date code. When the value's low byte is '%', the program appends today's date to the text when
        /// it reads the file; the next byte ('1' to '5') picks the format. Microsoft Organization Chart only
        /// writes the value 0x29, which it ignores.
        /// </summary>
        DateCode = 0x04,

        /// <summary>Paragraph: whether the field's label is shown with its text (text flag 0x1000).</summary>
        ShowFieldLabel = 0x05,

        /// <summary>Paragraph: whether the text wraps (text flag 0x02).</summary>
        WrapText = 0x06,

        /// <summary>Paragraph: the alignment. See <see cref="TextAlignment"/>.</summary>
        Alignment = 0x07,

        /// <summary>Paragraph: the name format (1 to 28) of a Name field; 0 for none.</summary>
        NameFormat = 0x08,

        /// <summary>Per run: the font size in points.</summary>
        FontSize = 0x09,

        /// <summary>Per run: the colour of the text shadow.</summary>
        TextShadowColor = 0x0A,

        /// <summary>Per run: the second colour of the text shadow.</summary>
        TextShadowSecondaryColor = 0x0B,

        /// <summary>Per run: the text shadow's vertical offset (signed, view units).</summary>
        TextShadowOffsetY = 0x0C,

        /// <summary>Per run: the text shadow's horizontal offset (signed, view units).</summary>
        TextShadowOffsetX = 0x0D,

        /// <summary>Per run: the type of text shadow, in line shadow codes (0, 1, 2, 7, 8).</summary>
        TextShadowType = 0x0E,
    }

    [Flags]
    public enum FontStyleFlags : uint
    {
        Normal = 0x00,
        Bold = 0x01,
        Italic = 0x02,

        /// <summary>A style Microsoft Organization Chart lacks (probably underline); it clears it in charts from other editions.</summary>
        Unsupported0x04 = 0x04,
    }

    /// <summary>
    /// The alignment. The canvas default stores the raw Windows text-alignment flags instead,
    /// where 2 means right and 6 means centre.
    /// </summary>
    public enum TextAlignment : uint
    {
        Left = 0x00,
        Center = 0x01,
        Right = 0x02,
    }
}
