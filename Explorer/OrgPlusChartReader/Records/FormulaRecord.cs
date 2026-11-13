using System.ComponentModel;

namespace OrgPlusChartReader;

/// <summary>
/// The formula of a calculated field (0x0013), such as "@sum(sub,field 2)": a function applied to a field
/// over a set of boxes.
/// </summary>
public class FormulaRecord : OrganizationChartRecord
{
    /// <summary>The length of the fixed part of the record (5), which is the offset of the formula text.</summary>
    public ushort HeaderSize { get; set; }

    /// <summary>0 when <see cref="Text"/> holds the formula; 2, 3 or 4 for three predefined formulas.</summary>
    public ushort PredefinedCode { get; set; }

    /// <summary>For a predefined formula, a field number; otherwise 0xFF.</summary>
    public byte PredefinedField { get; set; }

    /// <summary>The raw formula text, or null for a predefined formula.</summary>
    public string? Text { get; set; }

    /// <summary>The function name, such as "sum", when the formula text could be parsed.</summary>
    public string? Function { get; set; }

    /// <summary>The scope, such as "sub" for subordinates, when the formula text could be parsed.</summary>
    public string? Scope { get; set; }

    /// <summary>The zero-based number of the field the formula is applied to, when the formula text could be parsed.</summary>
    public byte? FieldIndex { get; set; }

    internal FormulaRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        HeaderSize = ReadUInt16(0);
        PredefinedCode = ReadUInt16(2);
        PredefinedField = ReadByte(4);
        if (PredefinedCode != 0)
        {
            return;
        }

        // "@" function "(" scope "," 0x01 field ")"
        int length = 0;
        while (5 + length < Bytes.Length && Bytes[5 + length] != 0)
        {
            length++;
        }

        byte[] raw = Bytes.Skip(5).Take(length).ToArray();
        Text = new string(raw.Select(b => (char)b).ToArray());
        int open = Array.IndexOf(raw, (byte)'(');
        int comma = Array.IndexOf(raw, (byte)',');
        if (raw.Length >= 8 && raw[0] == '@' && open > 1 && comma > open && comma + 3 < raw.Length
            && raw[comma + 1] == 0x01 && raw[comma + 3] == ')')
        {
            Function = Text.Substring(1, open - 1);
            Scope = Text.Substring(open + 1, comma - open - 1);
            FieldIndex = raw[comma + 2];
        }
    }
}
