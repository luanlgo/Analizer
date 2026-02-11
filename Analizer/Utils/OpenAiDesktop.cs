using Analizer.Domain;
using Analizer.Integration;
using Analizer.Utils;

public static class OpenAiDesktop
{
    public static async Task<ClickDecision> GetClickDecisionAsync(string userCommand, ScreenShoot screenShoot)
    {
        var ia = GenerateIa(userCommand, screenShoot);
        var payload = GeneratePayload(userCommand, screenShoot);

        return await IaConnect.GetIaDecision(ia, payload);
    }

    private static IA GenerateIa(string userCommand, ScreenShoot screenShoot)
    {
        var key = (Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? "").Trim();
        var model = "gemini-3-flash-preview";
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={key}";

        var ia = new IA(url, model, key);
        ia.SetSystemInstruction(PromptToText.RenderPrompt(userCommand, screenShoot.GetWidthToSend(), screenShoot.GetHeightToSend()));
        ia.SetUserCommand(userCommand);
        return ia;
    }

    private static object GeneratePayload(string userCommand, ScreenShoot ss)
    {
        return new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new object[]
                    {
                        // fazer o mapping de formato dinamicamente se precisar de outro formato no futuro
                        new { text =
@"Você é um robô de automação. Analise a imagem e responda APENAS com JSON (sem markdown).

Retorne EXATAMENTE no formato:
{
  ""action"": ""click"" | ""ask_confirmation"" | ""none"",
  ""x"": number | null,
  ""y"": number | null,
  ""confidence"": number, 
  ""reason"": string,
  ""question"": string | null
}

Regras:
- x,y são coordenadas NA IMAGEM (0..width-1, 0..height-1).
- Se action = ""ask_confirmation"", preencha ""question"".
- Se action != ""ask_confirmation"", question deve ser null.
- Se action != ""click"", x e y devem ser null." 
                        },
                        new { text = $"Tarefa: {userCommand}. Resolução da tela: {ss.GetWidthToSend()}x{ss.GetHeightToSend()}." },
                        new { inline_data = new { mime_type = "image/jpeg", data = Convert.ToBase64String(ss.ToBytes()) } }
                    }
                }
            },
            generationConfig = new
            {
                responseMimeType = "application/json",
                temperature = 0.1
            }
        };
    }
}
