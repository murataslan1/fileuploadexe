using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using ImageToPdfMerger.Models;
using ImageToPdfMerger.Utils;

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

        // System.Drawing supports HEIC natively on Windows 10/11
        using var image = Image.FromFile(filePath);

        CorrectOrientation(image);

        item.Width = image.Width;
        item.Height = image.Height;

        // Always convert to standard JPEG for PdfSharpCore compatibility
        var tempPath = FileHelper.GetTempFilePath(".jpg");
        SaveAsJpeg(image, tempPath, 90);
        item.ConvertedPath = tempPath;

        item.Thumbnail = CreateThumbnail(image, 40);

        return item;
    }

    private void CorrectOrientation(Image image)
    {
        try
        {
            if (Array.IndexOf(image.PropertyIdList, 0x0112) < 0) return;

            var prop = image.GetPropertyItem(0x0112);
            if (prop?.Value == null || prop.Value.Length < 2) return;

            var orientation = BitConverter.ToUInt16(prop.Value, 0);
            switch (orientation)
            {
                case 2: image.RotateFlip(RotateFlipType.RotateNoneFlipX); break;
                case 3: image.RotateFlip(RotateFlipType.Rotate180FlipNone); break;
                case 4: image.RotateFlip(RotateFlipType.RotateNoneFlipY); break;
                case 5: image.RotateFlip(RotateFlipType.Rotate90FlipX); break;
                case 6: image.RotateFlip(RotateFlipType.Rotate90FlipNone); break;
                case 7: image.RotateFlip(RotateFlipType.Rotate270FlipX); break;
                case 8: image.RotateFlip(RotateFlipType.Rotate270FlipNone); break;
            }
            image.RemovePropertyItem(0x0112);
        }
        catch
        {
            // EXIF data not available or corrupted -- skip orientation fix
        }
    }

    private void SaveAsJpeg(Image image, string outputPath, int quality)
    {
        var jpegCodec = ImageCodecInfo.GetImageEncoders()
            .FirstOrDefault(c => c.FormatID == ImageFormat.Jpeg.Guid);

        if (jpegCodec != null)
        {
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, (long)quality);
            image.Save(outputPath, jpegCodec, encoderParams);
        }
        else
        {
            image.Save(outputPath, ImageFormat.Jpeg);
        }
    }

    private Image CreateThumbnail(Image sourceImage, int size)
    {
        int thumbWidth, thumbHeight;
        if (sourceImage.Width > sourceImage.Height)
        {
            thumbWidth = size;
            thumbHeight = (int)((double)sourceImage.Height / sourceImage.Width * size);
        }
        else
        {
            thumbHeight = size;
            thumbWidth = (int)((double)sourceImage.Width / sourceImage.Height * size);
        }

        if (thumbWidth < 1) thumbWidth = 1;
        if (thumbHeight < 1) thumbHeight = 1;

        var thumbnail = new Bitmap(thumbWidth, thumbHeight);
        using var g = Graphics.FromImage(thumbnail);
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.DrawImage(sourceImage, 0, 0, thumbWidth, thumbHeight);
        return thumbnail;
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
