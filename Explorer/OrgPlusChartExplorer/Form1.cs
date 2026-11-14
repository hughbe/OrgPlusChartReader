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

        try
        {
            using FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            chart = new OrganizationChart(stream);
            return true;
        }
        catch (Exception ex) when (ex is InvalidDataException || ex is IOException || ex is UnauthorizedAccessException)
        {
            MessageBox.Show($"Could not read {path}: {ex.Message}");
            chart = null;
            return false;
        }
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
        void Append(OrganizationChartRecord record, int depth)
        {
            string indent = new string(' ', depth * 2);
            builder.AppendLine($"{indent}Id: {GetName((ushort)record.Id)}");
            if (record is OrganizationChartSubstream substream)
            {
                foreach (OrganizationChartRecord child in substream.Records)
                {
                    Append(child, depth + 1);
                }
            }
            else
            {
                builder.AppendLine($"{indent}Data: {HexDump(record.Bytes)}");
            }

            builder.AppendLine();
        }

        foreach (OrganizationChartRecord record in chart.Records)
        {
            Append(record, 0);
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
            case OrganizationChartRecordId.Group:
                return $"Group (0x{id:X4})";
            case OrganizationChartRecordId.Box:
                return $"Box (0x{id:X4})";
            case OrganizationChartRecordId.BoxTemplate:
                return $"Box Template (0x{id:X4})";
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
            case OrganizationChartRecordId.GroupsSection:
                return $"Groups Section (0x{id:X4})";
            case OrganizationChartRecordId.BoxesSection:
                return $"Boxes Section (0x{id:X4})";
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

        void AddNodes(TreeNodeCollection nodes, OrganizationChartRecord record)
        {
            TreeNode node = nodes.Add(GetName((ushort)record.Id));
            node.Tag = record;
            if (record is OrganizationChartSubstream substream)
            {
                foreach (OrganizationChartRecord child in substream.Records)
                {
                    AddNodes(node.Nodes, child);
                }
            }
        }

        TreeNode root = dumpTreeView.Nodes.Add("Root");
        foreach (OrganizationChartRecord record in chart.Records)
        {
            AddNodes(root.Nodes, record);
        }

        dumpTreeView.ExpandAll();
    }

    private void dumpTreeView_AfterSelect(object sender, TreeViewEventArgs e)
    {
        if (dumpTreeView.SelectedNode?.Tag is OrganizationChartRecord record)
        {
            // Records are already parsed into their specific types, so show them directly.
            byteViewer.SetBytes(record.Bytes);
            propertyGrid.SelectedObject = record;
        }
        else
        {
            byteViewer.SetBytes(new byte[0]);
            propertyGrid.SelectedObject = new object();
        }

        propertyGrid.Update();
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
