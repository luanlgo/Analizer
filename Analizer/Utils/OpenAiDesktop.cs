using Analizer.Domain;
using Analizer.Integration;
using Analizer.Utils;
using System;
using System.IO;

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
        ia.SetSystemInstruction(PromptToText.RenderPrompt(userCommand, screenShoot.Width, screenShoot.Height));
        ia.SetUserCommand(userCommand);
        return ia;
    }

    private static object GeneratePayload(string userCommand, ScreenShoot ss)
    {
        var maxKb = int.TryParse(Environment.GetEnvironmentVariable("MAX_IMAGE_KB"), out var v) ? v : 100;
        var imageBytes = maxKb > 0 ? ss.ToBytesCompressed(maxKb) : ss.ToBytes();

        Console.WriteLine($"Payload image size: {imageBytes.Length / 1024.0:F2} KB (max {maxKb} KB)");

        // Salva a versão enviada (comprimida) em disco para inspeção / debugging
        var imagesDir = Environment.GetEnvironmentVariable("SENT_IMAGES_DIR") ?? @"..\";
        try
        {
            Directory.CreateDirectory(imagesDir);
            var fileName = $"sent_{DateTime.Now:yyyyMMdd_HHmmss_fff}.jpg";
            var fullPath = Path.Combine(imagesDir, fileName);
            File.WriteAllBytes(fullPath, imageBytes);
            Console.WriteLine($"Saved sent image: {fullPath} ({imageBytes.Length / 1024.0:F2} KB)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Não foi possível salvar a imagem enviada: {ex.Message}");
        }

        return new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new object[]
                    {
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
- Se action != ""click"", x e y devem ser null." }
                        ,
                        new { text = $"Tarefa: {userCommand}. Resolução da tela: {ss.Width}x{ss.Height}." },
                        new { inline_data = new { mime_type = "image/jpeg", data = Convert.ToBase64String(imageBytes) } }
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
