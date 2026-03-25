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

    private byte[]? _currentPdfBytes;
    private List<Image>? _previewPages;
    private int _currentPageIndex;

    public MainForm()
    {
        InitializeComponent();
        SetupSearchAutoComplete();
        WireEvents();
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

        if (string.IsNullOrWhiteSpace(query))
        {
            _filteredImages.AddRange(_allImages);
        }
        else
        {
            _filteredImages.AddRange(
                _allImages.Where(i => i.FileName.Contains(query, StringComparison.OrdinalIgnoreCase)));
        }

        listViewImages.RefreshFromList(_filteredImages);

        // Auto-highlight exact match or single result
        if (_filteredImages.Count == 1)
        {
            listViewImages.ScrollToAndHighlight(0);
        }
        else if (!string.IsNullOrWhiteSpace(query))
        {
            int exactIndex = _filteredImages.FindIndex(
                i => i.FileName.Equals(query, StringComparison.OrdinalIgnoreCase));
            if (exactIndex >= 0)
                listViewImages.ScrollToAndHighlight(exactIndex);
        }

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
    // EVENT WIRING
    // ===========================

    private void WireEvents()
    {
        // DropZone
        dropZone.FilesDropped += OnFilesDropped;

        // Search
        txtSearch.TextChanged += OnSearchTextChanged;
        btnClearSearch.Click += OnClearSearchClick;

        // Toolbar Left
        btnDelete.Click += OnDeleteClick;
        btnMoveUp.Click += OnMoveUpClick;
        btnMoveDown.Click += OnMoveDownClick;
        btnClearAll.Click += OnClearAllClick;

        // Toolbar Right
        btnMergePreview.Click += OnMergePreviewClick;
        btnSavePdf.Click += OnSavePdfClick;

        // Navigation
        btnPrevPage.Click += (s, e) => ShowPreviewPage(_currentPageIndex - 1);
        btnNextPage.Click += (s, e) => ShowPreviewPage(_currentPageIndex + 1);

        // Form
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
                // Duplicate check
                if (_allImages.Any(existing =>
                    string.Equals(existing.FilePath, item.FilePath, StringComparison.OrdinalIgnoreCase)))
                {
                    item.Dispose();
                    continue;
                }
                item.Order = _allImages.Count;
                _allImages.Add(item);
            }

            RefreshAutoCompleteSource();
            ApplyFilter(txtSearch.Text.Trim());
            UpdateStatus();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading images:\n{ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        ApplyFilter(txtSearch.Text.Trim());
        UpdateStatus();
        UpdateMergeButtonState();
    }

    private void OnMoveUpClick(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtSearch.Text))
        {
            listViewImages.MoveSelectedUp();
            SyncAllImagesFromListView();
        }
    }

    private void OnMoveDownClick(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtSearch.Text))
        {
            listViewImages.MoveSelectedDown();
            SyncAllImagesFromListView();
        }
    }

    private void OnClearAllClick(object? sender, EventArgs e)
    {
        if (_allImages.Count == 0) return;

        var result = MessageBox.Show("Clear all images?", "Confirm",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (result != DialogResult.Yes) return;

        foreach (var item in _allImages)
            item.Dispose();
        _allImages.Clear();

        txtSearch.Clear();
        RefreshAutoCompleteSource();
        ApplyFilter(string.Empty);

        ClearPreview();
        UpdateStatus();
        UpdateMergeButtonState();
    }

    private void SyncAllImagesFromListView()
    {
        var ordered = listViewImages.GetOrderedItems();
        _allImages.Clear();
        _allImages.AddRange(ordered);
    }

    // ===========================
    // PDF MERGE & PREVIEW
    // ===========================

    private async void OnMergePreviewClick(object? sender, EventArgs e)
    {
        var settings = GetCurrentPdfSettings();
        var selectedImages = _allImages.Where(i => i.IsSelected).OrderBy(i => i.Order).ToList();

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
                MessageBox.Show(
                    $"Some images could not be added to PDF:\n\n{string.Join("\n", pdfErrors)}",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            progressBar.Value = 50;
            lblStatus.Text = "Rendering preview...";

            // Generate preview pages
            _previewPages?.ForEach(p => p.Dispose());
            _previewPages = new List<Image>();

            int previewWidth = Math.Max(picPreview.Width - 20, 200);
            foreach (var img in selectedImages)
            {
                var preview = _previewService.RenderPagePreview(img, settings, previewWidth);
                _previewPages.Add(preview);
            }

            _currentPageIndex = 0;
            ShowPreviewPage(0);
            btnSavePdf.Enabled = true;
            lblStatus.Text = $"PDF ready - {selectedImages.Count} pages";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"PDF generation error:\n{ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            lblStatus.Text = "Error generating PDF";
        }
        finally
        {
            progressBar.Visible = false;
            btnMergePreview.Enabled = _allImages.Count > 0;
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
        _previewPages?.ForEach(p => p.Dispose());
        _previewPages = null;
        _currentPdfBytes = null;
        picPreview.Image = null;
        lblPageInfo.Text = "Page 0 / 0";
        btnPrevPage.Enabled = false;
        btnNextPage.Enabled = false;
        btnSavePdf.Enabled = false;
    }

    // ===========================
    // SAVE PDF
    // ===========================

    private void OnSavePdfClick(object? sender, EventArgs e)
    {
        if (_currentPdfBytes == null) return;

        using var dialog = new SaveFileDialog
        {
            Title = "Save PDF",
            Filter = "PDF Files|*.pdf",
            DefaultExt = "pdf",
            FileName = "merged_images.pdf"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                _pdfService.SavePdf(_currentPdfBytes, dialog.FileName);
                lblStatus.Text = $"Saved: {dialog.FileName}";
                MessageBox.Show($"PDF saved successfully!\n{dialog.FileName}",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving PDF:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            PageSize = cmbPageSize.SelectedIndex switch
            {
                0 => PageSizeOption.A4,
                1 => PageSizeOption.Letter,
                2 => PageSizeOption.A3,
                3 => PageSizeOption.Original,
                _ => PageSizeOption.A4
            },
            Orientation = cmbOrientation.SelectedIndex switch
            {
                0 => PageOrientationOption.Portrait,
                1 => PageOrientationOption.Landscape,
                _ => PageOrientationOption.Portrait
            },
            ScaleMode = cmbScaleMode.SelectedIndex switch
            {
                0 => ScaleMode.FitToPage,
                1 => ScaleMode.OriginalSize,
                2 => ScaleMode.FitToWidth,
                _ => ScaleMode.FitToPage
            },
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
            lblStatus.Text = "Ready - Drag & drop images to get started";
            return;
        }

        long totalSize = _allImages.Sum(i => i.FileSize);
        int selected = _allImages.Count(i => i.IsSelected);
        lblStatus.Text = $"{_allImages.Count} images loaded ({selected} selected) | " +
                         $"Total: {FileHelper.FormatFileSize(totalSize)} | Ready";
    }

    private void UpdateMergeButtonState()
    {
        btnMergePreview.Enabled = _allImages.Count > 0 && _allImages.Any(i => i.IsSelected);
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
    // CLEANUP
    // ===========================

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        ClearPreview();
        _imageService.Cleanup(_allImages);
    }
}
