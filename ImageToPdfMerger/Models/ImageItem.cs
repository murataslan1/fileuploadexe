using System.Drawing;

namespace ImageToPdfMerger.Models;

public class ImageItem : IDisposable
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public Image? Thumbnail { get; set; }
    public int Order { get; set; }
    public bool IsSelected { get; set; } = true;
    public string? ConvertedPath { get; set; }

    private bool _disposed;

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Thumbnail?.Dispose();
        Thumbnail = null;
        if (ConvertedPath != null && File.Exists(ConvertedPath))
        {
            try { File.Delete(ConvertedPath); } catch { }
        }
    }

    public string FormattedSize
    {
        get
        {
            if (FileSize < 1024) return $"{FileSize} B";
            if (FileSize < 1024 * 1024) return $"{FileSize / 1024.0:F1} KB";
            return $"{FileSize / (1024.0 * 1024.0):F1} MB";
        }
    }

    public string Resolution => $"{Width}x{Height}";

    public string DrawablePath => ConvertedPath ?? FilePath;
}
