using System.ComponentModel;

namespace OrgPlusChartReader;

/// <summary>
/// The number format of a numeric or currency field (0x0071). The separators and symbol are copied from
/// Windows' [Intl] settings when the chart is saved.
/// </summary>
public class NumberFormatRecord : OrganizationChartRecord
{
    public ushort Size { get; set; }

    /// <summary>The text's flag 0x1000, the same flag as text-style property 5.</summary>
    public ushort TextFlag { get; set; }

    /// <summary>The currency symbol, for currency formats; otherwise empty.</summary>
    public string CurrencySymbol { get; set; }

    public char DecimalSeparator { get; set; }

    public char ThousandsSeparator { get; set; }

    /// <summary>Whether the currency symbol goes before the amount.</summary>
    public bool CurrencySymbolFirst { get; set; }

    /// <summary>The number of decimal digits.</summary>
    public ushort Digits { get; set; }

    /// <summary>A value derived from the text's alignment.</summary>
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Alignment { get; set; }

    /// <summary>The format's flags: bit 0 = currency, bit 4 = explicit digit count.</summary>
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Flags { get; set; }

    public bool IsCurrency => (Flags & 0x01) != 0;

    internal NumberFormatRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        Size = ReadUInt16(0);
        TextFlag = ReadUInt16(2);
        CurrencySymbol = ReadString(4, 4);
        DecimalSeparator = (char)ReadByte(8);
        ThousandsSeparator = (char)ReadByte(9);
        CurrencySymbolFirst = ReadUInt16(10) != 0;
        Digits = ReadUInt16(12);
        Alignment = ReadUInt16(14);
        Flags = ReadUInt16(16);
    }
}
