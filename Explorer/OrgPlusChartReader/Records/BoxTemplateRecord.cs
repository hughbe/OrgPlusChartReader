using System.ComponentModel;

namespace OrgPlusChartReader;

/// <summary>
/// A box template (0x0023), called a "label setup" by the program: a small byte-code program that lays
/// out the text fields in a box, line by line. Boxes refer to a template by <see cref="TemplateId"/>.
/// </summary>
public class BoxTemplateRecord : OrganizationChartRecord
{
    /// <summary>
    /// The template id, which is a bitmask of the fields shown (bit n is field n). Template 0 lists every field.
    /// </summary>
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort TemplateId { get; set; }

    /// <summary>
    /// Four inner margins of the label setup (8 in charts, 4 in style files).
    /// </summary>
    public IReadOnlyList<ushort> Margins { get; set; }

    /// <summary>
    /// The layout program, in order, ending before the <see cref="Opcode.End"/> instruction.
    /// </summary>
    public IReadOnlyList<Instruction> Instructions { get; set; }

    /// <summary>
    /// Bytes after an opcode this reader does not know, if any. Parsing stops at an unknown opcode.
    /// </summary>
    public byte[] UnparsedBytes { get; set; }

    /// <summary>
    /// The field indexes placed by the program, in order.
    /// </summary>
    public IEnumerable<byte> FieldIndices =>
        Instructions.Where(x => x.Opcode == Opcode.Field).Select(x => x.Operand);

    internal BoxTemplateRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        TemplateId = ReadUInt16(0);

        var margins = new ushort[4];
        for (int i = 0; i < margins.Length; i++)
        {
            margins[i] = ReadUInt16(2 + i * 2);
        }

        Margins = margins;

        var instructions = new List<Instruction>();
        int offset = 10;
        UnparsedBytes = Array.Empty<byte>();
        while (true)
        {
            var opcode = (Opcode)ReadByte(offset);
            offset++;
            if (opcode == Opcode.End)
            {
                break;
            }

            switch (opcode)
            {
                case Opcode.Field:
                case Opcode.VerticalLine:
                case Opcode.HorizontalLine:
                    instructions.Add(new Instruction(opcode, ReadByte(offset)));
                    offset++;
                    break;
                case Opcode.MarkerA:
                case Opcode.MarkerB:
                case Opcode.EndOfLine:
                case Opcode.Ignored7:
                case Opcode.Ignored8:
                case Opcode.HideField:
                    instructions.Add(new Instruction(opcode, 0));
                    break;
                default:
                    UnparsedBytes = Bytes.Skip(offset - 1).ToArray();
                    Instructions = instructions;
                    return;
            }
        }

        Instructions = instructions;
    }

    public enum Opcode : byte
    {
        /// <summary>The end of the program.</summary>
        End = 0x00,

        /// <summary>Place a field. The operand is the field index, or 0xFF for none.</summary>
        Field = 0x01,

        /// <summary>
        /// A vertical line inside the box, to the right of the field. Operand 1 keeps it inside the box's
        /// inner margins; operand 0 runs it the full height to the border.
        /// </summary>
        VerticalLine = 0x02,

        /// <summary>
        /// A horizontal line inside the box, under the field's row. Operand 1 keeps it inside the inner
        /// margins; operand 0 runs it the full width to the border.
        /// </summary>
        HorizontalLine = 0x03,

        /// <summary>Marks the line selected by one of the label setup's line indexes.</summary>
        MarkerA = 0x04,

        /// <summary>Marks the line selected by the label setup's other line index.</summary>
        MarkerB = 0x05,

        /// <summary>The end of a line.</summary>
        EndOfLine = 0x06,

        /// <summary>Accepted and ignored by the program.</summary>
        Ignored7 = 0x07,

        /// <summary>Accepted and ignored by the program.</summary>
        Ignored8 = 0x08,

        /// <summary>The field just placed is hidden: it stays in the template but is not displayed.</summary>
        HideField = 0x09,
    }

    public readonly struct Instruction
    {
        public Opcode Opcode { get; }

        public byte Operand { get; }

        public Instruction(Opcode opcode, byte operand)
        {
            Opcode = opcode;
            Operand = operand;
        }

        public override string ToString() => Opcode switch
        {
            Opcode.Field or Opcode.VerticalLine or Opcode.HorizontalLine => $"{Opcode}({Operand})",
            _ => Opcode.ToString(),
        };
    }
}
