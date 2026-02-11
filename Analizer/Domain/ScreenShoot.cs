using System.Drawing.Imaging;
using System.IO;

namespace Analizer.Domain
{
    public class ScreenShoot
    {
        public const int DEFAULT_QUALITY = 70;

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

        // Compressão POR QUALIDADE APENAS: mantém width/height originais.
        // Tenta reduzir a qualidade em passos até ficar abaixo de maxKb (KB).
        public byte[] ToBytesCompressed(int maxKb, int minQuality = 30)
        {
            if (maxKb <= 0) return ToBytes();

            byte[] lastBytes = ToBytes();
            if (lastBytes.Length <= maxKb * 1024) return lastBytes;

            // Tentar reduzir qualidade em passos (sem redimensionar)
            for (int q = Quality; q >= minQuality; q -= 5)
            {
                using var ms = new MemoryStream();
                var encoder = GetEncoder(Format);
                using var encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, (long)q);

                Bitmap.Save(ms, encoder, encoderParams);
                var bytes = ms.ToArray();
                if (bytes.Length <= maxKb * 1024) return bytes;
                lastBytes = bytes;
            }

            // Se não conseguiu, retorna o último (mais comprimido via qualidade) — dimensions mantidos.
            return lastBytes;
        }

        public void SetBitmap(Bitmap? newBitMap = null)
        {
            var old = Bitmap;
            Bitmap = newBitMap ?? new Bitmap(Width, Height, PixelFormat.Format24bppRgb);
            old.Dispose();
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            var codecs = ImageCodecInfo.GetImageDecoders();
            foreach (var c in codecs)
                if (c.FormatID == format.Guid) return c;

            throw new InvalidOperationException($"{Format.ToString()} encoder not found.");
        }

        public override string ToString()
        {
            return $"Captured primary {Width}x{Height} -> sent {Bitmap.Width}x{Bitmap.Height} ({ToBytes().Length / 1024} KB)";
        }
    }
}
