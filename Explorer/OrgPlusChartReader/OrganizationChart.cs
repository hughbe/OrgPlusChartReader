using System.Text;

namespace OrgPlusChartReader;

public class OrganizationChart
{
    public IReadOnlyList<OrganizationChartRecord> Records { get; set; }

    public OrganizationChart(Stream stream)
    {
        if (stream == null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        // The header is 4 bytes: "UOCF"
        using var reader = new BinaryReader(stream);
        if (Encoding.ASCII.GetString(reader.ReadBytes(4)) != "UOCF")
        {
            throw new Exception("Invalid file signature");
        }

        // The rest of the file consists of records
        // Each record starts with a 2-byte ID and a 2-byte length, followed by the data.
        var records = new List<OrganizationChartRecord>();
        Stack<OrganizationChartSubstream> substreamStack = new();
        while (stream.Position < stream.Length)
        {
            // Some records are substreams and do not have length/data.
            // They have a start ID >= 0x4000
            // And a corresponding end ID = start ID + 0x2000
            var id = (OrganizationChartRecordId)reader.ReadUInt16();
            if (id >= (OrganizationChartRecordId)0x4000)
            {
                if (substreamStack.Count != 0 &&
                    id == substreamStack.Peek().Id + 0x2000)
                {
                    // This is the end of the current substream
                    substreamStack.Pop();
                }
                else
                {
                    // This is the start of a new substream
                    var substream = new OrganizationChartSubstream(id, reader);
                    substreamStack.Push(substream);
                    records.Add(substream);
                }
            }
            else
            {
                OrganizationChartRecord record = id switch
                {
                    OrganizationChartRecordId.Copyright => new CopyrightRecord(id, reader),
                    OrganizationChartRecordId.Copyright2 => new CopyrightRecord2(id, reader),
                    OrganizationChartRecordId.Unknown0x0002 => new Unknown0x0002Record(id, reader),
                    OrganizationChartRecordId.Unknown0x0003 => new Unknown0x0003Record(id, reader),
                    OrganizationChartRecordId.BoxTextPartCount => new BoxTextPartCountRecord(id, reader),
                    OrganizationChartRecordId.BoxTextPartIndex => new BoxTextPartIndexRecord(id, reader),
                    OrganizationChartRecordId.BoxTextPartType => new BoxTextPartTypeRecord(id, reader),
                    OrganizationChartRecordId.BodyInfo => new BodyInfoRecord(id, reader),
                    OrganizationChartRecordId.FontName => new FontNameRecord(id, reader),
                    OrganizationChartRecordId.TitlePartIndex => new TitlePartIndexRecord(id, reader),
                    OrganizationChartRecordId.Level => new LevelRecord(id, reader),
                    OrganizationChartRecordId.BoxIndex => new BoxIndexRecord(id, reader),
                    OrganizationChartRecordId.Border => new BorderRecord(id, reader),
                    OrganizationChartRecordId.Connector => new ConnectorRecord(id, reader),
                    OrganizationChartRecordId.Line => new LineRecord(id, reader),
                    OrganizationChartRecordId.Shape => new ShapeRecord(id, reader),
                    OrganizationChartRecordId.ShapeColor => new ShapeColorRecord(id, reader),
                    OrganizationChartRecordId.Selected => new SelectedRecord(id, reader),
                    OrganizationChartRecordId.Shadow => new ShadowRecord(id, reader),
                    OrganizationChartRecordId.FontIndex => new FontIndexRecord(id, reader),
                    OrganizationChartRecordId.Text => new TextRecord(id, reader),
                    OrganizationChartRecordId.GlobalInfo => new GlobalInfoRecord(id, reader),
                    OrganizationChartRecordId.Footer => new FooterRecord(id, reader),
                    // Not yet specialized, use base record for now
                    OrganizationChartRecordId.TextStyle => new OrganizationChartRecord(id, reader),
                    _ => !Enum.IsDefined(typeof(OrganizationChartRecordId), id)
                        ? throw new Exception($"Unknown record ID: 0x{((ushort)id).ToString("X4")}")
                        : new OrganizationChartRecord(id, reader),
                };

                if (substreamStack.Count != 0)
                {
                    substreamStack.Peek().Records.Add(record);
                }
                else
                {
                    records.Add(record);
                }
            }
        }

        Records = records;
    }
}
