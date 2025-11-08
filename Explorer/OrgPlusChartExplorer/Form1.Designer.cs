
namespace OrganizationChartExplorer;

partial class Form1
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.elementHost1 = new System.Windows.Forms.Integration.ElementHost();
        this.sideBySideDiffViewer1 = new DiffPlex.Wpf.Controls.SideBySideDiffViewer();
        this.loadButton1 = new System.Windows.Forms.Button();
        this.fileTextBox1 = new System.Windows.Forms.TextBox();
        this.fileTextBox2 = new System.Windows.Forms.TextBox();
        this.loadButton2 = new System.Windows.Forms.Button();
        this.generateDiffButton = new System.Windows.Forms.Button();
        this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
        this.tabControl1 = new System.Windows.Forms.TabControl();
        this.tabPage1 = new System.Windows.Forms.TabPage();
        this.tabPage2 = new System.Windows.Forms.TabPage();
        this.propertyGrid = new System.Windows.Forms.PropertyGrid();
        this.byteViewer = new System.ComponentModel.Design.ByteViewer();
        this.dumpTreeView = new System.Windows.Forms.TreeView();
        this.dumpButton = new System.Windows.Forms.Button();
        this.dumpTextBox = new System.Windows.Forms.TextBox();
        this.tabControl1.SuspendLayout();
        this.tabPage1.SuspendLayout();
        this.tabPage2.SuspendLayout();
        this.SuspendLayout();
        // 
        // elementHost1
        // 
        this.elementHost1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.elementHost1.Location = new System.Drawing.Point(7, 64);
        this.elementHost1.Name = "elementHost1";
        this.elementHost1.Size = new System.Drawing.Size(1298, 412);
        this.elementHost1.TabIndex = 0;
        this.elementHost1.Text = "elementHost1";
        this.elementHost1.Child = this.sideBySideDiffViewer1;
        // 
        // loadButton1
        // 
        this.loadButton1.Location = new System.Drawing.Point(967, 8);
        this.loadButton1.Name = "loadButton1";
        this.loadButton1.Size = new System.Drawing.Size(75, 23);
        this.loadButton1.TabIndex = 1;
        this.loadButton1.Text = "Load File 1";
        this.loadButton1.UseVisualStyleBackColor = true;
        this.loadButton1.Click += new System.EventHandler(this.loadButton1_Click);
        // 
        // fileTextBox1
        // 
        this.fileTextBox1.Location = new System.Drawing.Point(8, 10);
        this.fileTextBox1.Name = "fileTextBox1";
        this.fileTextBox1.Size = new System.Drawing.Size(953, 20);
        this.fileTextBox1.TabIndex = 2;
        this.fileTextBox1.KeyUp += new System.Windows.Forms.KeyEventHandler(this.fileTextBox1_KeyUp);
        // 
        // fileTextBox2
        // 
        this.fileTextBox2.Location = new System.Drawing.Point(8, 37);
        this.fileTextBox2.Name = "fileTextBox2";
        this.fileTextBox2.Size = new System.Drawing.Size(953, 20);
        this.fileTextBox2.TabIndex = 4;
        this.fileTextBox2.KeyUp += new System.Windows.Forms.KeyEventHandler(this.fileTextBox2_KeyUp);
        // 
        // loadButton2
        // 
        this.loadButton2.Location = new System.Drawing.Point(967, 35);
        this.loadButton2.Name = "loadButton2";
        this.loadButton2.Size = new System.Drawing.Size(75, 23);
        this.loadButton2.TabIndex = 3;
        this.loadButton2.Text = "Load File 2";
        this.loadButton2.UseVisualStyleBackColor = true;
        this.loadButton2.Click += new System.EventHandler(this.loadButton2_Click);
        // 
        // generateDiffButton
        // 
        this.generateDiffButton.Location = new System.Drawing.Point(1048, 6);
        this.generateDiffButton.Name = "generateDiffButton";
        this.generateDiffButton.Size = new System.Drawing.Size(172, 51);
        this.generateDiffButton.TabIndex = 5;
        this.generateDiffButton.Text = "Generate Diff";
        this.generateDiffButton.UseVisualStyleBackColor = true;
        this.generateDiffButton.Click += new System.EventHandler(this.generateDiffButton_Click);
        // 
        // openFileDialog1
        // 
        this.openFileDialog1.DefaultExt = "opx";
        this.openFileDialog1.FileName = "openFileDialog1";
        this.openFileDialog1.Filter = "Organization Chart Files (*.opx)|*.opx|All files (*.*)|*.*";
        // 
        // tabControl1
        // 
        this.tabControl1.Controls.Add(this.tabPage1);
        this.tabControl1.Controls.Add(this.tabPage2);
        this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tabControl1.Location = new System.Drawing.Point(0, 0);
        this.tabControl1.Name = "tabControl1";
        this.tabControl1.SelectedIndex = 0;
        this.tabControl1.Size = new System.Drawing.Size(1234, 510);
        this.tabControl1.TabIndex = 6;
        // 
        // tabPage1
        // 
        this.tabPage1.Controls.Add(this.loadButton2);
        this.tabPage1.Controls.Add(this.generateDiffButton);
        this.tabPage1.Controls.Add(this.elementHost1);
        this.tabPage1.Controls.Add(this.fileTextBox2);
        this.tabPage1.Controls.Add(this.loadButton1);
        this.tabPage1.Controls.Add(this.fileTextBox1);
        this.tabPage1.Location = new System.Drawing.Point(4, 22);
        this.tabPage1.Name = "tabPage1";
        this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
        this.tabPage1.Size = new System.Drawing.Size(1226, 484);
        this.tabPage1.TabIndex = 0;
        this.tabPage1.Text = "Diff";
        this.tabPage1.UseVisualStyleBackColor = true;
        // 
        // tabPage2
        // 
        this.tabPage2.Controls.Add(this.propertyGrid);
        this.tabPage2.Controls.Add(this.byteViewer);
        this.tabPage2.Controls.Add(this.dumpTreeView);
        this.tabPage2.Controls.Add(this.dumpButton);
        this.tabPage2.Controls.Add(this.dumpTextBox);
        this.tabPage2.Location = new System.Drawing.Point(4, 22);
        this.tabPage2.Name = "tabPage2";
        this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
        this.tabPage2.Size = new System.Drawing.Size(1226, 484);
        this.tabPage2.TabIndex = 1;
        this.tabPage2.Text = "Dump";
        this.tabPage2.UseVisualStyleBackColor = true;
        // 
        // propertyGrid
        // 
        this.propertyGrid.Location = new System.Drawing.Point(978, 33);
        this.propertyGrid.Name = "propertyGrid";
        this.propertyGrid.PropertySort = System.Windows.Forms.PropertySort.NoSort;
        this.propertyGrid.Size = new System.Drawing.Size(240, 442);
        this.propertyGrid.TabIndex = 4;
        // 
        // byteViewer
        // 
        this.byteViewer.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Inset;
        this.byteViewer.ColumnCount = 1;
        this.byteViewer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.byteViewer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.byteViewer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.byteViewer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.byteViewer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.byteViewer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.byteViewer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.byteViewer.Location = new System.Drawing.Point(338, 33);
        this.byteViewer.Name = "byteViewer";
        this.byteViewer.RowCount = 1;
        this.byteViewer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.byteViewer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.byteViewer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.byteViewer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.byteViewer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.byteViewer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.byteViewer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.byteViewer.Size = new System.Drawing.Size(634, 430);
        this.byteViewer.TabIndex = 3;
        // 
        // dumpTreeView
        // 
        this.dumpTreeView.Location = new System.Drawing.Point(9, 33);
        this.dumpTreeView.Name = "dumpTreeView";
        this.dumpTreeView.Size = new System.Drawing.Size(323, 443);
        this.dumpTreeView.TabIndex = 2;
        this.dumpTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.dumpTreeView_AfterSelect);
        // 
        // dumpButton
        // 
        this.dumpButton.Location = new System.Drawing.Point(1143, 5);
        this.dumpButton.Name = "dumpButton";
        this.dumpButton.Size = new System.Drawing.Size(75, 23);
        this.dumpButton.TabIndex = 1;
        this.dumpButton.Text = "Dump Button";
        this.dumpButton.UseVisualStyleBackColor = true;
        this.dumpButton.Click += new System.EventHandler(this.dumpButton_Click);
        // 
        // dumpTextBox
        // 
        this.dumpTextBox.Location = new System.Drawing.Point(9, 7);
        this.dumpTextBox.Name = "dumpTextBox";
        this.dumpTextBox.Size = new System.Drawing.Size(1128, 20);
        this.dumpTextBox.TabIndex = 0;
        this.dumpTextBox.TextChanged += new System.EventHandler(this.dumpTextBox_TextChanged);
        this.dumpTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.dumpTextBox_KeyUp);
        // 
        // Form1
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(1234, 510);
        this.Controls.Add(this.tabControl1);
        this.Name = "Form1";
        this.Text = "Form1";
        this.tabControl1.ResumeLayout(false);
        this.tabPage1.ResumeLayout(false);
        this.tabPage1.PerformLayout();
        this.tabPage2.ResumeLayout(false);
        this.tabPage2.PerformLayout();
        this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Integration.ElementHost elementHost1;
    private DiffPlex.Wpf.Controls.SideBySideDiffViewer sideBySideDiffViewer1;
    private System.Windows.Forms.Button loadButton1;
    private System.Windows.Forms.TextBox fileTextBox1;
    private System.Windows.Forms.TextBox fileTextBox2;
    private System.Windows.Forms.Button loadButton2;
    private System.Windows.Forms.Button generateDiffButton;
    private System.Windows.Forms.OpenFileDialog openFileDialog1;
    private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.TabPage tabPage2;
    private System.Windows.Forms.Button dumpButton;
    private System.Windows.Forms.TextBox dumpTextBox;
    private System.Windows.Forms.TreeView dumpTreeView;
    private System.ComponentModel.Design.ByteViewer byteViewer;
    private System.Windows.Forms.PropertyGrid propertyGrid;
}

