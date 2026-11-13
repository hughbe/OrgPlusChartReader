namespace OrgPlusChartReader;

public enum OrganizationChartRecordId : ushort
{
    Password = 0x0001,
    Unknown0x0002 = 0x0002,
    Unknown0x0003 = 0x0003,
    BoxTextPartCount = 0x0010,
    BoxTextPartIndex = 0x0011,
    /// <summary>The field's label type: 1 Name, 2 Text, 3 Amount, 4 Picture.</summary>
    BoxTextPartType = 0x0012,
    /// <summary>Written by Org Plus only; Microsoft Organization Chart ignores it when reading.</summary>
    Formula = 0x0013,
    BodyInfo = 0x0020,
    ViewUnits = 0x0021,
    PageSetup = 0x0022,
    BoxTemplate = 0x0023,
    /// <summary>The id of the root group (the group the groups section starts from).</summary>
    RootGroup = 0x0024,
    FontName = 0x0025,
    TitlePartIndex = 0x0026,
    Group = 0x0041,
    GroupLayout = 0x0042,
    BoxIndex = 0x0050,
    Box = 0x0051,
    /// <summary>A program-wide setting. The program ignores it when reading.</summary>
    Unknown0x0052 = 0x0052,
    Border = 0x0053,
    BoxLayout = 0x0054,
    Connector = 0x0055,
    Line = 0x0056,
    TextStyle = 0x0061,
    NumericValue = 0x0070,
    NumberFormat = 0x0071,
    PictureSize = 0x0080,
    BitmapHeader = 0x0081,
    BitmapData = 0x0082,
    Shape = 0x0083,
    ShapeColor = 0x0084,
    PictureData = 0x0085,
    PictureBox = 0x0087,
    Selected = 0x0088,
    Shadow = 0x0092,
    LineShadow = 0x0093,
    Arrow = 0x0094,
    /// <summary>The border of the chart frame: a border drawn around the chart at the page margins.</summary>
    ChartFrameBorder = 0x0095,
    /// <summary>The shadow of the chart frame. See <see cref="ChartFrameBorder"/>.</summary>
    ChartFrameShadow = 0x0096,
    Anchor = 0x0097,
    FontIndex = 0x009C,
    Text = 0x0060,
    DevMode = 0x0089,
    ColorPalette = 0x0090,
    EmbeddedSize = 0x0091,
    /// <summary>{4, v}: v is 0 when formulas are recalculated automatically.</summary>
    Recalculation = 0x0098,
    FieldFlags = 0x0099,
    /// <summary>Marks a box that is excluded from calculated fields (box flag 0x200).</summary>
    ExcludeFromCalculations = 0x009A,
    FitToPages = 0x009B,
    Copyright = 0x0D0A,
    /// <summary>
    /// An Org Plus print time code. Never written by Microsoft Organization Chart. In the global info
    /// section its presence marks an older file format; in a box's text it holds a code such as "#".
    /// </summary>
    PrintTimeCode = 0x0FFC,
    GlobalInfo = 0x0FFD,
    Footer = 0x0FFF,
    /// <summary>
    /// The first preserved record. Records 0x1000 to 0x13E7 are written back unchanged by Org Plus;
    /// Microsoft Organization Chart drops them.
    /// </summary>
    Unknown0x1000 = 0x1000,

    ParentSection = 0x4001,
    ContainerSection = 0x4002,
    CanvasSection = 0x4003,
    GlobalInfoSection = 0x4005,
    GroupsSection = 0x4006,
    BoxesSection = 0x4007,
    BoxTextPartsSection = 0x4008,
    TitleSection = 0x4009,
    FontSection = 0x400B,
    LinesSection = 0x400C,
    ShapesSection = 0x400E,
    TextStyleSection = 0x4010,
    ConnectorsSection = 0x4012,
}
