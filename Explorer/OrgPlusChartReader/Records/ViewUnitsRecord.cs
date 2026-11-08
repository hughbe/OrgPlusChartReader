using System.ComponentModel;

namespace OrgPlusChartReader;

/// <summary>
/// The view units record (0x0021), called VIEWUNITS by the program. Coordinates and distances in the
/// file are in 1/<see cref="UnitsPerInch"/> inch. The program always writes {8, 0, 144, 100}.
/// </summary>
public class ViewUnitsRecord : OrganizationChartRecord
{
    public ushort Size { get; set; }

    [TypeConverter(typeof(HexTypeConverter))]
    public ushort Unknown { get; set; }

    /// <summary>The base number of units per inch (144).</summary>
    public ushort BaseUnits { get; set; }

    /// <summary>A percentage scale applied to <see cref="BaseUnits"/> (100).</summary>
    public ushort Scale { get; set; }

    /// <summary>
    /// Units per inch: BaseUnits × Scale / 100. The program rejects a file where this is 0.
    /// </summary>
    public int UnitsPerInch => BaseUnits * Scale / 100;

    internal ViewUnitsRecord(OrganizationChartRecordId id, byte[] bytes) : base(id, bytes)
    {
        Size = ReadUInt16(0);
        Unknown = ReadUInt16(2);
        BaseUnits = ReadUInt16(4);
        Scale = ReadUInt16(6);
    }
}
