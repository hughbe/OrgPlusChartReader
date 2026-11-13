namespace OrgPlusChartReader;

/// <summary>
/// An Org Plus print time code (0x0FFC). Microsoft Organization Chart never writes it. In the global
/// info section only its presence matters: it marks an older file format. In a box's text section it
/// follows a field index and holds a code; the program only acts on head-count codes, which start with '#'.
/// </summary>
public class PrintTimeCodeRecord : OrganizationChartRecord
{
    /// <summary>The code text, up to the first NUL.</summary>
    public string Code { get; set; }

    /// <summary>Whether the code is a head-count code ("#"): show the number of boxes in this box's branch.</summary>
    public bool IsHeadCount => Code.Length > 0 && Code[0] == '#';

    /// <summary>For a head-count code, whether the box at the head of the branch is left out ("#S").</summary>
    public bool ExcludesManager => IsHeadCount && Code.Length > 1 && (Code[1] == 'S' || Code[1] == 's');

    internal PrintTimeCodeRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        Code = ReadNullTerminatedString(0, out _);
    }
}
