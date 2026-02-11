using Analizer.Domain;
using Analizer.Domain.Enum;
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

                var screenShoot = DesktopContext.CapturePrimaryScreenAsJpeg(70);

                Console.WriteLine(screenShoot.ToString());

                ClickDecision decision;
                decision = await OpenAiDesktop.GetClickDecisionAsync(cmd, screenShoot);
                if (decision == null)
                {
                    Console.WriteLine("AI não retornou uma decisão válida.");
                    continue;
                }

                // valida coordenadas na imagem
                if (decision.X < 0 || decision.Y < 0 || decision.X >= screenShoot.Width || decision.Y >= screenShoot.Height)
                {
                    Console.WriteLine("Coords fora da imagem enviada. Abortando.");
                    continue;
                }

                // mapeia (coords na imagem -> coords reais na tela principal)
                var (screenX, screenY) = MapImagePointToPrimaryScreen(decision.X, decision.Y, screenShoot);

                // margem de segurança
                screenX = Math.Clamp(screenX, screenShoot.Bounds.Left + 5, screenShoot.Bounds.Right - 5);
                screenY = Math.Clamp(screenY, screenShoot.Bounds.Top + 5, screenShoot.Bounds.Bottom - 5);


                if (decision.Reason != null)
                {
                    Console.WriteLine($"{decision.Reason}. Confirmar? (y/n)");
                    if (Console.ReadLine()?.Trim().ToLowerInvariant() != "y")
                    {
                        Console.WriteLine("Cancelado.");
                        continue;
                    }
                }

                var ok = Clicker.TryLeftClickOnPrimaryScreen(screenX, screenY, screenShoot.Bounds);
                Console.WriteLine(ok ? "Clicked." : "Blocked (fora do principal).");
            }

            static (int screenX, int screenY) MapImagePointToPrimaryScreen(
                int imageX, int imageY,
                ScreenShoot screenShoot)
            {
                double scaleX = (double)screenShoot.Bounds.Width / screenShoot.Width;
                double scaleY = (double)screenShoot.Bounds.Height / screenShoot.Height;

                int screenX = screenShoot.Bounds.Left + (int)Math.Round(imageX * scaleX);
                int screenY = screenShoot.Bounds.Top + (int)Math.Round(imageY * scaleY);
                return (screenX, screenY);
            }
        }
    }
}
