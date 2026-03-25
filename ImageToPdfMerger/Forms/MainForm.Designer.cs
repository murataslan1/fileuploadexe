using ImageToPdfMerger.Controls;

namespace ImageToPdfMerger.Forms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    private SplitContainer splitMain;
    private DropZonePanel dropZone;
    private FlowLayoutPanel tabBar;
    private Panel searchPanel;
    private TextBox txtSearch;
    private Button btnClearSearch;
    private ThumbnailListView listViewImages;
    private FlowLayoutPanel toolbarLeft;
    private Button btnDelete;
    private Button btnMoveUp;
    private Button btnMoveDown;
    private Button btnMoveToGroup;
    private Button btnClearAll;

    private PictureBox picPreview;
    private Panel imageInfoPanel;
    private Label lblImageInfo;
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

    // Color palette
    private static readonly System.Drawing.Color BgMain = System.Drawing.Color.FromArgb(245, 246, 250);
    private static readonly System.Drawing.Color BgPanel = System.Drawing.Color.White;
    private static readonly System.Drawing.Color BgPreview = System.Drawing.Color.FromArgb(248, 249, 252);
    private static readonly System.Drawing.Color BgToolbar = System.Drawing.Color.FromArgb(240, 242, 248);
    private static readonly System.Drawing.Color AccentBlue = System.Drawing.Color.FromArgb(41, 121, 255);
    private static readonly System.Drawing.Color AccentGreen = System.Drawing.Color.FromArgb(67, 160, 71);
    private static readonly System.Drawing.Color TextDark = System.Drawing.Color.FromArgb(40, 50, 70);
    private static readonly System.Drawing.Color TextMuted = System.Drawing.Color.FromArgb(130, 140, 160);
    private static readonly System.Drawing.Color BorderLight = System.Drawing.Color.FromArgb(215, 220, 232);

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        AutoScaleMode = AutoScaleMode.Font;
        Font = new System.Drawing.Font("Segoe UI", 9f);
        Text = "Image to PDF Merger";
        Size = new System.Drawing.Size(1200, 750);
        MinimumSize = new System.Drawing.Size(900, 600);
        StartPosition = FormStartPosition.CenterScreen;
        KeyPreview = true;
        BackColor = BgMain;

        // === StatusStrip ===
        statusStrip = new StatusStrip
        {
            BackColor = System.Drawing.Color.FromArgb(30, 40, 60),
            ForeColor = System.Drawing.Color.FromArgb(200, 210, 230),
            SizingGrip = false
        };
        lblStatus = new ToolStripStatusLabel("Ready -- Drop images to get started")
        {
            Spring = true,
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
            ForeColor = System.Drawing.Color.FromArgb(200, 210, 230),
            Font = new System.Drawing.Font("Segoe UI", 8.5f)
        };
        progressBar = new ToolStripProgressBar { Visible = false, Minimum = 0, Maximum = 100 };
        statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus, progressBar });
        Controls.Add(statusStrip);

        // === SplitContainer ===
        splitMain = new SplitContainer
        {
            Dock = DockStyle.Fill,
            FixedPanel = FixedPanel.Panel1,
            SplitterDistance = 380,
            SplitterWidth = 1,
            BackColor = BorderLight
        };
        splitMain.Panel1.BackColor = BgPanel;
        splitMain.Panel2.BackColor = BgPreview;
        Controls.Add(splitMain);

        // ==============================
        // LEFT PANEL
        // ==============================

        // --- DropZone (Top) ---
        dropZone = new DropZonePanel();
        splitMain.Panel1.Controls.Add(dropZone);

        // --- Tab Bar (Top, below DropZone) ---
        tabBar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 36,
            BackColor = BgToolbar,
            Padding = new Padding(6, 4, 6, 0),
            WrapContents = false,
            AutoScroll = true
        };
        splitMain.Panel1.Controls.Add(tabBar);

        // --- Search Panel (Top) ---
        searchPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 38,
            Padding = new Padding(8, 6, 8, 4),
            BackColor = BgPanel
        };

        btnClearSearch = new Button
        {
            Text = "X",
            Dock = DockStyle.Right,
            Width = 32,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            BackColor = BgPanel,
            ForeColor = TextMuted,
            Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold)
        };
        btnClearSearch.FlatAppearance.BorderSize = 0;

        txtSearch = new TextBox
        {
            Dock = DockStyle.Fill,
            Font = new System.Drawing.Font("Segoe UI", 10f),
            PlaceholderText = "Search images... (Ctrl+F)",
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = System.Drawing.Color.FromArgb(248, 249, 253)
        };

        searchPanel.Controls.Add(txtSearch);
        searchPanel.Controls.Add(btnClearSearch);
        splitMain.Panel1.Controls.Add(searchPanel);

        // --- Toolbar Left (Bottom) ---
        toolbarLeft = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 48,
            Padding = new Padding(4, 8, 4, 6),
            FlowDirection = FlowDirection.LeftToRight,
            BackColor = BgToolbar
        };

        btnDelete = CreateStyledButton("Delete", TextDark, BgPanel, BorderLight, 65);
        btnMoveUp = CreateStyledButton("Up", TextDark, BgPanel, BorderLight, 45);
        btnMoveDown = CreateStyledButton("Down", TextDark, BgPanel, BorderLight, 55);
        btnMoveToGroup = CreateStyledButton("Move to...", AccentBlue, BgPanel, AccentBlue, 80);
        btnClearAll = CreateStyledButton("Clear", System.Drawing.Color.FromArgb(180, 60, 60), BgPanel, System.Drawing.Color.FromArgb(230, 180, 180), 55);

        toolbarLeft.Controls.AddRange(new Control[] { btnDelete, btnMoveUp, btnMoveDown, btnMoveToGroup, btnClearAll });
        splitMain.Panel1.Controls.Add(toolbarLeft);

        // --- ListView (Fill) ---
        listViewImages = new ThumbnailListView
        {
            Dock = DockStyle.Fill,
            BackColor = BgPanel,
            ForeColor = TextDark,
            Font = new System.Drawing.Font("Segoe UI", 9f),
            BorderStyle = BorderStyle.None
        };
        splitMain.Panel1.Controls.Add(listViewImages);

        // Dock order for left panel
        splitMain.Panel1.Controls.SetChildIndex(dropZone, 4);
        splitMain.Panel1.Controls.SetChildIndex(tabBar, 3);
        splitMain.Panel1.Controls.SetChildIndex(searchPanel, 2);
        splitMain.Panel1.Controls.SetChildIndex(toolbarLeft, 1);
        splitMain.Panel1.Controls.SetChildIndex(listViewImages, 0);

        // ==============================
        // RIGHT PANEL
        // ==============================

        // --- Bottom Toolbar ---
        toolbarRight = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 52,
            Padding = new Padding(12, 10, 12, 8),
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = BgToolbar
        };

        lblPageSizeLabel = new Label { Text = "Page:", AutoSize = true, ForeColor = TextMuted, Font = new System.Drawing.Font("Segoe UI", 8.5f), Margin = new Padding(0, 7, 2, 0) };
        cmbPageSize = CreateStyledComboBox(80, new object[] { "A4", "Letter", "A3", "Original" });
        lblOrientationLabel = new Label { Text = "Orient:", AutoSize = true, ForeColor = TextMuted, Font = new System.Drawing.Font("Segoe UI", 8.5f), Margin = new Padding(8, 7, 2, 0) };
        cmbOrientation = CreateStyledComboBox(90, new object[] { "Portrait", "Landscape" });
        lblScaleModeLabel = new Label { Text = "Scale:", AutoSize = true, ForeColor = TextMuted, Font = new System.Drawing.Font("Segoe UI", 8.5f), Margin = new Padding(8, 7, 2, 0) };
        cmbScaleMode = CreateStyledComboBox(105, new object[] { "Fit to Page", "Original Size", "Fit to Width" });
        chkMargin = new CheckBox { Text = "Margin", Checked = true, AutoSize = true, ForeColor = TextDark, Font = new System.Drawing.Font("Segoe UI", 8.5f), Margin = new Padding(12, 6, 8, 0) };

        btnMergePreview = new Button
        {
            Text = "Create PDF",
            Width = 110,
            Height = 34,
            Margin = new Padding(12, 0, 4, 0),
            BackColor = AccentBlue,
            ForeColor = System.Drawing.Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new System.Drawing.Font("Segoe UI Semibold", 9f, System.Drawing.FontStyle.Bold),
            Cursor = Cursors.Hand,
            Enabled = false
        };
        btnMergePreview.FlatAppearance.BorderSize = 0;

        btnSavePdf = new Button
        {
            Text = "Save PDF",
            Width = 90,
            Height = 34,
            Margin = new Padding(4, 0, 0, 0),
            BackColor = AccentGreen,
            ForeColor = System.Drawing.Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new System.Drawing.Font("Segoe UI Semibold", 9f, System.Drawing.FontStyle.Bold),
            Cursor = Cursors.Hand,
            Enabled = false
        };
        btnSavePdf.FlatAppearance.BorderSize = 0;

        toolbarRight.Controls.AddRange(new Control[] { lblPageSizeLabel, cmbPageSize, lblOrientationLabel, cmbOrientation, lblScaleModeLabel, cmbScaleMode, chkMargin, btnMergePreview, btnSavePdf });
        splitMain.Panel2.Controls.Add(toolbarRight);

        // --- Image Info Panel ---
        imageInfoPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 28,
            BackColor = System.Drawing.Color.FromArgb(240, 242, 248),
            Padding = new Padding(12, 4, 12, 4)
        };
        lblImageInfo = new Label
        {
            Text = "Select an image to preview",
            Dock = DockStyle.Fill,
            ForeColor = TextMuted,
            Font = new System.Drawing.Font("Segoe UI", 8.5f),
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        };
        imageInfoPanel.Controls.Add(lblImageInfo);
        splitMain.Panel2.Controls.Add(imageInfoPanel);

        // --- Navigation Panel ---
        navPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 40,
            BackColor = BgPreview,
            Visible = false
        };

        btnPrevPage = new Button { Text = "\u25C0", Width = 40, Height = 28, FlatStyle = FlatStyle.Flat, ForeColor = AccentBlue, BackColor = BgPreview, Font = new System.Drawing.Font("Segoe UI", 10f), Cursor = Cursors.Hand, Enabled = false };
        btnPrevPage.FlatAppearance.BorderColor = BorderLight;
        lblPageInfo = new Label { Text = "Page 0 / 0", AutoSize = true, ForeColor = TextDark, Font = new System.Drawing.Font("Segoe UI Semibold", 10f) };
        btnNextPage = new Button { Text = "\u25B6", Width = 40, Height = 28, FlatStyle = FlatStyle.Flat, ForeColor = AccentBlue, BackColor = BgPreview, Font = new System.Drawing.Font("Segoe UI", 10f), Cursor = Cursors.Hand, Enabled = false };
        btnNextPage.FlatAppearance.BorderColor = BorderLight;

        var navFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Padding = new Padding(0, 5, 0, 0) };
        navFlow.Controls.AddRange(new Control[] { btnPrevPage, lblPageInfo, btnNextPage });
        lblPageInfo.Margin = new Padding(12, 5, 12, 0);
        navPanel.Controls.Add(navFlow);
        navPanel.Resize += (s, e) =>
        {
            int totalW = btnPrevPage.Width + lblPageInfo.PreferredWidth + btnNextPage.Width + 48;
            navFlow.Padding = new Padding(Math.Max(0, (navPanel.Width - totalW) / 2), 5, 0, 0);
        };
        splitMain.Panel2.Controls.Add(navPanel);

        // --- PictureBox Preview (Fill) ---
        picPreview = new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = BgPreview,
            BorderStyle = BorderStyle.None,
            Padding = new Padding(12)
        };
        splitMain.Panel2.Controls.Add(picPreview);

        // Dock order for right panel
        splitMain.Panel2.Controls.SetChildIndex(toolbarRight, 3);
        splitMain.Panel2.Controls.SetChildIndex(imageInfoPanel, 2);
        splitMain.Panel2.Controls.SetChildIndex(navPanel, 1);
        splitMain.Panel2.Controls.SetChildIndex(picPreview, 0);
    }

    private Button CreateStyledButton(string text, System.Drawing.Color foreColor, System.Drawing.Color backColor, System.Drawing.Color borderColor, int width = 78)
    {
        var btn = new Button
        {
            Text = text,
            Width = width,
            Height = 30,
            Margin = new Padding(2),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            ForeColor = foreColor,
            BackColor = backColor,
            Font = new System.Drawing.Font("Segoe UI", 8.5f)
        };
        btn.FlatAppearance.BorderColor = borderColor;
        btn.FlatAppearance.BorderSize = 1;
        return btn;
    }

    private ComboBox CreateStyledComboBox(int width, object[] items)
    {
        var cmb = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = width,
            Font = new System.Drawing.Font("Segoe UI", 8.5f),
            Margin = new Padding(2, 3, 0, 0),
            BackColor = BgPanel
        };
        cmb.Items.AddRange(items);
        cmb.SelectedIndex = 0;
        return cmb;
    }
}
