namespace ImageToPdfMerger.Forms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    private SplitContainer splitMain;
    private Controls.DropZonePanel dropZone;
    private Panel searchPanel;
    private TextBox txtSearch;
    private Button btnClearSearch;
    private TreeView treeImages;
    private FlowLayoutPanel toolbarLeft;
    private Button btnDelete;
    private Button btnMoveUp;
    private Button btnMoveDown;
    private Button btnAddGroup;
    private Button btnSelectAll;
    private Button btnSelectNone;
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
    private Button btnSaveAll;

    private StatusStrip statusStrip;
    private ToolStripStatusLabel lblStatus;
    private ToolStripProgressBar progressBar;

    private ImageList treeImageList;

    // Colors
    private static readonly System.Drawing.Color BgMain = System.Drawing.Color.FromArgb(245, 246, 250);
    private static readonly System.Drawing.Color BgPanel = System.Drawing.Color.White;
    private static readonly System.Drawing.Color BgPreview = System.Drawing.Color.FromArgb(248, 249, 252);
    private static readonly System.Drawing.Color BgToolbar = System.Drawing.Color.FromArgb(240, 242, 248);
    private static readonly System.Drawing.Color AccentBlue = System.Drawing.Color.FromArgb(41, 121, 255);
    private static readonly System.Drawing.Color AccentGreen = System.Drawing.Color.FromArgb(67, 160, 71);
    private static readonly System.Drawing.Color AccentOrange = System.Drawing.Color.FromArgb(245, 150, 30);
    private static readonly System.Drawing.Color TextDark = System.Drawing.Color.FromArgb(40, 50, 70);
    private static readonly System.Drawing.Color TextMuted = System.Drawing.Color.FromArgb(130, 140, 160);
    private static readonly System.Drawing.Color BorderLight = System.Drawing.Color.FromArgb(215, 220, 232);

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
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

        // Tree icons
        treeImageList = new ImageList { ImageSize = new System.Drawing.Size(18, 18), ColorDepth = ColorDepth.Depth32Bit };

        // === StatusStrip ===
        statusStrip = new StatusStrip
        {
            BackColor = System.Drawing.Color.FromArgb(30, 40, 60),
            SizingGrip = false
        };
        lblStatus = new ToolStripStatusLabel("Ready -- Drop images to get started")
        {
            Spring = true, TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
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
            SplitterDistance = 340,
            SplitterWidth = 1,
            BackColor = BorderLight
        };
        splitMain.Panel1.BackColor = BgPanel;
        splitMain.Panel2.BackColor = BgPreview;
        Controls.Add(splitMain);

        // ==============================
        // LEFT PANEL -- correct dock order
        // In WinForms: add Bottom first, then Top, then Fill last
        // ==============================

        // 1) Toolbar Left (BOTTOM) -- add FIRST so it docks correctly
        toolbarLeft = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 46,
            Padding = new Padding(3, 7, 3, 5),
            FlowDirection = FlowDirection.LeftToRight,
            BackColor = BgToolbar,
            AutoSize = false
        };

        btnDelete = CreateBtn("Del", TextDark, 38);
        btnMoveUp = CreateBtn("\u25B2", TextDark, 30);
        btnMoveDown = CreateBtn("\u25BC", TextDark, 30);
        btnAddGroup = CreateBtn("+Grp", AccentBlue, 44);
        btnSelectAll = CreateBtn("All", System.Drawing.Color.FromArgb(50, 120, 50), 34);
        btnSelectNone = CreateBtn("None", TextMuted, 42);
        btnClearAll = CreateBtn("Clr", System.Drawing.Color.FromArgb(180, 60, 60), 34);

        toolbarLeft.Controls.AddRange(new Control[] { btnDelete, btnMoveUp, btnMoveDown, btnAddGroup, btnSelectAll, btnSelectNone, btnClearAll });
        splitMain.Panel1.Controls.Add(toolbarLeft);

        // 2) DropZone (TOP)
        dropZone = new Controls.DropZonePanel();
        splitMain.Panel1.Controls.Add(dropZone);

        // 3) Search Panel (TOP, below DropZone)
        searchPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 34,
            Padding = new Padding(6, 4, 6, 2),
            BackColor = BgPanel
        };
        btnClearSearch = new Button
        {
            Text = "X", Dock = DockStyle.Right, Width = 26,
            FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand,
            BackColor = BgPanel, ForeColor = TextMuted,
            Font = new System.Drawing.Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold)
        };
        btnClearSearch.FlatAppearance.BorderSize = 0;
        txtSearch = new TextBox
        {
            Dock = DockStyle.Fill,
            Font = new System.Drawing.Font("Segoe UI", 9.5f),
            PlaceholderText = "Search... (Ctrl+F)",
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = System.Drawing.Color.FromArgb(248, 249, 253)
        };
        searchPanel.Controls.Add(txtSearch);
        searchPanel.Controls.Add(btnClearSearch);
        splitMain.Panel1.Controls.Add(searchPanel);

        // 4) TreeView (FILL) -- add LAST
        treeImages = new TreeView
        {
            Dock = DockStyle.Fill,
            CheckBoxes = true,
            AllowDrop = true,
            HideSelection = false,
            ShowLines = true,
            ShowPlusMinus = true,
            ShowRootLines = true,
            LabelEdit = true,
            FullRowSelect = true,
            Font = new System.Drawing.Font("Segoe UI", 9.5f),
            BackColor = BgPanel,
            ForeColor = TextDark,
            BorderStyle = BorderStyle.None,
            Indent = 22,
            ItemHeight = 26,
            ImageList = treeImageList
        };
        splitMain.Panel1.Controls.Add(treeImages);

        // ==============================
        // RIGHT PANEL
        // ==============================

        // 1) Bottom Toolbar (BOTTOM) -- add first
        toolbarRight = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom, Height = 46,
            Padding = new Padding(8, 7, 8, 5),
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false, BackColor = BgToolbar
        };

        lblPageSizeLabel = new Label { Text = "Page:", AutoSize = true, ForeColor = TextMuted, Font = new System.Drawing.Font("Segoe UI", 8f), Margin = new Padding(0, 6, 1, 0) };
        cmbPageSize = CreateCmb(70, new object[] { "A4", "Letter", "A3", "Original" });
        lblOrientationLabel = new Label { Text = "Or:", AutoSize = true, ForeColor = TextMuted, Font = new System.Drawing.Font("Segoe UI", 8f), Margin = new Padding(4, 6, 1, 0) };
        cmbOrientation = CreateCmb(80, new object[] { "Portrait", "Landscape" });
        lblScaleModeLabel = new Label { Text = "Sc:", AutoSize = true, ForeColor = TextMuted, Font = new System.Drawing.Font("Segoe UI", 8f), Margin = new Padding(4, 6, 1, 0) };
        cmbScaleMode = CreateCmb(90, new object[] { "Fit to Page", "Original", "Fit Width" });
        chkMargin = new CheckBox { Text = "Margin", Checked = true, AutoSize = true, ForeColor = TextDark, Font = new System.Drawing.Font("Segoe UI", 8f), Margin = new Padding(6, 4, 4, 0) };

        btnMergePreview = new Button
        {
            Text = "Create PDF", Width = 85, Height = 30,
            Margin = new Padding(6, 0, 2, 0),
            BackColor = AccentBlue, ForeColor = System.Drawing.Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new System.Drawing.Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold),
            Cursor = Cursors.Hand, Enabled = false
        };
        btnMergePreview.FlatAppearance.BorderSize = 0;

        btnSavePdf = new Button
        {
            Text = "Save", Width = 55, Height = 30,
            Margin = new Padding(2, 0, 2, 0),
            BackColor = AccentGreen, ForeColor = System.Drawing.Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new System.Drawing.Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold),
            Cursor = Cursors.Hand, Enabled = false
        };
        btnSavePdf.FlatAppearance.BorderSize = 0;

        btnSaveAll = new Button
        {
            Text = "Save All", Width = 65, Height = 30,
            Margin = new Padding(2, 0, 0, 0),
            BackColor = AccentOrange, ForeColor = System.Drawing.Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new System.Drawing.Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold),
            Cursor = Cursors.Hand, Enabled = false
        };
        btnSaveAll.FlatAppearance.BorderSize = 0;

        toolbarRight.Controls.AddRange(new Control[] { lblPageSizeLabel, cmbPageSize, lblOrientationLabel, cmbOrientation, lblScaleModeLabel, cmbScaleMode, chkMargin, btnMergePreview, btnSavePdf, btnSaveAll });
        splitMain.Panel2.Controls.Add(toolbarRight);

        // 2) Image Info Panel
        imageInfoPanel = new Panel { Dock = DockStyle.Bottom, Height = 24, BackColor = BgToolbar, Padding = new Padding(10, 2, 10, 2) };
        lblImageInfo = new Label { Text = "Select an image to preview", Dock = DockStyle.Fill, ForeColor = TextMuted, Font = new System.Drawing.Font("Segoe UI", 8f), TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
        imageInfoPanel.Controls.Add(lblImageInfo);
        splitMain.Panel2.Controls.Add(imageInfoPanel);

        // 3) Nav Panel
        navPanel = new Panel { Dock = DockStyle.Bottom, Height = 36, BackColor = BgPreview, Visible = false };
        btnPrevPage = new Button { Text = "\u25C0", Width = 34, Height = 24, FlatStyle = FlatStyle.Flat, ForeColor = AccentBlue, BackColor = BgPreview, Cursor = Cursors.Hand, Enabled = false };
        btnPrevPage.FlatAppearance.BorderColor = BorderLight;
        lblPageInfo = new Label { Text = "Page 0 / 0", AutoSize = true, ForeColor = TextDark, Font = new System.Drawing.Font("Segoe UI Semibold", 9f) };
        btnNextPage = new Button { Text = "\u25B6", Width = 34, Height = 24, FlatStyle = FlatStyle.Flat, ForeColor = AccentBlue, BackColor = BgPreview, Cursor = Cursors.Hand, Enabled = false };
        btnNextPage.FlatAppearance.BorderColor = BorderLight;

        var navFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Padding = new Padding(0, 5, 0, 0) };
        navFlow.Controls.AddRange(new Control[] { btnPrevPage, lblPageInfo, btnNextPage });
        lblPageInfo.Margin = new Padding(8, 3, 8, 0);
        navPanel.Controls.Add(navFlow);
        navPanel.Resize += (s, e) =>
        {
            int totalW = btnPrevPage.Width + lblPageInfo.PreferredWidth + btnNextPage.Width + 32;
            navFlow.Padding = new Padding(Math.Max(0, (navPanel.Width - totalW) / 2), 5, 0, 0);
        };
        splitMain.Panel2.Controls.Add(navPanel);

        // 4) PictureBox (FILL) -- add last
        picPreview = new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = BgPreview,
            BorderStyle = BorderStyle.None
        };
        splitMain.Panel2.Controls.Add(picPreview);
    }

    private Button CreateBtn(string text, System.Drawing.Color foreColor, int width)
    {
        var btn = new Button
        {
            Text = text, Width = width, Height = 28, Margin = new Padding(1),
            FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand,
            ForeColor = foreColor, BackColor = BgPanel,
            Font = new System.Drawing.Font("Segoe UI", 8f)
        };
        btn.FlatAppearance.BorderColor = BorderLight;
        btn.FlatAppearance.BorderSize = 1;
        return btn;
    }

    private ComboBox CreateCmb(int width, object[] items)
    {
        var cmb = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList, Width = width,
            Font = new System.Drawing.Font("Segoe UI", 8f),
            Margin = new Padding(1, 2, 0, 0), BackColor = BgPanel
        };
        cmb.Items.AddRange(items);
        cmb.SelectedIndex = 0;
        return cmb;
    }
}
