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
        Height = 150;
        Dock = DockStyle.Top;
        Cursor = Cursors.Hand;
        BackColor = Color.White;

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
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var rect = new Rectangle(12, 8, Width - 25, Height - 17);

        // Background gradient
        Color bgTop, bgBottom, borderColor, accentColor;
        if (_isDragHover)
        {
            bgTop = Color.FromArgb(235, 243, 255);
            bgBottom = Color.FromArgb(218, 232, 255);
            borderColor = Color.FromArgb(41, 121, 255);
            accentColor = Color.FromArgb(41, 121, 255);
        }
        else
        {
            bgTop = Color.FromArgb(250, 251, 254);
            bgBottom = Color.FromArgb(240, 244, 255);
            borderColor = Color.FromArgb(180, 198, 230);
            accentColor = Color.FromArgb(120, 150, 200);
        }

        // Rounded rectangle path
        int radius = 12;
        using var roundedPath = CreateRoundedRectangle(rect, radius);

        // Fill gradient background
        using var gradientBrush = new LinearGradientBrush(rect, bgTop, bgBottom, 90f);
        g.FillPath(gradientBrush, roundedPath);

        // Dashed border
        float dashWidth = _isDragHover ? 2.5f : 1.8f;
        using var borderPen = new Pen(borderColor, dashWidth);
        if (!_isDragHover)
        {
            borderPen.DashStyle = DashStyle.Custom;
            borderPen.DashPattern = new float[] { 6, 4 };
        }
        g.DrawPath(borderPen, roundedPath);

        // Upload arrow icon (drawn with GDI+)
        float centerX = Width / 2f;
        float iconY = Height / 2f - 38;
        DrawUploadIcon(g, centerX, iconY, accentColor);

        // Title text
        using var titleFont = new Font("Segoe UI Semibold", 11f, FontStyle.Bold);
        using var titleBrush = new SolidBrush(Color.FromArgb(50, 60, 80));
        var titleText = "Drop images here or click to browse";
        var titleSize = g.MeasureString(titleText, titleFont);
        g.DrawString(titleText, titleFont, titleBrush,
            (Width - titleSize.Width) / 2, Height / 2 + 8);

        // Subtitle
        using var subFont = new Font("Segoe UI", 8.5f);
        using var subBrush = new SolidBrush(Color.FromArgb(140, 155, 180));
        var subText = "JPG  PNG  BMP  HEIC  WebP  TIFF";
        var subSize = g.MeasureString(subText, subFont);
        g.DrawString(subText, subFont, subBrush,
            (Width - subSize.Width) / 2, Height / 2 + 30);
    }

    private void DrawUploadIcon(Graphics g, float cx, float cy, Color color)
    {
        using var pen = new Pen(color, 2.5f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
        using var fillBrush = new SolidBrush(Color.FromArgb(40, color));

        // Cloud shape
        float cloudW = 40, cloudH = 22;
        float cloudX = cx - cloudW / 2, cloudY = cy;
        using var cloudPath = new GraphicsPath();
        cloudPath.AddArc(cloudX, cloudY, cloudH, cloudH, 180, 180);
        cloudPath.AddArc(cloudX + cloudW - cloudH, cloudY, cloudH, cloudH, 180, 180);
        cloudPath.AddLine(cloudX + cloudW, cloudY + cloudH / 2, cloudX + cloudW, cloudY + cloudH);
        cloudPath.AddLine(cloudX + cloudW, cloudY + cloudH, cloudX, cloudY + cloudH);
        cloudPath.AddLine(cloudX, cloudY + cloudH, cloudX, cloudY + cloudH / 2);
        cloudPath.CloseFigure();

        g.FillPath(fillBrush, cloudPath);
        g.DrawPath(pen, cloudPath);

        // Upload arrow
        float arrowX = cx;
        float arrowTop = cy + 6;
        float arrowBottom = cy + cloudH - 2;
        g.DrawLine(pen, arrowX, arrowTop, arrowX, arrowBottom);
        g.DrawLine(pen, arrowX - 7, arrowTop + 7, arrowX, arrowTop);
        g.DrawLine(pen, arrowX + 7, arrowTop + 7, arrowX, arrowTop);
    }

    private GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        int d = radius * 2;
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
