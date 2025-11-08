using System;
using System.IO;
using System.Text;

namespace OrgPlusChartReader;

public class OrganizationChartRecord
{
    public OrganizationChartRecordId Id { get; }
    public int RecordSize { get; set; }
    public byte[] Bytes { get; set; }

    internal OrganizationChartRecord(OrganizationChartRecordId id, BinaryReader reader)
    {
        Id = id;

        if ((int)id >= 0x4000)
        {
            // Substream records do not have a length field
            RecordSize = 0;
            Bytes = Array.Empty<byte>();
        }
        else
        {
            // Other records have a length field of 2 bytes
            RecordSize = reader.ReadUInt16();
            Bytes = reader.ReadBytes(RecordSize);
        }
    }

    protected byte ReadByte(int offset)
    {
        return Bytes[offset];
    }

    protected ushort ReadUInt16(int offset)
    {
        return BitConverter.ToUInt16(Bytes, offset);
    }

    protected short ReadInt16(int offset)
    {
        return BitConverter.ToInt16(Bytes, offset);
    }

    protected uint ReadUInt32(int offset)
    {
        return BitConverter.ToUInt32(Bytes, offset);
    }

    protected System.Drawing.Color ReadColor(int offset)
    {
        byte red = Bytes[offset];
        byte blue = Bytes[offset + 1];
        byte green = Bytes[offset + 2];
        // byte _ = Bytes[offset + 3]; // ignored
        return System.Drawing.Color.FromArgb(red, green, blue);
    }

    protected string ReadASCIIString(int offset, int length)
    {
        return Encoding.ASCII.GetString(Bytes, offset, length);
    }

    protected string ReadNullTerminatedASCIIString(int offset, out int bytesRead)
    {
        int endOffset = offset;
        while (endOffset < Bytes.Length && Bytes[endOffset] != 0)
        {
            endOffset++;
        }

        bytesRead = endOffset - offset + 1; // +1 for null terminator
        return Encoding.ASCII.GetString(Bytes, offset, endOffset - offset);
    }
}
