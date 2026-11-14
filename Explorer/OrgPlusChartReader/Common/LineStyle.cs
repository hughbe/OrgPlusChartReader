namespace OrgPlusChartReader;

public enum LineStyle : ushort
{
    None = 0x00,
    Solid = 0x01,
    /// <summary>One pen width (kept between 2 and 6 units) on, one off.</summary>
    Dotted = 0x03,

    /// <summary>Three pen widths on, one off.</summary>
    Dashed = 0x04
}
