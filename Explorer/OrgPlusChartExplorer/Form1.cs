using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using OrgPlusChartReader;

namespace OrganizationChartExplorer;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
        sideBySideDiffViewer1.FontFamily = new System.Windows.Media.FontFamily("Consolas");
        openFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    }

    private bool TryOpenFile(out string fileName)
    {
        if (openFileDialog1.ShowDialog() == DialogResult.OK)
        {
            fileName = openFileDialog1.FileName;
            return true;
        }

        fileName = null;
        return false;
    }

    private void loadButton1_Click(object sender, EventArgs e)
    {
        if (TryOpenFile(out string fileName))
        {
            fileTextBox1.Text = fileName;
        }

        if (fileTextBox2.Text.Length > 0)
        {
            GenerateDiff();
        }
    }

    private void loadButton2_Click(object sender, EventArgs e)
    {
        if (TryOpenFile(out string fileName))
        {
            fileTextBox2.Text = fileName;
        }

        if (fileTextBox1.Text.Length > 0)
        {
            GenerateDiff();
        }
    }

    private void generateDiffButton_Click(object sender, EventArgs e)
    {
        GenerateDiff();
    }

    private void fileTextBox1_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter && fileTextBox2.Text.Length > 0)
        {
            GenerateDiff();
        }
    }

    private void fileTextBox2_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter && fileTextBox1.Text.Length > 0)
        {
            GenerateDiff();
        }
    }

    private bool TryLoadChart(string path, out OrganizationChart chart)
    {
        if (!File.Exists(path))
        {
            MessageBox.Show($"File does not exist: {path}");
            chart = null;
            return false;
        }

        using FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        chart = new OrganizationChart(stream);
        return true;
    }

    private void GenerateDiff()
    {
        sideBySideDiffViewer1.SetDiffModel("", "");

        if (TryLoadChart(fileTextBox1.Text, out OrganizationChart chart1) &&
            TryLoadChart(fileTextBox2.Text, out OrganizationChart chart2))
        {
            sideBySideDiffViewer1.SetDiffModel(DumpFile(chart1), DumpFile(chart2));
        }
    }

    private string DumpFile(OrganizationChart chart)
    {
        string HexDump(byte[] data)
        {
            if (data.Length == 0)
            {
                return "[]";
            }

            var sb = new StringBuilder();
            sb.AppendLine("[");
            sb.Append("    ");
            for (int i = 0; i < data.Length; i++)
            {
                if (i != 0 && (i % 4) == 0)
                {
                    sb.AppendLine();
                    sb.Append("    ");
                }

                sb.Append($"0x{data[i].ToString("X2")}");

                if (i != data.Length - 1)
                {
                    sb.Append(", ");
                }
            }
            sb.AppendLine();
            sb.Append("]");
            return sb.ToString();
        }

        var builder = new StringBuilder();
        foreach ((ushort id, byte[] data) in chart.Records)
        {
            builder.AppendLine($"Id: {GetName(id)}");
            builder.AppendLine($"Data: {HexDump(data)}");
            builder.AppendLine();
        }

        return builder.ToString();
    }

    private void dumpButton_Click(object sender, EventArgs e)
    {
        if (_dumpDirty)
        {
            if (TryLoadChart(dumpTextBox.Text, out OrganizationChart chart))
            {
                DumpChartToTreeView(chart);
            }
        }
        else
        {
            if (TryOpenFile(out string fileName))
            {
                dumpTextBox.Text = fileName;

                if (TryLoadChart(fileName, out OrganizationChart chart))
                {
                    DumpChartToTreeView(chart);
                }
            }
        }
    }

    private string GetName(ushort id)
    {
        OrganizationChartRecordId enumValue = (OrganizationChartRecordId)id;
        switch (enumValue)
        {
            case OrganizationChartRecordId.BoxTextPartCount:
                return $"Part Count (0x{id:X4})";
            case OrganizationChartRecordId.BoxTextPartIndex:
                return $"Part Index (0x{id:X4})";
            case OrganizationChartRecordId.BoxTextPartType:
                return $"Part Type (0x{id:X4})";
            case OrganizationChartRecordId.BodyInfo:
                return $"Body Info (0x{id:X4})";
            case OrganizationChartRecordId.FontName:
                return $"Font Name (0x{id:X4})";
            case OrganizationChartRecordId.TitlePartIndex:
                return $"Title Part Index (0x{id:X4})";
            case OrganizationChartRecordId.Level:
                return $"Level (0x{id:X4})";
            case OrganizationChartRecordId.BoxIndex:
                return $"Box Index (0x{id:X4})";
            case OrganizationChartRecordId.Border:
                return $"Border (0x{id:X4})";
            case OrganizationChartRecordId.Connector:
                return $"Connector (0x{id:X4})";
            case OrganizationChartRecordId.Line:
                return $"Line (0x{id:X4})";
            case OrganizationChartRecordId.Text:
                return $"Text (0x{id:X4})";
            case OrganizationChartRecordId.TextStyle:
                return $"Text Style (0x{id:X4})";
            case OrganizationChartRecordId.Shape:
                return $"Shape (0x{id:X4})";
            case OrganizationChartRecordId.ShapeColor:
                return $"Shape Color (0x{id:X4})";
            case OrganizationChartRecordId.Selected:
                return $"Selected (0x{id:X4})";
            case OrganizationChartRecordId.Shadow:
                return $"Shadow (0x{id:X4})";
            case OrganizationChartRecordId.FontIndex:
                return $"Font Index (0x{id:X4})";
            case OrganizationChartRecordId.GlobalInfo:
                return $"Global Info (0x{id:X4})";
            case OrganizationChartRecordId.Footer:
                return $"Footer (0x{id:X4})";

            case OrganizationChartRecordId.ParentSection:
                return $"Parent Section (0x{id:X4})";
            case OrganizationChartRecordId.ContainerSection:
                return $"Container Section (0x{id:X4})";
            case OrganizationChartRecordId.CanvasSection:
                return $"Canvas Section (0x{id:X4})";
            case OrganizationChartRecordId.GlobalInfoSection:
                return $"Global Info Section (0x{id:X4})";
            case OrganizationChartRecordId.LevelsSection:
                return $"Levels Section (0x{id:X4})";
            case OrganizationChartRecordId.LevelSection:
                return $"Level Section (0x{id:X4})";
           case OrganizationChartRecordId.BoxTextPartsSection:
                return $"Box Text Parts Section (0x{id:X4})";
            case OrganizationChartRecordId.TitleSection:
                return $"Title Section (0x{id:X4})";
            case OrganizationChartRecordId.FontSection:
                return $"Font Section (0x{id:X4})";
            case OrganizationChartRecordId.LinesSection:
                return $"Lines Section (0x{id:X4})";
            case OrganizationChartRecordId.ShapesSection:
                return $"Shapes Section (0x{id:X4})";
            case OrganizationChartRecordId.TextStyleSection:
                return $"Text Style Section (0x{id:X4})";
            case OrganizationChartRecordId.ConnectorsSection:
                return $"Connectors Section (0x{id:X4})";
            default:
                return $"0x{id:X4}";
        }
    }

    private void DumpChartToTreeView(OrganizationChart chart)
    {
        _dumpDirty = false;
        dumpTreeView.Nodes.Clear();

        TreeNode node = dumpTreeView.Nodes.Add("Root");

        foreach ((ushort id, byte[] data) in chart.Records)
        {
            if (id >= 0x4000)
            {
                if (node.Tag is ValueTuple<ushort, byte[]> tag && tag.Item1 == id - 0x2000)
                {
                    node = node.Parent;
                }
                else
                {
                    node = node.Nodes.Add(GetName(id));
                    node.Tag = (id, data);
                }
            }
            else
            {
                TreeNode newNode = node.Nodes.Add(GetName(id));
                newNode.Tag = (id, data);
            }
        }

        dumpTreeView.ExpandAll();
    }

    private void dumpTreeView_AfterSelect(object sender, TreeViewEventArgs e)
    {
        if (dumpTreeView.SelectedNode != null && dumpTreeView.SelectedNode.Tag is ValueTuple<ushort, byte[]> tag)
        {
            Display(tag.Item1, tag.Item2);
        }
        else
        {
            byteViewer.SetBytes([]);
            propertyGrid.SelectedObject = new object();
            propertyGrid.Update();
        }
    }

    private void Display(ushort id, byte[] data)
    {
        byteViewer.SetBytes(data);
        using (var stream = new MemoryStream(data))
        using (var reader = new BinaryReader(stream))
        {
            propertyGrid.SelectedObject = (OrganizationChartRecordId)(OrganizationChartRecordId)id switch
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
                _ => new object(),
            };
            propertyGrid.Update();
        }
    }

    private bool _dumpDirty = false;

    private void dumpTextBox_TextChanged(object sender, EventArgs e)
    {
        _dumpDirty = true;
    }

    private void dumpTextBox_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter && TryLoadChart(dumpTextBox.Text, out OrganizationChart chart))
        {
            DumpChartToTreeView(chart);
        }
    }
}
