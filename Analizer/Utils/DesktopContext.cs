using Analizer.Domain;

namespace Analizer.Utils
{
    public static class DesktopContext
    {
        public static ScreenShoot CapturePrimaryScreenAsJpeg(int? quality = null)
        {
            var screenShoot = new ScreenShoot(Screen.PrimaryScreen, quality);
            using (var g = Graphics.FromImage(screenShoot.Bitmap))
                g.CopyFromScreen(screenShoot.Bounds.Left, screenShoot.Bounds.Top, 0, 0, screenShoot.Bitmap.Size);

            screenShoot.ResizeToMaxWidth();
            return screenShoot;
        }
    }
}
