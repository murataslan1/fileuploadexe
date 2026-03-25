using System.Drawing;
using ImageToPdfMerger.Models;
using ImageToPdfMerger.Services;
using ImageToPdfMerger.Utils;

namespace ImageToPdfMerger.Forms;

public partial class MainForm : Form
{
    private readonly ImageService _imageService = new();
    private readonly PdfService _pdfService = new();
    private readonly PreviewService _previewService = new();

    private readonly List<ImageItem> _allImages = new();
    private readonly List<ImageItem> _filteredImages = new();
    private readonly AutoCompleteStringCollection _autoCompleteSource = new();

    private readonly List<string> _groups = new();
    private string _activeTab = "all";
    private bool _isPdfPreviewMode;

    private byte[]? _currentPdfBytes;
    private List<Image>? _previewPages;
    private int _currentPageIndex;
    private Image? _currentPreviewImage;

    public MainForm()
    {
        InitializeComponent();
        SetupSearchAutoComplete();
        RebuildTabBar();
        WireEvents();
    }

    // ===========================
    // TAB / GROUP SYSTEM
    // ===========================

    private void RebuildTabBar()
    {
        tabBar.SuspendLayout();
        tabBar.Controls.Clear();

        // "All" tab
        var allCount = _allImages.Count;
        AddTabButton($"All ({allCount})", "all");

        // Group tabs
        foreach (var group in _groups)
        {
            var count = _allImages.Count(i => i.GroupName == group);
            AddTabButton($"{group} ({count})", group);
        }

        // "+" button
        var addBtn = new Button
        {
            Text = "+",
            Width = 30,
            Height = 26,
            Margin = new Padding(4, 1, 0, 0),
            FlatStyle = FlatStyle.Flat,
            BackColor = System.Drawing.Color.FromArgb(230, 235, 245),
            ForeColor = AccentBlue,
            Font = new Font("Segoe UI", 11f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        addBtn.FlatAppearance.BorderSize = 0;
        addBtn.Click += OnAddGroupClick;
        tabBar.Controls.Add(addBtn);

        tabBar.ResumeLayout();
    }

    private void AddTabButton(string text, string tabId)
    {
        bool isActive = tabId == _activeTab;
        var btn = new Button
        {
            Text = text,
            AutoSize = true,
            Height = 26,
            Margin = new Padding(2, 1, 2, 0),
            Padding = new Padding(8, 0, 8, 0),
            FlatStyle = FlatStyle.Flat,
            BackColor = isActive ? AccentBlue : System.Drawing.Color.FromArgb(225, 230, 240),
            ForeColor = isActive ? Color.White : TextDark,
            Font = new Font("Segoe UI", 8.5f, isActive ? FontStyle.Bold : FontStyle.Regular),
            Cursor = Cursors.Hand,
            Tag = tabId
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.Click += (s, e) => SwitchToTab((string)((Button)s!).Tag!);
        tabBar.Controls.Add(btn);
    }

    private void SwitchToTab(string tabId)
    {
        _activeTab = tabId;
        _isPdfPreviewMode = false;
        navPanel.Visible = false;
        RebuildTabBar();
        ApplyFilter(txtSearch.Text.Trim());
        UpdateMergeButtonState();
    }

    private void OnAddGroupClick(object? sender, EventArgs e)
    {
        var name = PromptGroupName("New Group", $"Group {_groups.Count + 1}");
        if (name == null) return;

        if (_groups.Contains(name))
        {
            MessageBox.Show($"Group \"{name}\" already exists.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _groups.Add(name);
        SwitchToTab(name);
    }

    private string? PromptGroupName(string title, string defaultValue)
    {
        using var form = new Form
        {
            Text = title,
            Size = new Size(350, 150),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false
        };

        var lbl = new Label { Text = "Group name:", Location = new Point(15, 18), AutoSize = true };
        var txt = new TextBox { Text = defaultValue, Location = new Point(15, 42), Width = 300 };
        txt.SelectAll();
        var btnOk = new Button { Text = "OK", DialogResult = DialogResult.OK, Location = new Point(155, 75), Width = 75 };
        var btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(240, 75), Width = 75 };

        form.Controls.AddRange(new Control[] { lbl, txt, btnOk, btnCancel });
        form.AcceptButton = btnOk;
        form.CancelButton = btnCancel;

        return form.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(txt.Text) ? txt.Text.Trim() : null;
    }

    // ===========================
    // MOVE TO GROUP
    // ===========================

    private void OnMoveToGroupClick(object? sender, EventArgs e)
    {
        if (listViewImages.SelectedItems.Count == 0)
        {
            MessageBox.Show("Select images first.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var menu = new ContextMenuStrip();

        foreach (var group in _groups)
        {
            var g = group;
            menu.Items.Add(group, null, (s, ev) => MoveSelectedToGroup(g));
        }

        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("+ New Group...", null, (s, ev) =>
        {
            var name = PromptGroupName("New Group", $"Group {_groups.Count + 1}");
            if (name == null) return;
            if (!_groups.Contains(name)) _groups.Add(name);
            MoveSelectedToGroup(name);
        });

        menu.Show(btnMoveToGroup, new Point(0, btnMoveToGroup.Height));
    }

    private void MoveSelectedToGroup(string groupName)
    {
        var selectedItems = listViewImages.SelectedItems.Cast<ListViewItem>().ToList();
        foreach (var lvi in selectedItems)
        {
            if (lvi.Tag is ImageItem item)
                item.GroupName = groupName;
        }

        RebuildTabBar();
        ApplyFilter(txtSearch.Text.Trim());
        UpdateStatus();
    }

    // ===========================
    // SEARCH & AUTOCOMPLETE
    // ===========================

    private void SetupSearchAutoComplete()
    {
        txtSearch.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        txtSearch.AutoCompleteSource = AutoCompleteSource.CustomSource;
        txtSearch.AutoCompleteCustomSource = _autoCompleteSource;
    }

    private void RefreshAutoCompleteSource()
    {
        _autoCompleteSource.Clear();
        _autoCompleteSource.AddRange(_allImages.Select(i => i.FileName).ToArray());
    }

    private void ApplyFilter(string query)
    {
        _filteredImages.Clear();

        // Step 1: filter by active tab
        IEnumerable<ImageItem> source;
        if (_activeTab == "all")
            source = _allImages;
        else
            source = _allImages.Where(i => i.GroupName == _activeTab);

        // Step 2: filter by search query
        if (!string.IsNullOrWhiteSpace(query))
            source = source.Where(i => i.FileName.Contains(query, StringComparison.OrdinalIgnoreCase));

        _filteredImages.AddRange(source);
        listViewImages.RefreshFromList(_filteredImages);

        if (_filteredImages.Count == 1)
            listViewImages.ScrollToAndHighlight(0);

        UpdateMergeButtonState();
    }

    private void OnSearchTextChanged(object? sender, EventArgs e)
    {
        ApplyFilter(txtSearch.Text.Trim());
    }

    private void OnClearSearchClick(object? sender, EventArgs e)
    {
        txtSearch.Clear();
        txtSearch.Focus();
    }

    // ===========================
    // IMAGE PREVIEW (single image)
    // ===========================

    private void OnImageSelected(object? sender, EventArgs e)
    {
        if (_isPdfPreviewMode) return;

        if (listViewImages.SelectedItems.Count == 0)
        {
            picPreview.Image = null;
            lblImageInfo.Text = "Select an image to preview";
            return;
        }

        var lvi = listViewImages.SelectedItems[0];
        if (lvi.Tag is not ImageItem item) return;

        try
        {
            _currentPreviewImage?.Dispose();
            _currentPreviewImage = Image.FromFile(item.DrawablePath);
            picPreview.Image = _currentPreviewImage;
            lblImageInfo.Text = $"{item.FileName}  |  {item.FormattedSize}  |  {item.Resolution}";
        }
        catch
        {
            picPreview.Image = null;
            lblImageInfo.Text = "Preview unavailable";
        }
    }

    // ===========================
    // EVENT WIRING
    // ===========================

    private void WireEvents()
    {
        dropZone.FilesDropped += OnFilesDropped;

        txtSearch.TextChanged += OnSearchTextChanged;
        btnClearSearch.Click += OnClearSearchClick;

        btnDelete.Click += OnDeleteClick;
        btnMoveUp.Click += OnMoveUpClick;
        btnMoveDown.Click += OnMoveDownClick;
        btnMoveToGroup.Click += OnMoveToGroupClick;
        btnClearAll.Click += OnClearAllClick;

        btnMergePreview.Click += OnMergePreviewClick;
        btnSavePdf.Click += OnSavePdfClick;

        btnPrevPage.Click += (s, e) => ShowPreviewPage(_currentPageIndex - 1);
        btnNextPage.Click += (s, e) => ShowPreviewPage(_currentPageIndex + 1);

        listViewImages.SelectedIndexChanged += OnImageSelected;

        FormClosing += OnFormClosing;
    }

    // ===========================
    // IMAGE LOADING
    // ===========================

    private async void OnFilesDropped(object? sender, string[] filePaths)
    {
        btnMergePreview.Enabled = false;
        progressBar.Visible = true;
        progressBar.Value = 0;
        lblStatus.Text = "Loading images...";

        var progress = new Progress<int>(p =>
        {
            if (IsDisposed) return;
            progressBar.Value = p;
        });

        try
        {
            var newItems = await _imageService.LoadImagesAsync(filePaths, progress, CancellationToken.None);

            foreach (var item in newItems)
            {
                if (_allImages.Any(existing =>
                    string.Equals(existing.FilePath, item.FilePath, StringComparison.OrdinalIgnoreCase)))
                {
                    item.Dispose();
                    continue;
                }
                item.Order = _allImages.Count;

                // If on a group tab, assign new images to that group
                if (_activeTab != "all")
                    item.GroupName = _activeTab;

                _allImages.Add(item);
            }

            RefreshAutoCompleteSource();
            RebuildTabBar();
            ApplyFilter(txtSearch.Text.Trim());
            UpdateStatus();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading images:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            progressBar.Visible = false;
            UpdateMergeButtonState();
        }
    }

    // ===========================
    // TOOLBAR ACTIONS
    // ===========================

    private void OnDeleteClick(object? sender, EventArgs e)
    {
        var removedItems = listViewImages.GetRemovedSelectedItems();
        foreach (var item in removedItems)
        {
            _allImages.Remove(item);
            item.Dispose();
        }

        RefreshAutoCompleteSource();
        RebuildTabBar();
        ApplyFilter(txtSearch.Text.Trim());
        UpdateStatus();
        UpdateMergeButtonState();
    }

    private void OnMoveUpClick(object? sender, EventArgs e)
    {
        listViewImages.MoveSelectedUp();
        SyncOrderFromListView();
    }

    private void OnMoveDownClick(object? sender, EventArgs e)
    {
        listViewImages.MoveSelectedDown();
        SyncOrderFromListView();
    }

    private void OnClearAllClick(object? sender, EventArgs e)
    {
        if (_allImages.Count == 0) return;

        string msg = _activeTab == "all"
            ? "Clear ALL images?"
            : $"Clear images in \"{_activeTab}\"?";

        if (MessageBox.Show(msg, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        if (_activeTab == "all")
        {
            foreach (var item in _allImages) item.Dispose();
            _allImages.Clear();
        }
        else
        {
            var toRemove = _allImages.Where(i => i.GroupName == _activeTab).ToList();
            foreach (var item in toRemove)
            {
                _allImages.Remove(item);
                item.Dispose();
            }
        }

        txtSearch.Clear();
        RefreshAutoCompleteSource();
        RebuildTabBar();
        ApplyFilter(string.Empty);
        ClearPreview();
        UpdateStatus();
        UpdateMergeButtonState();
    }

    private void SyncOrderFromListView()
    {
        var ordered = listViewImages.GetOrderedItems();
        foreach (var item in ordered)
        {
            var match = _allImages.FirstOrDefault(i => i.FilePath == item.FilePath);
            if (match != null) match.Order = item.Order;
        }
    }

    // ===========================
    // PDF MERGE & PREVIEW
    // ===========================

    private async void OnMergePreviewClick(object? sender, EventArgs e)
    {
        var settings = GetCurrentPdfSettings();

        // Get images from current view (active tab + checked)
        var selectedImages = _filteredImages.Where(i => i.IsSelected).OrderBy(i => i.Order).ToList();

        if (selectedImages.Count == 0)
        {
            MessageBox.Show("No images selected. Check the images you want to include.",
                "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        btnMergePreview.Enabled = false;
        progressBar.Visible = true;
        progressBar.Value = 0;
        lblStatus.Text = "Generating PDF...";

        try
        {
            var (pdfBytes, pdfErrors) = await Task.Run(() => _pdfService.MergeImagesToPdf(selectedImages, settings));
            _currentPdfBytes = pdfBytes;

            if (pdfErrors.Count > 0)
            {
                MessageBox.Show($"Some images could not be added:\n\n{string.Join("\n", pdfErrors)}",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            progressBar.Value = 50;
            lblStatus.Text = "Rendering preview...";

            _previewPages?.ForEach(p => p.Dispose());
            _previewPages = new List<Image>();

            int previewWidth = Math.Max(picPreview.Width - 20, 200);
            foreach (var img in selectedImages)
            {
                var preview = _previewService.RenderPagePreview(img, settings, previewWidth);
                _previewPages.Add(preview);
            }

            _isPdfPreviewMode = true;
            navPanel.Visible = true;
            _currentPageIndex = 0;
            ShowPreviewPage(0);
            btnSavePdf.Enabled = true;
            lblStatus.Text = $"PDF ready -- {selectedImages.Count} pages";
            lblImageInfo.Text = "PDF Preview mode (click an image in the list to return to image preview)";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"PDF generation error:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            lblStatus.Text = "Error generating PDF";
        }
        finally
        {
            progressBar.Visible = false;
            btnMergePreview.Enabled = _filteredImages.Any(i => i.IsSelected);
        }
    }

    private void ShowPreviewPage(int index)
    {
        if (_previewPages == null || _previewPages.Count == 0)
        {
            picPreview.Image = null;
            lblPageInfo.Text = "Page 0 / 0";
            btnPrevPage.Enabled = false;
            btnNextPage.Enabled = false;
            return;
        }

        index = Math.Clamp(index, 0, _previewPages.Count - 1);
        _currentPageIndex = index;
        picPreview.Image = _previewPages[index];
        lblPageInfo.Text = $"Page {index + 1} / {_previewPages.Count}";
        btnPrevPage.Enabled = index > 0;
        btnNextPage.Enabled = index < _previewPages.Count - 1;
    }

    private void ClearPreview()
    {
        _isPdfPreviewMode = false;
        navPanel.Visible = false;
        _previewPages?.ForEach(p => p.Dispose());
        _previewPages = null;
        _currentPdfBytes = null;
        _currentPreviewImage?.Dispose();
        _currentPreviewImage = null;
        picPreview.Image = null;
        lblPageInfo.Text = "Page 0 / 0";
        btnPrevPage.Enabled = false;
        btnNextPage.Enabled = false;
        btnSavePdf.Enabled = false;
        lblImageInfo.Text = "Select an image to preview";
    }

    // ===========================
    // SAVE PDF
    // ===========================

    private void OnSavePdfClick(object? sender, EventArgs e)
    {
        if (_currentPdfBytes == null) return;

        string defaultName = _activeTab != "all" ? $"{_activeTab}.pdf" : "merged_images.pdf";

        using var dialog = new SaveFileDialog
        {
            Title = "Save PDF",
            Filter = "PDF Files|*.pdf",
            DefaultExt = "pdf",
            FileName = defaultName
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                _pdfService.SavePdf(_currentPdfBytes, dialog.FileName);
                lblStatus.Text = $"Saved: {dialog.FileName}";
                MessageBox.Show($"PDF saved!\n{dialog.FileName}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving PDF:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // ===========================
    // SETTINGS HELPERS
    // ===========================

    private PdfSettings GetCurrentPdfSettings()
    {
        return new PdfSettings
        {
            PageSize = cmbPageSize.SelectedIndex switch { 0 => PageSizeOption.A4, 1 => PageSizeOption.Letter, 2 => PageSizeOption.A3, 3 => PageSizeOption.Original, _ => PageSizeOption.A4 },
            Orientation = cmbOrientation.SelectedIndex switch { 0 => PageOrientationOption.Portrait, 1 => PageOrientationOption.Landscape, _ => PageOrientationOption.Portrait },
            ScaleMode = cmbScaleMode.SelectedIndex switch { 0 => ScaleMode.FitToPage, 1 => ScaleMode.OriginalSize, 2 => ScaleMode.FitToWidth, _ => ScaleMode.FitToPage },
            MarginMm = chkMargin.Checked ? 10 : 0,
            ImageQuality = 90
        };
    }

    // ===========================
    // UI HELPERS
    // ===========================

    private void UpdateStatus()
    {
        if (_allImages.Count == 0)
        {
            lblStatus.Text = "Ready -- Drop images to get started";
            return;
        }

        long totalSize = _allImages.Sum(i => i.FileSize);
        int total = _allImages.Count;
        int groupCount = _groups.Count;
        string groupInfo = groupCount > 0 ? $" | {groupCount} groups" : "";
        lblStatus.Text = $"{total} images loaded{groupInfo} | Total: {FileHelper.FormatFileSize(totalSize)} | Ready";
    }

    private void UpdateMergeButtonState()
    {
        btnMergePreview.Enabled = _filteredImages.Count > 0 && _filteredImages.Any(i => i.IsSelected);
    }

    private void OpenFileDialogManual()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Select Images",
            Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tiff;*.heic;*.webp|All Files|*.*",
            Multiselect = true
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            var supported = FileHelper.FilterSupportedFiles(dialog.FileNames);
            if (supported.Length > 0)
                OnFilesDropped(this, supported);
        }
    }

    // ===========================
    // KEYBOARD SHORTCUTS
    // ===========================

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        switch (keyData)
        {
            case Keys.Control | Keys.F:
                txtSearch.Focus();
                txtSearch.SelectAll();
                return true;

            case Keys.Control | Keys.O:
                OpenFileDialogManual();
                return true;

            case Keys.Control | Keys.S:
                if (btnSavePdf.Enabled)
                    OnSavePdfClick(this, EventArgs.Empty);
                return true;

            case Keys.Control | Keys.A:
                if (listViewImages.Focused)
                {
                    foreach (ListViewItem item in listViewImages.Items)
                        item.Selected = true;
                    return true;
                }
                break;

            case Keys.Delete:
                if (listViewImages.Focused && listViewImages.SelectedItems.Count > 0)
                {
                    OnDeleteClick(this, EventArgs.Empty);
                    return true;
                }
                break;

            case Keys.Control | Keys.Up:
                OnMoveUpClick(this, EventArgs.Empty);
                return true;

            case Keys.Control | Keys.Down:
                OnMoveDownClick(this, EventArgs.Empty);
                return true;

            case Keys.F5:
                if (btnMergePreview.Enabled)
                    OnMergePreviewClick(this, EventArgs.Empty);
                return true;

            case Keys.Escape:
                if (_isPdfPreviewMode)
                {
                    _isPdfPreviewMode = false;
                    navPanel.Visible = false;
                    picPreview.Image = null;
                    lblImageInfo.Text = "Select an image to preview";
                    return true;
                }
                if (txtSearch.Focused)
                {
                    txtSearch.Clear();
                    listViewImages.Focus();
                    return true;
                }
                break;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    // ===========================
    // CLEANUP
    // ===========================

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        ClearPreview();
        _imageService.Cleanup(_allImages);
    }
}
