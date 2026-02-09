using Analizer.Domain;
using Analizer.Integration;
using Analizer.Utils;

public static class OpenAiDesktop
{
    public static async Task<ClickDecision> GetClickDecisionAsync(string userCommand, ScreenShoot screenShoot)
    {
        var ia = GenerateIa(userCommand, screenShoot);
        var payload = GeneratePayload(ia, userCommand, screenShoot);

        return await IaConnect.GetIaDecision(ia, payload);
    }

    private static IA GenerateIa(string userCommand, ScreenShoot screenShoot)
    {
        var key = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? "";
        var model = "gemini-1.5-flash";
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={key}";
        var ia = new IA(url, model, key);
        ia.SetSystemInstruction(PromptToText.RenderPrompt(userCommand, screenShoot.GetWidthToSend(), screenShoot.GetHeightToSend()));
        ia.SetUserCommand(userCommand);
        return ia;
    }

    private static object GeneratePayload(IA ia, string userCommand, ScreenShoot ss)
    {
        return new
        {
            system_instruction = new { parts = new[] { new { text = ia.SystemInstruction } } },
            contents = new[]
            {
            new
            {
                role = "user",
                parts = new object[]
                {
                    new { text = ia.UserCommand },
                    new { inline_data = new { mime_type = "image/jpeg", data = Convert.ToBase64String(ss.ToBytes()) } }
                }
            }
        },
            generationConfig = new
            {
                response_mime_type = "application/json", // Força o output a ser JSON puro
                temperature = 0.1 // Baixa temperatura para maior precisão em coordenadas
            }
        };
    }
}
