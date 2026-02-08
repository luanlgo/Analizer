using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Analizer.Domain
{
    public class ScreenShoot
    {
        public const int DEFAULT_QUALITY = 70;
        public const int DEFAULT_MAX_WIDTH = 2048;

        public Rectangle Bounds { get; set; }
        public int Width { get; set; }
        public int MaxWidth { get; set; } = DEFAULT_MAX_WIDTH;
        public int Height { get; set; }
        public int Quality { get; set; }
        public ImageFormat Format { get; set; } = ImageFormat.Jpeg;
        public Bitmap Bitmap { get; set; } = new Bitmap(1, 1);

        public ScreenShoot(Screen? screen, int? quality)
        {
            SetScreen(screen);
            SetQualidty(quality);
            SetBitmap();
        }

        public void SetScreen(Screen? screen)
        {
            if (screen == null)
                throw new InvalidOperationException("PrimaryScreen is null.");
            
            Bounds = screen.Bounds;
            Width = Bounds.Width;
            Height = Bounds.Height;
        }

        public void SetQualidty(int? quality)
        {
            Quality = Math.Clamp(quality ?? DEFAULT_QUALITY, 1, 100);
        }

        public byte[] ToBytes()
        {
            using var ms = new MemoryStream();
            var encoder = GetEncoder(Format);
            using var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, (long)Quality);

            Bitmap.Save(ms, encoder, encoderParams);
            return ms.ToArray();
        }

        public void SetBitmap(Bitmap? newBitMap = null)
        {
            var old = Bitmap;
            Bitmap = newBitMap ?? new Bitmap(Width, Height, PixelFormat.Format24bppRgb);
            old.Dispose();
        }

        public void ResizeToMaxWidth()
        {
            if (Bitmap.Width <= MaxWidth)
            {
                Bitmap = new Bitmap(Bitmap);
                return;
            }

            var ratio = (double)MaxWidth / Bitmap.Width;
            var newHeight = (int)Math.Round(Bitmap.Height * ratio);

            var resized = new Bitmap(MaxWidth, newHeight);
            using var g = Graphics.FromImage(resized);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.SmoothingMode = SmoothingMode.HighQuality;

            g.DrawImage(Bitmap, 0, 0, MaxWidth, newHeight);
            SetBitmap(resized);
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            var codecs = ImageCodecInfo.GetImageDecoders();
            foreach (var c in codecs)
                if (c.FormatID == format.Guid) return c;

            throw new InvalidOperationException($"{Format.ToString()} encoder not found.");
        }
    }
}
