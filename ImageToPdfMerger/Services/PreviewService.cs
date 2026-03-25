using System.Drawing;
using System.Drawing.Drawing2D;
using ImageToPdfMerger.Models;

namespace ImageToPdfMerger.Services;

public class PreviewService
{
    public Image RenderPagePreview(ImageItem item, PdfSettings settings, int previewWidth)
    {
        var (pageW, pageH) = settings.GetPageSizePoints();

        if (settings.PageSize == PageSizeOption.Original)
        {
            pageW = item.Width;
            pageH = item.Height;
        }

        double aspectRatio = pageH / pageW;
        int bmpWidth = previewWidth;
        int bmpHeight = (int)(previewWidth * aspectRatio);

        if (bmpHeight < 1) bmpHeight = 1;
        if (bmpWidth < 1) bmpWidth = 1;

        var bitmap = new Bitmap(bmpWidth, bmpHeight);
        using var g = Graphics.FromImage(bitmap);
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;

        g.Clear(Color.White);

        var margin = settings.PageSize == PageSizeOption.Original ? 0 : settings.MarginPoints;
        var (imgX, imgY, imgW, imgH) = PdfService.CalculateImagePlacement(
            item.Width, item.Height,
            pageW, pageH,
            margin, settings.ScaleMode,
            settings.PageSize == PageSizeOption.Original);

        double scaleToPreview = bmpWidth / pageW;
        int drawX = (int)(imgX * scaleToPreview);
        int drawY = (int)(imgY * scaleToPreview);
        int drawW = (int)(imgW * scaleToPreview);
        int drawH = (int)(imgH * scaleToPreview);

        try
        {
            using var sourceImage = Image.FromFile(item.DrawablePath);
            g.DrawImage(sourceImage, drawX, drawY, drawW, drawH);
        }
        catch
        {
            g.FillRectangle(Brushes.LightGray, drawX, drawY, drawW, drawH);
            g.DrawString("Preview unavailable", SystemFonts.DefaultFont, Brushes.Gray, drawX + 10, drawY + 10);
        }

        using var borderPen = new Pen(Color.LightGray, 1);
        g.DrawRectangle(borderPen, 0, 0, bmpWidth - 1, bmpHeight - 1);

        return bitmap;
    }

    public int GetPageCount(List<ImageItem> items)
    {
        return items.Count(i => i.IsSelected);
    }
}
