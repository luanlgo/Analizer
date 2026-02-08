using Analizer.Domain;

namespace Analizer
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            DesktopContext.MaxWidth = 2048;

            Console.WriteLine("DesktopAgent (digite: 'sair' para encerrar)");
            while (true)
            {
                Console.Write("\n> ");
                var cmd = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(cmd)) continue;
                if (cmd.Trim().Equals("sair", StringComparison.OrdinalIgnoreCase)) break;

                var (bounds, bytes, sentW, sentH) = DesktopContext.CapturePrimaryScreenAsJpeg(60);
                Console.WriteLine($"Captured primary {bounds.Width}x{bounds.Height} -> sent {sentW}x{sentH} ({bytes.Length / 1024} KB)");

                ClickDecision decision;
                try
                {
                    decision = await OpenAiDesktop.GetClickDecisionAsync(cmd, bytes, sentW, sentH);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    continue;
                }

                Console.WriteLine($"AI: action={decision.Action}, conf={decision.Confidence:F2}, reason={decision.Reason}");

                if (decision.Action.Equals("ask_confirmation", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Pergunta: {decision.Question}");
                    continue;
                }

                if (!decision.Action.Equals("click", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Sem click.");
                    continue;
                }

                // valida coordenadas na imagem
                if (decision.X < 0 || decision.Y < 0 || decision.X >= sentW || decision.Y >= sentH)
                {
                    Console.WriteLine("Coords fora da imagem enviada. Abortando.");
                    continue;
                }

                // mapeia (coords na imagem -> coords reais na tela principal)
                var (screenX, screenY) = MapImagePointToPrimaryScreen(decision.X, decision.Y, sentW, sentH, bounds);

                // margem de segurança
                screenX = Math.Clamp(screenX, bounds.Left + 5, bounds.Right - 5);
                screenY = Math.Clamp(screenY, bounds.Top + 5, bounds.Bottom - 5);

                Console.WriteLine($"bounds: L={bounds.Left} T={bounds.Top} W={bounds.Width} H={bounds.Height}");
                Console.WriteLine($"sent:   W={sentW} H={sentH}");
                Console.WriteLine($"ai:     x={decision.X} y={decision.Y} (image coords)");
                Console.WriteLine($"mapped: x={screenX} y={screenY} (screen coords)");
                Console.WriteLine($"center: x={bounds.Left + bounds.Width / 2} y={bounds.Top + bounds.Height / 2}");


                Console.WriteLine($"Vou clicar em {screenX},{screenY} no monitor principal. Confirmar? (y/n)");
                if (Console.ReadLine()?.Trim().ToLowerInvariant() != "y")
                {
                    Console.WriteLine("Cancelado.");
                    continue;
                }

                var ok = Clicker.TryLeftClickOnPrimaryScreen(screenX, screenY, bounds);
                Console.WriteLine(ok ? "Clicked." : "Blocked (fora do principal).");
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
