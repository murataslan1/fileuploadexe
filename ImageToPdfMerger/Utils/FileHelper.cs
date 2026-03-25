namespace ImageToPdfMerger.Utils;

public static class FileHelper
{
    public static readonly string[] SupportedExtensions =
        { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff", ".heic", ".webp" };

    public static bool IsSupportedImage(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return SupportedExtensions.Contains(ext);
    }

    public static string[] FilterSupportedFiles(string[] paths)
    {
        return paths.Where(IsSupportedImage).ToArray();
    }

    public static string FormatFileSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        return $"{bytes / (1024.0 * 1024.0):F1} MB";
    }

    public static string GetTempFilePath(string extension = ".jpg")
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "ImageToPdfMerger");
        Directory.CreateDirectory(tempDir);
        return Path.Combine(tempDir, $"{Guid.NewGuid()}{extension}");
    }

    public static void CleanupTempFiles()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "ImageToPdfMerger");
        if (Directory.Exists(tempDir))
        {
            try { Directory.Delete(tempDir, true); } catch { }
        }
    }
}
