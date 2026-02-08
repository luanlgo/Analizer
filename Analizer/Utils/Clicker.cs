using System.Runtime.InteropServices;

public static class Clicker
{
    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int X, int Y);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    private const int INPUT_MOUSE = 0;
    private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const uint MOUSEEVENTF_LEFTUP = 0x0004;

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public MOUSEINPUT mi;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public nint dwExtraInfo;
    }

    public static bool TryLeftClickOnPrimaryScreen(int screenX, int screenY, Rectangle primaryBounds)
    {
        // Limita *estritamente* ao monitor principal
        if (!primaryBounds.Contains(screenX, screenY))
            return false;

        SetCursorPos(screenX, screenY);

        var inputs = new[]
        {
            new INPUT { type = INPUT_MOUSE, mi = new MOUSEINPUT { dwFlags = MOUSEEVENTF_LEFTDOWN } },
            new INPUT { type = INPUT_MOUSE, mi = new MOUSEINPUT { dwFlags = MOUSEEVENTF_LEFTUP } },
        };

        var sent = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());
        return sent == inputs.Length;
    }

    public static (int screenX, int screenY) MapImagePointToPrimaryScreen(
        int imageX, int imageY,
        int sentImageWidth, int sentImageHeight,
        Rectangle primaryBounds)
    {
        // A imagem enviada representa o monitor principal inteiro (só redimensionado)
        double scaleX = (double)primaryBounds.Width / sentImageWidth;
        double scaleY = (double)primaryBounds.Height / sentImageHeight;

        int screenX = primaryBounds.Left + (int)Math.Round(imageX * scaleX);
        int screenY = primaryBounds.Top + (int)Math.Round(imageY * scaleY);

        return (screenX, screenY);
    }

}
