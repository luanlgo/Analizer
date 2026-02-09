using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Analizer.Domain
{
    public class ScreenShoot
    {
        public const int DEFAULT_QUALITY = 70;
        public const int DEFAULT_MAX_WIDTH = 2048;

        private Screen _screen;
        public Screen Screen 
        { 
            get => _screen;
            set 
            {
                _screen = value ?? throw new InvalidOperationException("PrimaryScreen is null.");

                Bounds = value.Bounds;
                Width = Bounds.Width;
                Height = Bounds.Height;
            }
        }
        public Rectangle Bounds { get; set; }
        public int Width { get; set; }
        public int MaxWidth { get; set; } = DEFAULT_MAX_WIDTH;
        public int Height { get; set; }
        private int _quality = DEFAULT_QUALITY;
        public int Quality
        {
            get => _quality;
            set
            {
                _quality = Math.Clamp(value, 1, 100);
            }
        }
        public ImageFormat Format { get; set; } = ImageFormat.Jpeg;
        public Bitmap Bitmap { get; set; } = new Bitmap(1, 1);

        public ScreenShoot(Screen screen, int quality)
        {
            Screen = screen;
            Quality = quality;
            SetBitmap();
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
    
        public int GetWidthToSend()
        {
            return Bitmap.Width;
        }

        public int GetHeightToSend()
        {
            return Bitmap.Height;
        }

        public override string ToString()
        {
            return $"Captured primary {Width}x{Height} -> sent {Bitmap.Width}x{Bitmap.Height} ({ToBytes().Length / 1024} KB)";
        }
    }
}
