using System.Text;

namespace OrgPlusChartReader;

/// <summary>
/// A parsed Microsoft Organization Chart (.opx / .ops) file.
/// </summary>
public class OrganizationChart
{
    private const int FooterLength = 2 + 2 + 10; // ID + length + payload.

    /// <summary>
    /// The top-level records and sections of the file, in order. The last item is the footer.
    /// </summary>
    public IReadOnlyList<OrganizationChartRecord> Records { get; }

    /// <summary>
    /// The footer record at the end of the file.
    /// </summary>
    public FooterRecord Footer { get; }

    /// <summary>
    /// Bytes between the end of the root section and the footer that are not part of the chart.
    /// Charts that have been saved over a longer earlier version can contain stale data here.
    /// This is empty for most files.
    /// </summary>
    public byte[] SlackBytes { get; }

    /// <summary>
    /// Whether both checksums in the footer match the contents of the file.
    /// A mismatch does not stop the file being read; it usually means the file was modified after it was saved.
    /// </summary>
    public bool IsChecksumValid { get; }

    /// <summary>
    /// Reads a chart from a stream, from its current position to its end.
    /// The stream is not closed and does not need to be seekable.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null.</exception>
    /// <exception cref="InvalidDataException">The data is not a well-formed organization chart.</exception>
    public OrganizationChart(Stream stream) : this(ReadAll(stream))
    {
    }

    /// <summary>
    /// Reads a chart from a byte array.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="data"/> is null.</exception>
    /// <exception cref="InvalidDataException">The data is not a well-formed organization chart.</exception>
    public OrganizationChart(byte[] data)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        // The header is 4 bytes: "UOCF"
        if (data.Length < 4 || Encoding.ASCII.GetString(data, 0, 4) != "UOCF")
        {
            throw new InvalidDataException("Invalid file signature");
        }

        // The rest of the file consists of records.
        // Each record starts with a 2-byte ID and a 2-byte length, followed by the data.
        // Sections are different: they have an ID >= 0x4000 and neither length nor data,
        // and are closed by a marker whose ID is the start ID + 0x2000.
        var records = new List<OrganizationChartRecord>();
        var substreamStack = new Stack<OrganizationChartSubstream>();
        bool rootClosed = false;
        int position = 4;
        while (position < data.Length && !rootClosed)
        {
            ushort rawId = ReadUInt16(data, ref position);
            var id = (OrganizationChartRecordId)rawId;
            if (rawId >= 0x6000)
            {
                // This is the end of the current section, which must be the innermost open one.
                if (substreamStack.Count == 0 || rawId != (ushort)substreamStack.Peek().Id + 0x2000)
                {
                    throw new InvalidDataException(
                        $"Unexpected section end marker 0x{rawId:X4} at offset {position - 2}.");
                }

                substreamStack.Pop();
                rootClosed = substreamStack.Count == 0;
            }
            else if (rawId >= 0x4000)
            {
                // This is the start of a new section.
                var substream = new OrganizationChartSubstream(id);
                AddRecord(records, substreamStack, substream);
                substreamStack.Push(substream);
            }
            else
            {
                int recordOffset = position - 2;
                long length = ReadUInt16(data, ref position);
                if (length == 0xFFFF)
                {
                    // A length of 0xFFFF is an escape: the real length follows as a 32-bit value.
                    length = ReadUInt16(data, ref position) | ((long)ReadUInt16(data, ref position) << 16);
                }

                if (length > data.Length - position)
                {
                    throw new InvalidDataException(
                        $"Record 0x{rawId:X4} at offset {recordOffset} declares {length} byte(s) but only {data.Length - position} remain.");
                }

                var recordData = new byte[(int)length];
                Buffer.BlockCopy(data, position, recordData, 0, recordData.Length);
                position += recordData.Length;

                AddRecord(records, substreamStack, OrganizationChartRecord.Create(id, recordData));
            }
        }

        if (substreamStack.Count != 0)
        {
            throw new InvalidDataException(
                $"The file ended inside section 0x{(ushort)substreamStack.Peek().Id:X4}: the file is truncated.");
        }

        if (!rootClosed)
        {
            throw new InvalidDataException("The file does not contain a root section.");
        }

        // The footer is the final record of the file. It is usually immediately after the root section,
        // but stale bytes from an earlier save can sit in between.
        int footerOffset = data.Length - FooterLength;
        if (footerOffset < position
            || BitConverter.ToUInt16(data, footerOffset) != (ushort)OrganizationChartRecordId.Footer
            || BitConverter.ToUInt16(data, footerOffset + 2) != FooterLength - 4)
        {
            throw new InvalidDataException("The file does not end with a footer record: the file is truncated.");
        }

        var footerData = new byte[FooterLength - 4];
        Buffer.BlockCopy(data, footerOffset + 4, footerData, 0, footerData.Length);
        Footer = new FooterRecord(OrganizationChartRecordId.Footer, footerData);

        // The footer records the length of the file up to (not including) the footer.
        if (Footer.FileSize != footerOffset)
        {
            throw new InvalidDataException(
                $"The footer records a file size of {Footer.FileSize} but the footer starts at offset {footerOffset}.");
        }

        // The checksums are not enforced: files that were edited by hand after saving are otherwise readable.
        var (byteSum, rotateXorChecksum) = FooterRecord.ComputeChecksums(data, footerOffset);
        IsChecksumValid = byteSum == Footer.ByteSum && rotateXorChecksum == Footer.RotateXorChecksum;

        SlackBytes = new byte[footerOffset - position];
        Buffer.BlockCopy(data, position, SlackBytes, 0, SlackBytes.Length);

        records.Add(Footer);
        Records = records;
    }

    private static void AddRecord(
        List<OrganizationChartRecord> records,
        Stack<OrganizationChartSubstream> substreamStack,
        OrganizationChartRecord record)
    {
        if (substreamStack.Count != 0)
        {
            substreamStack.Peek().Add(record);
        }
        else
        {
            records.Add(record);
        }
    }

    private static ushort ReadUInt16(byte[] data, ref int position)
    {
        if (data.Length - position < 2)
        {
            throw new InvalidDataException("The file ended unexpectedly: the file is truncated.");
        }

        ushort value = BitConverter.ToUInt16(data, position);
        position += 2;
        return value;
    }

    private static byte[] ReadAll(Stream stream)
    {
        if (stream == null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }
}
