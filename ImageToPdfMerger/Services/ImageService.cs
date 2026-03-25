using System.Drawing;
using ImageToPdfMerger.Models;
using ImageToPdfMerger.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace ImageToPdfMerger.Services;

public class ImageService
{
    public ImageItem LoadImage(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        var item = new ImageItem
        {
            FilePath = filePath,
            FileName = fileInfo.Name,
            FileSize = fileInfo.Length,
            Order = 0
        };

        using var image = SixLabors.ImageSharp.Image.Load(filePath);
        image.Mutate(x => x.AutoOrient());

        item.Width = image.Width;
        item.Height = image.Height;

        if (FileHelper.NeedsConversion(filePath))
        {
            var tempPath = FileHelper.GetTempFilePath(".jpg");
            image.Save(tempPath, new JpegEncoder { Quality = 90 });
            item.ConvertedPath = tempPath;
        }

        item.Thumbnail = CreateThumbnail(image, 48);

        return item;
    }

    private System.Drawing.Image CreateThumbnail(SixLabors.ImageSharp.Image sourceImage, int size)
    {
        using var clone = sourceImage.Clone(x => x.Resize(new ResizeOptions
        {
            Size = new SixLabors.ImageSharp.Size(size, size),
            Mode = ResizeMode.Max,
            Sampler = KnownResamplers.Lanczos3
        }));

        var ms = new MemoryStream();
        clone.Save(ms, new JpegEncoder { Quality = 85 });
        ms.Position = 0;
        return System.Drawing.Image.FromStream(ms);
    }

    public async Task<List<ImageItem>> LoadImagesAsync(
        string[] filePaths,
        IProgress<int>? progress,
        CancellationToken ct)
    {
        var supportedFiles = FileHelper.FilterSupportedFiles(filePaths);
        var items = new List<ImageItem>();
        var errors = new List<string>();

        for (int i = 0; i < supportedFiles.Length; i++)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                var item = await Task.Run(() => LoadImage(supportedFiles[i]), ct);
                item.Order = i;
                items.Add(item);
            }
            catch (Exception ex)
            {
                errors.Add($"{Path.GetFileName(supportedFiles[i])}: {ex.Message}");
            }

            progress?.Report((int)((i + 1) / (double)supportedFiles.Length * 100));
        }

        if (errors.Count > 0)
        {
            System.Windows.Forms.MessageBox.Show(
                $"Some files could not be loaded:\n\n{string.Join("\n", errors)}",
                "Warning",
                System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.Warning);
        }

        return items;
    }

    public void Cleanup(List<ImageItem> items)
    {
        foreach (var item in items)
            item.Dispose();
        items.Clear();
        FileHelper.CleanupTempFiles();
    }
}
