using Analizer.Domain;
using Analizer.Utils;

namespace Analizer
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("DesktopAgent (digite: 'sair' para encerrar)");
            while (true)
            {
                Console.Write("\n> ");
                var cmd = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(cmd)) continue;
                if (cmd.Trim().Equals("sair", StringComparison.OrdinalIgnoreCase)) break;

                var screenShot = DesktopContext.CapturePrimaryScreenAsJpeg(70);

                Console.WriteLine(screenShot.ToString());


                ClickDecision decision;
                try
                {
                    decision = await OpenAiDesktop.GetClickDecisionAsync(cmd, screenShot);
                    Console.WriteLine($"Decisão: {decision.ToString()}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    continue;
                }
            }

            static (int screenX, int screenY) MapImagePointToPrimaryScreen(
                int imageX, int imageY,
                int sentW, int sentH,
                Rectangle bounds)
            {
                double scaleX = (double)bounds.Width / sentW;
                double scaleY = (double)bounds.Height / sentH;

                int screenX = bounds.Left + (int)Math.Round(imageX * scaleX);
                int screenY = bounds.Top + (int)Math.Round(imageY * scaleY);
                return (screenX, screenY);
            }
        }
    }
}
