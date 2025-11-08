// Read the file path from command line arguments.
using OrgPlusChartReader;

var filePath = args.Length > 0 ? args[0] : throw new ArgumentException("Please provide the path to the organization chart file.");
using var stream = File.OpenRead(filePath);

var chart = new OrganizationChart(stream);

foreach (var record in chart.Records)
{
    Console.WriteLine($"{record.id}: {GetRecordDetails((OrganizationChartRecordId)record.id, record.data)}");
}

static string GetRecordDetails(OrganizationChartRecordId id, byte[] data)
{
    using var stream = new MemoryStream(data);
    using var reader = new BinaryReader(stream);
    object obj = id switch
    {
        OrganizationChartRecordId.BoxTextPartCount => new BoxTextPartCountRecord(reader),
        OrganizationChartRecordId.BoxTextPartIndex => new BoxTextPartIndexRecord(reader),
        OrganizationChartRecordId.BoxTextPartType => new BoxTextPartTypeRecord(reader),
        OrganizationChartRecordId.BodyInfo => new BodyInfoRecord(reader),
        OrganizationChartRecordId.FontName => new FontNameRecord(reader),
        OrganizationChartRecordId.TitlePartIndex => new TitlePartIndexRecord(reader),
        OrganizationChartRecordId.Level => new LevelRecord(reader),
        OrganizationChartRecordId.BoxIndex => new BoxIndexRecord(reader),
        OrganizationChartRecordId.Border => new BorderRecord(reader),
        OrganizationChartRecordId.Connector => new ConnectorRecord(reader),
        OrganizationChartRecordId.Line => new LineRecord(reader),
        OrganizationChartRecordId.Text => new TextRecord(reader),
        OrganizationChartRecordId.Shape => new ShapeRecord(reader),
        OrganizationChartRecordId.ShapeColor => new ShapeColorRecord(reader),
        OrganizationChartRecordId.Selected => new SelectedRecord(reader),
        OrganizationChartRecordId.Shadow => new ShadowRecord(reader),
        OrganizationChartRecordId.FontIndex => new FontIndexRecord(reader),
        OrganizationChartRecordId.Footer => new FooterRecord(reader),
        OrganizationChartRecordId.GlobalInfo => new GlobalInfoRecord(reader),
        _ => throw new NotSupportedException($"Record ID {id} is not supported."),
    };
    return obj.ToString()!;
}
