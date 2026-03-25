using System.Drawing;
using System.Drawing.Drawing2D;
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
    private readonly List<string> _groups = new();
    private readonly AutoCompleteStringCollection _autoCompleteSource = new();

    private const string UngroupedKey = "";
    private const string UngroupedLabel = "Ungrouped";

    private bool _isPdfPreviewMode;
    private byte[]? _currentPdfBytes;
    private List<Image>? _previewPages;
    private int _currentPageIndex;
    private Image? _currentPreviewImage;
    private TreeNode? _dragNode;

    public MainForm()
    {
        InitializeComponent();
        CreateTreeIcons();
        SetupSearchAutoComplete();
        RebuildTree();
        WireEvents();
    }

    // ===========================
    // TREE ICONS
    // ===========================

    private void CreateTreeIcons()
    {
        // Folder icon (index 0)
        var folderBmp = new Bitmap(18, 18);
        using (var g = Graphics.FromImage(folderBmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);
            using var brush = new SolidBrush(Color.FromArgb(255, 183, 77));
            using var pen = new Pen(Color.FromArgb(200, 150, 50), 1);
            g.FillRectangle(brush, 1, 5, 15, 11);
            g.DrawRectangle(pen, 1, 5, 15, 11);
            g.FillRectangle(brush, 1, 3, 7, 4);
        }
        treeImageList.Images.Add("folder", folderBmp);

        // Image icon (index 1)
        var imgBmp = new Bitmap(18, 18);
        using (var g = Graphics.FromImage(imgBmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);
            using var brush = new SolidBrush(Color.FromArgb(100, 180, 255));
            using var pen = new Pen(Color.FromArgb(70, 130, 200), 1);
            g.FillRectangle(brush, 2, 1, 14, 16);
            g.DrawRectangle(pen, 2, 1, 14, 16);
            using var mountBrush = new SolidBrush(Color.FromArgb(80, 160, 80));
            g.FillPolygon(mountBrush, new[] { new Point(2, 17), new Point(9, 9), new Point(16, 17) });
        }
        treeImageList.Images.Add("image", imgBmp);

        // All icon (index 2)
        var allBmp = new Bitmap(18, 18);
        using (var g = Graphics.FromImage(allBmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);
            using var brush = new SolidBrush(AccentBlue);
            g.FillEllipse(brush, 2, 2, 14, 14);
            using var textBrush = new SolidBrush(Color.White);
            using var font = new Font("Segoe UI", 7f, FontStyle.Bold);
            g.DrawString("A", font, textBrush, 4, 2);
        }
        treeImageList.Images.Add("all", allBmp);
    }

    // ===========================
    // TREE VIEW MANAGEMENT
    // ===========================

    private void RebuildTree()
    {
        var searchQuery = txtSearch?.Text?.Trim() ?? "";
        var selectedPath = treeImages.SelectedNode?.Tag;

        treeImages.BeginUpdate();
        treeImages.Nodes.Clear();

        // "All" node
        var allNode = new TreeNode($"All ({_allImages.Count})")
        {
            ImageKey = "all",
            SelectedImageKey = "all",
            Tag = "__all__",
            NodeFont = new Font(treeImages.Font, FontStyle.Bold)
        };
        treeImages.Nodes.Add(allNode);

        // Group nodes
        foreach (var group in _groups)
        {
            var groupImages = _allImages.Where(i => i.GroupName == group).ToList();
            var groupNode = new TreeNode($"{group} ({groupImages.Count})")
            {
                ImageKey = "folder",
                SelectedImageKey = "folder",
                Tag = group,
                NodeFont = new Font(treeImages.Font, FontStyle.Bold)
            };

            foreach (var img in groupImages)
            {
                if (!string.IsNullOrEmpty(searchQuery) &&
                    !img.FileName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                    continue;

                var imgNode = new TreeNode(img.FileName)
                {
                    ImageKey = "image",
                    SelectedImageKey = "image",
                    Tag = img,
                    Checked = img.IsSelected
                };
                groupNode.Nodes.Add(imgNode);
            }

            treeImages.Nodes.Add(groupNode);
            groupNode.Expand();
        }

        // Ungrouped node
        var ungrouped = _allImages.Where(i => string.IsNullOrEmpty(i.GroupName)).ToList();
        if (ungrouped.Count > 0 || _groups.Count == 0)
        {
            var ungroupedNode = new TreeNode($"{UngroupedLabel} ({ungrouped.Count})")
            {
                ImageKey = "folder",
                SelectedImageKey = "folder",
                Tag = UngroupedKey,
                NodeFont = new Font(treeImages.Font, FontStyle.Bold)
            };

            foreach (var img in ungrouped)
            {
                if (!string.IsNullOrEmpty(searchQuery) &&
                    !img.FileName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                    continue;

                var imgNode = new TreeNode(img.FileName)
                {
                    ImageKey = "image",
                    SelectedImageKey = "image",
                    Tag = img,
                    Checked = img.IsSelected
                };
                ungroupedNode.Nodes.Add(imgNode);
            }

            treeImages.Nodes.Add(ungroupedNode);
            ungroupedNode.Expand();
        }

        treeImages.EndUpdate();

        // Restore selection
        if (selectedPath != null)
            SelectNodeByTag(selectedPath);

        UpdateMergeButtonState();
    }

    private void SelectNodeByTag(object tag)
    {
        foreach (TreeNode node in treeImages.Nodes)
        {
            if (node.Tag == tag || (node.Tag is string s && tag is string t && s == t))
            {
                treeImages.SelectedNode = node;
                return;
            }
            foreach (TreeNode child in node.Nodes)
            {
                if (child.Tag == tag)
                {
                    treeImages.SelectedNode = child;
                    return;
                }
            }
        }
    }

    private string? GetSelectedGroupName()
    {
        var node = treeImages.SelectedNode;
        if (node == null) return null;

        // If image node, get parent group
        if (node.Tag is ImageItem)
            node = node.Parent;

        if (node?.Tag is string groupKey)
        {
            if (groupKey == "__all__") return null; // "All" means all images
            return groupKey; // "" = ungrouped, or group name
        }

        return null;
    }

    private List<ImageItem> GetImagesForPdf()
    {
        var groupKey = GetSelectedGroupName();

        IEnumerable<ImageItem> source;
        if (groupKey == null) // "All" selected
            source = _allImages;
        else if (groupKey == UngroupedKey)
            source = _allImages.Where(i => string.IsNullOrEmpty(i.GroupName));
        else
            source = _allImages.Where(i => i.GroupName == groupKey);

        return source.Where(i => i.IsSelected).OrderBy(i => i.Order).ToList();
    }

    // ===========================
    // DRAG & DROP (TreeView)
    // ===========================

    private void OnTreeItemDrag(object? sender, ItemDragEventArgs e)
    {
        if (e.Item is TreeNode node && node.Tag is ImageItem)
        {
            _dragNode = node;
            DoDragDrop(node, DragDropEffects.Move);
        }
    }

    private void OnTreeDragOver(object? sender, DragEventArgs e)
    {
        var targetPoint = treeImages.PointToClient(new Point(e.X, e.Y));
        var targetNode = treeImages.GetNodeAt(targetPoint);

        if (targetNode != null && IsGroupNode(targetNode) && _dragNode != null)
        {
            e.Effect = DragDropEffects.Move;
            treeImages.SelectedNode = targetNode;
        }
        else
        {
            e.Effect = DragDropEffects.None;
        }
    }

    private void OnTreeDragDrop(object? sender, DragEventArgs e)
    {
        var targetPoint = treeImages.PointToClient(new Point(e.X, e.Y));
        var targetNode = treeImages.GetNodeAt(targetPoint);

        if (_dragNode?.Tag is ImageItem item && targetNode != null && IsGroupNode(targetNode))
        {
            var targetGroup = (string)targetNode.Tag!;
            if (targetGroup == "__all__") return;

            item.GroupName = targetGroup;
            RebuildTree();
            UpdateStatus();
        }

        _dragNode = null;
    }

    private bool IsGroupNode(TreeNode node) => node.Tag is string;

    // ===========================
    // CONTEXT MENU (Right-click)
    // ===========================

    private void OnTreeNodeMouseClick(object? sender, TreeNodeMouseClickEventArgs e)
    {
        if (e.Button != MouseButtons.Right) return;
        treeImages.SelectedNode = e.Node;

        var menu = new ContextMenuStrip();

        if (e.Node.Tag is ImageItem item)
        {
            menu.Items.Add("Preview", null, (s, ev) => PreviewImage(item));
            menu.Items.Add(new ToolStripSeparator());

            // Move to submenu
            var moveMenu = new ToolStripMenuItem("Move to...");
            foreach (var group in _groups)
            {
                var g = group;
                moveMenu.DropDownItems.Add(group, null, (s, ev) => { item.GroupName = g; RebuildTree(); });
            }
            moveMenu.DropDownItems.Add(new ToolStripSeparator());
            moveMenu.DropDownItems.Add(UngroupedLabel, null, (s, ev) => { item.GroupName = UngroupedKey; RebuildTree(); });
            menu.Items.Add(moveMenu);

            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Delete", null, (s, ev) =>
            {
                _allImages.Remove(item);
                item.Dispose();
                RebuildTree();
                UpdateStatus();
            });
        }
        else if (e.Node.Tag is string groupKey && groupKey != "__all__")
        {
            if (groupKey != UngroupedKey)
            {
                menu.Items.Add("Rename Group", null, (s, ev) => e.Node.BeginEdit());
                menu.Items.Add("Delete Group", null, (s, ev) => DeleteGroup(groupKey));
                menu.Items.Add(new ToolStripSeparator());
            }
            menu.Items.Add("Create PDF from this Group", null, (s, ev) => CreatePdfFromGroup(groupKey));
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("New Group...", null, (s, ev) => AddNewGroup());
        }
        else
        {
            menu.Items.Add("New Group...", null, (s, ev) => AddNewGroup());
        }

        menu.Show(treeImages, e.Location);
    }

    private void OnTreeAfterLabelEdit(object? sender, NodeLabelEditEventArgs e)
    {
        if (e.Label == null || e.Node?.Tag is not string oldName || oldName == "__all__" || oldName == UngroupedKey)
        {
            e.CancelEdit = true;
            return;
        }

        var newName = e.Label.Trim();
        if (string.IsNullOrEmpty(newName) || _groups.Contains(newName))
        {
            e.CancelEdit = true;
            return;
        }

        var idx = _groups.IndexOf(oldName);
        if (idx >= 0) _groups[idx] = newName;

        foreach (var img in _allImages.Where(i => i.GroupName == oldName))
            img.GroupName = newName;

        e.CancelEdit = true; // We rebuild manually
        BeginInvoke(() => RebuildTree());
    }

    private void DeleteGroup(string groupName)
    {
        var result = MessageBox.Show($"Delete group \"{groupName}\"?\nImages will be moved to Ungrouped.",
            "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (result != DialogResult.Yes) return;

        foreach (var img in _allImages.Where(i => i.GroupName == groupName))
            img.GroupName = UngroupedKey;

        _groups.Remove(groupName);
        RebuildTree();
        UpdateStatus();
    }

    private void CreatePdfFromGroup(string groupKey)
    {
        // Select the group node first
        foreach (TreeNode node in treeImages.Nodes)
        {
            if (node.Tag is string key && key == groupKey)
            {
                treeImages.SelectedNode = node;
                break;
            }
        }
        OnMergePreviewClick(this, EventArgs.Empty);
    }

    // ===========================
    // GROUP MANAGEMENT
    // ===========================

    private void AddNewGroup()
    {
        var name = PromptGroupName("New Group", $"Group {_groups.Count + 1}");
        if (name == null) return;
        if (_groups.Contains(name))
        {
            MessageBox.Show($"Group \"{name}\" already exists.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        _groups.Add(name);
        RebuildTree();
    }

    private string? PromptGroupName(string title, string defaultValue)
    {
        using var form = new Form
        {
            Text = title, Size = new Size(350, 150),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false, MinimizeBox = false
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

    private void OnSearchTextChanged(object? sender, EventArgs e) => RebuildTree();
    private void OnClearSearchClick(object? sender, EventArgs e) { txtSearch.Clear(); txtSearch.Focus(); }

    // ===========================
    // IMAGE PREVIEW
    // ===========================

    private void OnTreeAfterSelect(object? sender, TreeViewEventArgs e)
    {
        if (_isPdfPreviewMode)
        {
            _isPdfPreviewMode = false;
            navPanel.Visible = false;
        }

        if (e.Node?.Tag is ImageItem item)
        {
            PreviewImage(item);
        }
        else if (e.Node?.Tag is string groupKey)
        {
            // Show group summary
            _currentPreviewImage?.Dispose();
            _currentPreviewImage = null;
            picPreview.Image = null;

            IEnumerable<ImageItem> imgs;
            if (groupKey == "__all__")
                imgs = _allImages;
            else if (groupKey == UngroupedKey)
                imgs = _allImages.Where(i => string.IsNullOrEmpty(i.GroupName));
            else
                imgs = _allImages.Where(i => i.GroupName == groupKey);

            var list = imgs.ToList();
            var totalSize = list.Sum(i => i.FileSize);
            var selected = list.Count(i => i.IsSelected);
            lblImageInfo.Text = $"{list.Count} images | {selected} selected | {FileHelper.FormatFileSize(totalSize)}";
        }

        UpdateMergeButtonState();
    }

    private void PreviewImage(ImageItem item)
    {
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
    // CHECKBOX SYNC
    // ===========================

    private void OnTreeAfterCheck(object? sender, TreeViewEventArgs e)
    {
        if (e.Action == TreeViewAction.Unknown) return; // programmatic, skip

        if (e.Node?.Tag is ImageItem item)
        {
            item.IsSelected = e.Node.Checked;
        }
        else if (e.Node != null && IsGroupNode(e.Node))
        {
            // Check/uncheck all children
            foreach (TreeNode child in e.Node.Nodes)
            {
                child.Checked = e.Node.Checked;
                if (child.Tag is ImageItem childItem)
                    childItem.IsSelected = e.Node.Checked;
            }
        }

        UpdateMergeButtonState();
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
        btnAddGroup.Click += (s, e) => AddNewGroup();
        btnClearAll.Click += OnClearAllClick;

        btnMergePreview.Click += OnMergePreviewClick;
        btnSavePdf.Click += OnSavePdfClick;
        btnPrevPage.Click += (s, e) => ShowPreviewPage(_currentPageIndex - 1);
        btnNextPage.Click += (s, e) => ShowPreviewPage(_currentPageIndex + 1);

        treeImages.AfterSelect += OnTreeAfterSelect;
        treeImages.AfterCheck += OnTreeAfterCheck;
        treeImages.ItemDrag += OnTreeItemDrag;
        treeImages.DragOver += OnTreeDragOver;
        treeImages.DragDrop += OnTreeDragDrop;
        treeImages.NodeMouseClick += OnTreeNodeMouseClick;
        treeImages.AfterLabelEdit += OnTreeAfterLabelEdit;

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

        var progress = new Progress<int>(p => { if (!IsDisposed) progressBar.Value = p; });

        try
        {
            var newItems = await _imageService.LoadImagesAsync(filePaths, progress, CancellationToken.None);
            foreach (var item in newItems)
            {
                if (_allImages.Any(x => string.Equals(x.FilePath, item.FilePath, StringComparison.OrdinalIgnoreCase)))
                {
                    item.Dispose();
                    continue;
                }
                item.Order = _allImages.Count;
                _allImages.Add(item);
            }
            RefreshAutoCompleteSource();
            RebuildTree();
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
        var node = treeImages.SelectedNode;
        if (node?.Tag is ImageItem item)
        {
            _allImages.Remove(item);
            item.Dispose();
            RebuildTree();
            UpdateStatus();
        }
    }

    private void OnMoveUpClick(object? sender, EventArgs e)
    {
        var node = treeImages.SelectedNode;
        if (node?.Tag is not ImageItem item || node.Parent == null) return;
        var idx = _allImages.IndexOf(item);
        if (idx <= 0) return;
        // Swap order
        var prev = _allImages[idx - 1];
        (item.Order, prev.Order) = (prev.Order, item.Order);
        (_allImages[idx], _allImages[idx - 1]) = (_allImages[idx - 1], _allImages[idx]);
        RebuildTree();
        SelectNodeByTag(item);
    }

    private void OnMoveDownClick(object? sender, EventArgs e)
    {
        var node = treeImages.SelectedNode;
        if (node?.Tag is not ImageItem item || node.Parent == null) return;
        var idx = _allImages.IndexOf(item);
        if (idx < 0 || idx >= _allImages.Count - 1) return;
        var next = _allImages[idx + 1];
        (item.Order, next.Order) = (next.Order, item.Order);
        (_allImages[idx], _allImages[idx + 1]) = (_allImages[idx + 1], _allImages[idx]);
        RebuildTree();
        SelectNodeByTag(item);
    }

    private void OnClearAllClick(object? sender, EventArgs e)
    {
        if (_allImages.Count == 0) return;
        if (MessageBox.Show("Clear all images?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        foreach (var item in _allImages) item.Dispose();
        _allImages.Clear();
        _groups.Clear();
        txtSearch.Clear();
        RefreshAutoCompleteSource();
        RebuildTree();
        ClearPreview();
        UpdateStatus();
    }

    // ===========================
    // PDF MERGE & PREVIEW
    // ===========================

    private async void OnMergePreviewClick(object? sender, EventArgs e)
    {
        var settings = GetCurrentPdfSettings();
        var selectedImages = GetImagesForPdf();

        if (selectedImages.Count == 0)
        {
            MessageBox.Show("No checked images found. Check the images you want in the PDF.",
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
                MessageBox.Show($"Some images could not be added:\n\n{string.Join("\n", pdfErrors)}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            _previewPages?.ForEach(p => p.Dispose());
            _previewPages = new List<Image>();
            int previewWidth = Math.Max(picPreview.Width - 20, 200);
            foreach (var img in selectedImages)
                _previewPages.Add(_previewService.RenderPagePreview(img, settings, previewWidth));

            _isPdfPreviewMode = true;
            navPanel.Visible = true;
            _currentPageIndex = 0;
            ShowPreviewPage(0);
            btnSavePdf.Enabled = true;
            lblStatus.Text = $"PDF ready -- {selectedImages.Count} pages";
            lblImageInfo.Text = "PDF Preview (select a tree node to return to image preview)";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"PDF error:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            lblStatus.Text = "Error generating PDF";
        }
        finally
        {
            progressBar.Visible = false;
            btnMergePreview.Enabled = true;
        }
    }

    private void ShowPreviewPage(int index)
    {
        if (_previewPages == null || _previewPages.Count == 0) return;
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
        btnSavePdf.Enabled = false;
        lblImageInfo.Text = "Select an image to preview";
    }

    // ===========================
    // SAVE PDF
    // ===========================

    private void OnSavePdfClick(object? sender, EventArgs e)
    {
        if (_currentPdfBytes == null) return;
        var groupName = GetSelectedGroupName();
        string defaultName = !string.IsNullOrEmpty(groupName) ? $"{groupName}.pdf" : "merged_images.pdf";

        using var dialog = new SaveFileDialog { Title = "Save PDF", Filter = "PDF Files|*.pdf", DefaultExt = "pdf", FileName = defaultName };
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
                MessageBox.Show($"Error saving:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // ===========================
    // HELPERS
    // ===========================

    private PdfSettings GetCurrentPdfSettings() => new()
    {
        PageSize = cmbPageSize.SelectedIndex switch { 0 => PageSizeOption.A4, 1 => PageSizeOption.Letter, 2 => PageSizeOption.A3, 3 => PageSizeOption.Original, _ => PageSizeOption.A4 },
        Orientation = cmbOrientation.SelectedIndex switch { 0 => PageOrientationOption.Portrait, 1 => PageOrientationOption.Landscape, _ => PageOrientationOption.Portrait },
        ScaleMode = cmbScaleMode.SelectedIndex switch { 0 => ScaleMode.FitToPage, 1 => ScaleMode.OriginalSize, 2 => ScaleMode.FitToWidth, _ => ScaleMode.FitToPage },
        MarginMm = chkMargin.Checked ? 10 : 0,
        ImageQuality = 90
    };

    private void UpdateStatus()
    {
        if (_allImages.Count == 0) { lblStatus.Text = "Ready -- Drop images to get started"; return; }
        long totalSize = _allImages.Sum(i => i.FileSize);
        lblStatus.Text = $"{_allImages.Count} images | {_groups.Count} groups | {FileHelper.FormatFileSize(totalSize)} | Ready";
    }

    private void UpdateMergeButtonState()
    {
        btnMergePreview.Enabled = _allImages.Count > 0 && _allImages.Any(i => i.IsSelected);
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
            if (supported.Length > 0) OnFilesDropped(this, supported);
        }
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        switch (keyData)
        {
            case Keys.Control | Keys.F: txtSearch.Focus(); txtSearch.SelectAll(); return true;
            case Keys.Control | Keys.O: OpenFileDialogManual(); return true;
            case Keys.Control | Keys.S when btnSavePdf.Enabled: OnSavePdfClick(this, EventArgs.Empty); return true;
            case Keys.Delete when treeImages.Focused: OnDeleteClick(this, EventArgs.Empty); return true;
            case Keys.Control | Keys.Up: OnMoveUpClick(this, EventArgs.Empty); return true;
            case Keys.Control | Keys.Down: OnMoveDownClick(this, EventArgs.Empty); return true;
            case Keys.F5 when btnMergePreview.Enabled: OnMergePreviewClick(this, EventArgs.Empty); return true;
            case Keys.Escape when _isPdfPreviewMode: _isPdfPreviewMode = false; navPanel.Visible = false; picPreview.Image = null; lblImageInfo.Text = "Select an image to preview"; return true;
            case Keys.Escape when txtSearch.Focused: txtSearch.Clear(); treeImages.Focus(); return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        ClearPreview();
        _imageService.Cleanup(_allImages);
    }
}
