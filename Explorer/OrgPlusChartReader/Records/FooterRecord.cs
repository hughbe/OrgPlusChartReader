using System.ComponentModel;

namespace OrgPlusChartReader;

/// <summary>
/// The footer record (0x0FFF): the last 14 bytes of the file.
/// </summary>
public class FooterRecord : OrganizationChartRecord
{
    public ushort Size { get; set; }

    /// <summary>
    /// The sum of every byte before the footer, modulo 2^16.
    /// </summary>
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort ByteSum { get; set; }

    /// <summary>
    /// The rotate-and-XOR checksum of every byte before the footer. See <see cref="ComputeChecksums"/>.
    /// </summary>
    [TypeConverter(typeof(HexTypeConverter))]
    public ushort RotateXorChecksum { get; set; }

    /// <summary>
    /// The offset of the footer, which is the length of the file minus 14.
    /// </summary>
    [TypeConverter(typeof(HexTypeConverter))]
    public uint FileSize { get; set; }

    internal FooterRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        int offset = 0;

        Size = ReadUInt16(offset);
        offset += 2;

        ByteSum = ReadUInt16(offset);
        offset += 2;

        RotateXorChecksum = ReadUInt16(offset);
        offset += 2;

        FileSize = ReadUInt32(offset);
    }

    /// <summary>
    /// Computes the two footer checksums over <paramref name="count"/> bytes of <paramref name="data"/>,
    /// starting at the beginning of the file.
    /// </summary>
    public static (ushort ByteSum, ushort RotateXorChecksum) ComputeChecksums(byte[] data, int count)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        if (count < 0 || count > data.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        ushort sum = 0;
        ushort rotateXor = 0;
        for (int i = 0; i < count; i++)
        {
            sum = (ushort)(sum + data[i]);
            rotateXor ^= data[i];
            rotateXor = (ushort)((rotateXor << 1) | (rotateXor >> 15));
        }

        return (sum, rotateXor);
    }
}
