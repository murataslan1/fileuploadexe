using ImageToPdfMerger.Models;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace ImageToPdfMerger.Services;

public class PdfService
{
    public byte[] MergeImagesToPdf(List<ImageItem> images, PdfSettings settings)
    {
        var document = new PdfDocument();
        document.Info.Title = "Merged Images";
        document.Info.Creator = "ImageToPdfMerger";

        var selectedImages = images.Where(i => i.IsSelected).OrderBy(i => i.Order).ToList();

        foreach (var item in selectedImages)
        {
            AddImageToPage(document, item, settings);
        }

        using var ms = new MemoryStream();
        document.Save(ms, false);
        return ms.ToArray();
    }

    private void AddImageToPage(PdfDocument document, ImageItem item, PdfSettings settings)
    {
        var page = document.AddPage();
        var imagePath = item.DrawablePath;

        using var xImage = XImage.FromFile(imagePath);

        if (settings.PageSize == PageSizeOption.Original)
        {
            page.Width = XUnit.FromPoint(xImage.PointWidth);
            page.Height = XUnit.FromPoint(xImage.PointHeight);
        }
        else
        {
            var (pw, ph) = settings.GetPageSizePoints();
            page.Width = XUnit.FromPoint(pw);
            page.Height = XUnit.FromPoint(ph);
        }

        if (settings.PageSize != PageSizeOption.Original &&
            settings.Orientation == PageOrientationOption.Landscape)
        {
            page.Orientation = PdfSharpCore.PageOrientation.Landscape;
        }

        using var gfx = XGraphics.FromPdfPage(page);

        var margin = settings.PageSize == PageSizeOption.Original ? 0 : settings.MarginPoints;

        var (x, y, w, h) = CalculateImagePlacement(
            xImage.PointWidth, xImage.PointHeight,
            page.Width.Point, page.Height.Point,
            margin, settings.ScaleMode,
            settings.PageSize == PageSizeOption.Original);

        gfx.DrawImage(xImage, x, y, w, h);
    }

    public static (double x, double y, double w, double h) CalculateImagePlacement(
        double imgWidth, double imgHeight,
        double pageWidth, double pageHeight,
        double margin, ScaleMode mode,
        bool isOriginalPageSize = false)
    {
        if (isOriginalPageSize)
            return (0, 0, imgWidth, imgHeight);

        double usableWidth = pageWidth - 2 * margin;
        double usableHeight = pageHeight - 2 * margin;

        double drawWidth, drawHeight;

        switch (mode)
        {
            case ScaleMode.FitToPage:
                double scaleX = usableWidth / imgWidth;
                double scaleY = usableHeight / imgHeight;
                double scale = Math.Min(scaleX, scaleY);
                drawWidth = imgWidth * scale;
                drawHeight = imgHeight * scale;
                break;

            case ScaleMode.FitToWidth:
                double widthScale = usableWidth / imgWidth;
                drawWidth = imgWidth * widthScale;
                drawHeight = imgHeight * widthScale;
                if (drawHeight > usableHeight)
                    drawHeight = usableHeight;
                break;

            case ScaleMode.OriginalSize:
                drawWidth = Math.Min(imgWidth, usableWidth);
                drawHeight = Math.Min(imgHeight, usableHeight);
                break;

            default:
                goto case ScaleMode.FitToPage;
        }

        double x = margin + (usableWidth - drawWidth) / 2;
        double y = margin + (usableHeight - drawHeight) / 2;

        return (x, y, drawWidth, drawHeight);
    }

    public void SavePdf(byte[] pdfBytes, string outputPath)
    {
        File.WriteAllBytes(outputPath, pdfBytes);
    }

    public string SaveToTempFile(byte[] pdfBytes)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"preview_{Guid.NewGuid()}.pdf");
        File.WriteAllBytes(tempPath, pdfBytes);
        return tempPath;
    }
}
