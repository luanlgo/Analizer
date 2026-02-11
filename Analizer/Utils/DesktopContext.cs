using Analizer.Domain;

namespace Analizer.Utils
{
    public static class DesktopContext
    {
        public static ScreenShoot CapturePrimaryScreenAsJpeg(int quality)
        {
            var screenShoot = new ScreenShoot(Screen.PrimaryScreen, quality);
            using (var g = Graphics.FromImage(screenShoot.Bitmap))
                g.CopyFromScreen(screenShoot.Bounds.Left, screenShoot.Bounds.Top, 0, 0, screenShoot.Bitmap.Size);

            // Não redimensionar — preservar width/height originais
            return screenShoot;
        }
    }
}
