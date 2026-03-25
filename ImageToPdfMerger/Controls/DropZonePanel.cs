using System.Drawing;
using System.Drawing.Drawing2D;
using ImageToPdfMerger.Utils;

namespace ImageToPdfMerger.Controls;

public class DropZonePanel : UserControl
{
    private bool _isDragHover;

    public event EventHandler<string[]>? FilesDropped;

    public DropZonePanel()
    {
        AllowDrop = true;
        DoubleBuffered = true;
        Height = 180;
        Dock = DockStyle.Top;
        Cursor = Cursors.Hand;

        DragEnter += OnDragEnter;
        DragOver += OnDragOver;
        DragLeave += OnDragLeave;
        DragDrop += OnDragDrop;
        Click += OnClick;
    }

    private void OnDragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
        {
            var files = (string[]?)e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Any(FileHelper.IsSupportedImage))
            {
                e.Effect = DragDropEffects.Copy;
                _isDragHover = true;
                Invalidate();
                return;
            }
        }
        e.Effect = DragDropEffects.None;
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
            e.Effect = DragDropEffects.Copy;
    }

    private void OnDragLeave(object? sender, EventArgs e)
    {
        _isDragHover = false;
        Invalidate();
    }

    private void OnDragDrop(object? sender, DragEventArgs e)
    {
        _isDragHover = false;
        Invalidate();

        if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
        {
            var files = (string[]?)e.Data.GetData(DataFormats.FileDrop);
            if (files != null)
            {
                var supported = FileHelper.FilterSupportedFiles(files);
                if (supported.Length > 0)
                    FilesDropped?.Invoke(this, supported);
            }
        }
    }

    private void OnClick(object? sender, EventArgs e)
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
                FilesDropped?.Invoke(this, supported);
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var bgColor = _isDragHover ? Color.FromArgb(230, 240, 255) : Color.FromArgb(248, 248, 248);
        using var bgBrush = new SolidBrush(bgColor);
        g.FillRectangle(bgBrush, ClientRectangle);

        var borderColor = _isDragHover ? Color.FromArgb(80, 130, 220) : Color.FromArgb(180, 180, 180);
        using var borderPen = new Pen(borderColor, 2) { DashStyle = DashStyle.Dash };
        var borderRect = new Rectangle(10, 10, Width - 21, Height - 21);
        g.DrawRectangle(borderPen, borderRect);

        var iconFont = new Font("Segoe UI", 28f, FontStyle.Regular);
        var titleFont = new Font("Segoe UI", 11f, FontStyle.Bold);
        var subtitleFont = new Font("Segoe UI", 9f, FontStyle.Regular);

        var textColor = _isDragHover ? Color.FromArgb(60, 100, 200) : Color.FromArgb(120, 120, 120);
        using var textBrush = new SolidBrush(textColor);

        var iconText = "\U0001F4C2";
        var iconSize = g.MeasureString(iconText, iconFont);
        g.DrawString(iconText, iconFont, textBrush,
            (Width - iconSize.Width) / 2, Height / 2 - iconSize.Height - 5);

        var titleText = "Drag & drop images here";
        var titleSize = g.MeasureString(titleText, titleFont);
        g.DrawString(titleText, titleFont, textBrush,
            (Width - titleSize.Width) / 2, Height / 2 + 5);

        var subtitleText = "or click to browse (JPG, PNG, BMP, HEIC, WebP)";
        var subtitleSize = g.MeasureString(subtitleText, subtitleFont);
        using var subtitleBrush = new SolidBrush(Color.FromArgb(160, 160, 160));
        g.DrawString(subtitleText, subtitleFont, subtitleBrush,
            (Width - subtitleSize.Width) / 2, Height / 2 + 28);

        iconFont.Dispose();
        titleFont.Dispose();
        subtitleFont.Dispose();
    }
}
