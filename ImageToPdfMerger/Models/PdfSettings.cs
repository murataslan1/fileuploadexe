namespace ImageToPdfMerger.Models;

public class PdfSettings
{
    public PageSizeOption PageSize { get; set; } = PageSizeOption.A4;
    public PageOrientationOption Orientation { get; set; } = PageOrientationOption.Portrait;
    public ScaleMode ScaleMode { get; set; } = ScaleMode.FitToPage;
    public double MarginMm { get; set; } = 10;
    public int ImageQuality { get; set; } = 90;

    public double MarginPoints => MarginMm * (72.0 / 25.4);

    public (double Width, double Height) GetPageSizePoints()
    {
        var (w, h) = PageSize switch
        {
            PageSizeOption.A4 => (595.0, 842.0),
            PageSizeOption.Letter => (612.0, 792.0),
            PageSizeOption.A3 => (842.0, 1191.0),
            PageSizeOption.Original => (0.0, 0.0),
            _ => (595.0, 842.0)
        };

        if (Orientation == PageOrientationOption.Landscape && PageSize != PageSizeOption.Original)
            (w, h) = (h, w);

        return (w, h);
    }
}

public enum PageSizeOption
{
    A4,
    Letter,
    A3,
    Original
}

public enum PageOrientationOption
{
    Portrait,
    Landscape
}

public enum ScaleMode
{
    FitToPage,
    OriginalSize,
    FitToWidth
}
