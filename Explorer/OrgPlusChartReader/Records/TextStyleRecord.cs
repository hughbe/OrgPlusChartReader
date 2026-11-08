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

        /// <summary>Written with the value 0x29 when the caller asks for it.</summary>
        Unknown0x04 = 0x04,

        /// <summary>Paragraph: text flag 0x1000.</summary>
        Unknown0x05 = 0x05,

        /// <summary>Paragraph: text flag 0x02.</summary>
        Unknown0x06 = 0x06,

        /// <summary>Paragraph: the alignment. See <see cref="TextAlignment"/>.</summary>
        Alignment = 0x07,

        /// <summary>Paragraph: a value that is only written when text flag 0x4000 is set.</summary>
        Unknown0x08 = 0x08,

        /// <summary>Per run: the font size in points.</summary>
        FontSize = 0x09,

        /// <summary>Per run: the colour of the text shadow.</summary>
        TextShadowColor = 0x0A,

        /// <summary>Per run: the second colour of the text shadow.</summary>
        TextShadowSecondaryColor = 0x0B,

        /// <summary>Per run: one of the text shadow's two offsets.</summary>
        TextShadowOffset1 = 0x0C,

        /// <summary>Per run: one of the text shadow's two offsets.</summary>
        TextShadowOffset2 = 0x0D,

        /// <summary>Per run: the type of text shadow.</summary>
        TextShadowType = 0x0E,
    }

    [Flags]
    public enum FontStyleFlags : uint
    {
        Normal = 0x00,
        Bold = 0x01,
        Italic = 0x02,
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
