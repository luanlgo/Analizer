using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

public static class DesktopContext
{
    public static int MaxWidth { get; set; } = 2048;

    public static (Rectangle bounds, byte[] jpegBytes, int sentW, int sentH) CapturePrimaryScreenAsJpeg(int jpegQuality = 70)
    {
        jpegQuality = Math.Clamp(jpegQuality, 1, 100);

        var screen = Screen.PrimaryScreen ?? throw new InvalidOperationException("PrimaryScreen is null.");
        Rectangle bounds = screen.Bounds;

        using var originalBmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb);
        using (var g = Graphics.FromImage(originalBmp))
            g.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, originalBmp.Size);

        using var resizedBmp = ResizeToMaxWidth(originalBmp, MaxWidth);

        int sentW = resizedBmp.Width;
        int sentH = resizedBmp.Height;

        using var ms = new MemoryStream();
        var jpgEncoder = GetEncoder(ImageFormat.Jpeg);
        using var encoderParams = new EncoderParameters(1);
        encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, (long)jpegQuality);

        resizedBmp.Save(ms, jpgEncoder, encoderParams);
        return (bounds, ms.ToArray(), sentW, sentH);
    }

    private static Bitmap ResizeToMaxWidth(Bitmap original, int maxWidth)
    {
        if (original.Width <= maxWidth)
            return new Bitmap(original);

        var ratio = (double)maxWidth / original.Width;
        var newHeight = (int)Math.Round(original.Height * ratio);

        var resized = new Bitmap(maxWidth, newHeight);
        using var g = Graphics.FromImage(resized);
        g.InterpolationMode =InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.SmoothingMode = SmoothingMode.HighQuality;

        g.DrawImage(original, 0, 0, maxWidth, newHeight);
        return resized;
    }

    private static ImageCodecInfo GetEncoder(ImageFormat format)
    {
        var codecs = ImageCodecInfo.GetImageDecoders();
        foreach (var c in codecs)
            if (c.FormatID == format.Guid) return c;

        throw new InvalidOperationException("JPEG encoder not found.");
    }
}
