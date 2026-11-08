using System.ComponentModel;

namespace OrgPlusChartReader;

/// <summary>
/// A box template (0x0023): the text fields a box shows, in order. Boxes refer to a template by
/// <see cref="TemplateId"/>.
/// </summary>
public class BoxTemplateRecord : OrganizationChartRecord
{
    /// <summary>
    /// The template id, which is a bitmask of the fields shown (bit n is field n). Template 0 lists every field.
    /// </summary>
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort TemplateId { get; set; }

    /// <summary>
    /// Four values that look like inner margins (8 in charts, 4 in style files).
    /// </summary>
    public IReadOnlyList<ushort> Margins { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public byte Unknown { get; set; }

    public IReadOnlyList<Entry> Fields { get; set; }

    internal BoxTemplateRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        int offset = 0;

        TemplateId = ReadUInt16(offset);
        offset += 2;

        var margins = new ushort[4];
        for (int i = 0; i < margins.Length; i++)
        {
            margins[i] = ReadUInt16(offset);
            offset += 2;
        }

        Margins = margins;

        Unknown = ReadByte(offset);
        offset += 1;

        // A list of 3-byte entries, terminated by a zero byte.
        var fields = new List<Entry>();
        while (ReadByte(offset) != 0)
        {
            fields.Add(new Entry(ReadByte(offset), ReadByte(offset + 1), ReadByte(offset + 2)));
            offset += 3;
        }

        Fields = fields;
    }

    public readonly struct Entry
    {
        /// <summary>
        /// The kind of entry. 1 (a text field) in every sample.
        /// </summary>
        public byte Kind { get; }

        /// <summary>
        /// The zero-based index of the text field (see <see cref="BoxTextPartIndexRecord"/>).
        /// </summary>
        public byte FieldIndex { get; }

        /// <summary>
        /// Unknown. 6 in every sample.
        /// </summary>
        public byte Unknown { get; }

        public Entry(byte kind, byte fieldIndex, byte unknown)
        {
            Kind = kind;
            FieldIndex = fieldIndex;
            Unknown = unknown;
        }

        public override string ToString() => $"({Kind}, {FieldIndex}, {Unknown})";
    }
}
