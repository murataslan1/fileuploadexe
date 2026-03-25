using ImageToPdfMerger.Controls;

namespace ImageToPdfMerger.Forms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    private SplitContainer splitMain;
    private DropZonePanel dropZone;
    private Panel searchPanel;
    private TextBox txtSearch;
    private Button btnClearSearch;
    private ThumbnailListView listViewImages;
    private FlowLayoutPanel toolbarLeft;
    private Button btnDelete;
    private Button btnMoveUp;
    private Button btnMoveDown;
    private Button btnClearAll;

    private PictureBox picPreview;
    private Panel navPanel;
    private Button btnPrevPage;
    private Label lblPageInfo;
    private Button btnNextPage;
    private FlowLayoutPanel toolbarRight;
    private Label lblPageSizeLabel;
    private ComboBox cmbPageSize;
    private Label lblOrientationLabel;
    private ComboBox cmbOrientation;
    private Label lblScaleModeLabel;
    private ComboBox cmbScaleMode;
    private CheckBox chkMargin;
    private Button btnMergePreview;
    private Button btnSavePdf;

    private StatusStrip statusStrip;
    private ToolStripStatusLabel lblStatus;
    private ToolStripProgressBar progressBar;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        AutoScaleMode = AutoScaleMode.Font;
        Text = "Image to PDF Merger";
        Size = new System.Drawing.Size(1200, 750);
        MinimumSize = new System.Drawing.Size(900, 600);
        StartPosition = FormStartPosition.CenterScreen;
        KeyPreview = true;

        // === StatusStrip ===
        statusStrip = new StatusStrip();
        lblStatus = new ToolStripStatusLabel("Ready") { Spring = true, TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
        progressBar = new ToolStripProgressBar { Visible = false, Minimum = 0, Maximum = 100 };
        statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus, progressBar });
        Controls.Add(statusStrip);

        // === SplitContainer ===
        splitMain = new SplitContainer
        {
            Dock = DockStyle.Fill,
            FixedPanel = FixedPanel.Panel1,
            SplitterDistance = 370,
            SplitterWidth = 5
        };
        Controls.Add(splitMain);

        // ==============================
        // LEFT PANEL (Panel1)
        // ==============================

        // --- DropZone (Top) ---
        dropZone = new DropZonePanel();
        splitMain.Panel1.Controls.Add(dropZone);

        // --- Search Panel (Top, below DropZone) ---
        searchPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 35,
            Padding = new Padding(5, 5, 5, 2)
        };

        btnClearSearch = new Button
        {
            Text = "X",
            Dock = DockStyle.Right,
            Width = 30,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnClearSearch.FlatAppearance.BorderSize = 0;

        txtSearch = new TextBox
        {
            Dock = DockStyle.Fill,
            Font = new System.Drawing.Font("Segoe UI", 10f),
            PlaceholderText = "Search images... (Ctrl+F)"
        };

        searchPanel.Controls.Add(txtSearch);
        searchPanel.Controls.Add(btnClearSearch);
        splitMain.Panel1.Controls.Add(searchPanel);

        // --- Toolbar Left (Bottom) ---
        toolbarLeft = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 45,
            Padding = new Padding(5),
            FlowDirection = FlowDirection.LeftToRight
        };

        btnDelete = CreateToolbarButton("Delete", "Delete selected images");
        btnMoveUp = CreateToolbarButton("Up", "Move selected up");
        btnMoveDown = CreateToolbarButton("Down", "Move selected down");
        btnClearAll = CreateToolbarButton("Clear All", "Clear all images");

        toolbarLeft.Controls.AddRange(new Control[] { btnDelete, btnMoveUp, btnMoveDown, btnClearAll });
        splitMain.Panel1.Controls.Add(toolbarLeft);

        // --- ListView (Fill, remaining space) ---
        listViewImages = new ThumbnailListView
        {
            Dock = DockStyle.Fill
        };
        splitMain.Panel1.Controls.Add(listViewImages);

        // Fix dock order: Fill must be added last conceptually,
        // but in WinForms the order in Controls determines fill priority.
        // Re-order: dropZone(Top) -> searchPanel(Top) -> toolbarLeft(Bottom) -> listView(Fill)
        splitMain.Panel1.Controls.SetChildIndex(dropZone, 3);
        splitMain.Panel1.Controls.SetChildIndex(searchPanel, 2);
        splitMain.Panel1.Controls.SetChildIndex(toolbarLeft, 1);
        splitMain.Panel1.Controls.SetChildIndex(listViewImages, 0);

        // ==============================
        // RIGHT PANEL (Panel2)
        // ==============================

        // --- Toolbar Right (Bottom) ---
        toolbarRight = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 70,
            Padding = new Padding(5),
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true
        };

        lblPageSizeLabel = new Label { Text = "Page:", AutoSize = true, Margin = new Padding(3, 8, 0, 0) };
        cmbPageSize = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 80,
            Margin = new Padding(3, 5, 10, 0)
        };
        cmbPageSize.Items.AddRange(new object[] { "A4", "Letter", "A3", "Original" });
        cmbPageSize.SelectedIndex = 0;

        lblOrientationLabel = new Label { Text = "Orientation:", AutoSize = true, Margin = new Padding(3, 8, 0, 0) };
        cmbOrientation = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 95,
            Margin = new Padding(3, 5, 10, 0)
        };
        cmbOrientation.Items.AddRange(new object[] { "Portrait", "Landscape" });
        cmbOrientation.SelectedIndex = 0;

        lblScaleModeLabel = new Label { Text = "Scale:", AutoSize = true, Margin = new Padding(3, 8, 0, 0) };
        cmbScaleMode = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 110,
            Margin = new Padding(3, 5, 10, 0)
        };
        cmbScaleMode.Items.AddRange(new object[] { "Fit to Page", "Original Size", "Fit to Width" });
        cmbScaleMode.SelectedIndex = 0;

        chkMargin = new CheckBox
        {
            Text = "Margin (10mm)",
            Checked = true,
            AutoSize = true,
            Margin = new Padding(10, 8, 10, 0)
        };

        btnMergePreview = new Button
        {
            Text = "Merge && Preview",
            Width = 120,
            Height = 32,
            Margin = new Padding(10, 3, 5, 0),
            BackColor = System.Drawing.Color.FromArgb(70, 130, 220),
            ForeColor = System.Drawing.Color.White,
            FlatStyle = FlatStyle.Flat,
            Enabled = false
        };
        btnMergePreview.FlatAppearance.BorderSize = 0;

        btnSavePdf = new Button
        {
            Text = "Save PDF",
            Width = 100,
            Height = 32,
            Margin = new Padding(5, 3, 5, 0),
            BackColor = System.Drawing.Color.FromArgb(60, 170, 80),
            ForeColor = System.Drawing.Color.White,
            FlatStyle = FlatStyle.Flat,
            Enabled = false
        };
        btnSavePdf.FlatAppearance.BorderSize = 0;

        toolbarRight.Controls.AddRange(new Control[]
        {
            lblPageSizeLabel, cmbPageSize,
            lblOrientationLabel, cmbOrientation,
            lblScaleModeLabel, cmbScaleMode,
            chkMargin,
            btnMergePreview, btnSavePdf
        });
        splitMain.Panel2.Controls.Add(toolbarRight);

        // --- Navigation Panel (Bottom, above toolbar) ---
        navPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 40
        };

        btnPrevPage = new Button
        {
            Text = "<",
            Width = 45,
            Height = 30,
            Location = new System.Drawing.Point(10, 5),
            Enabled = false
        };

        lblPageInfo = new Label
        {
            Text = "Page 0 / 0",
            AutoSize = true,
            Location = new System.Drawing.Point(65, 10),
            Font = new System.Drawing.Font("Segoe UI", 10f)
        };

        btnNextPage = new Button
        {
            Text = ">",
            Width = 45,
            Height = 30,
            Location = new System.Drawing.Point(170, 5),
            Enabled = false
        };

        navPanel.Controls.AddRange(new Control[] { btnPrevPage, lblPageInfo, btnNextPage });
        splitMain.Panel2.Controls.Add(navPanel);

        // --- PictureBox Preview (Fill) ---
        picPreview = new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = System.Drawing.Color.FromArgb(240, 240, 240),
            BorderStyle = BorderStyle.FixedSingle
        };
        splitMain.Panel2.Controls.Add(picPreview);

        // Fix dock order for Panel2
        splitMain.Panel2.Controls.SetChildIndex(toolbarRight, 2);
        splitMain.Panel2.Controls.SetChildIndex(navPanel, 1);
        splitMain.Panel2.Controls.SetChildIndex(picPreview, 0);
    }

    private Button CreateToolbarButton(string text, string tooltip)
    {
        var btn = new Button
        {
            Text = text,
            Width = 75,
            Height = 30,
            Margin = new Padding(3),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(200, 200, 200);

        var tt = new ToolTip();
        tt.SetToolTip(btn, tooltip);
        return btn;
    }
}
