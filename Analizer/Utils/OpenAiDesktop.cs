using Analizer.Domain;
using Analizer.Utils;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public static class OpenAiDesktop
{
    public static async Task<ClickDecision> GetClickDecisionAsync(
        string userCommand,
        byte[] jpegBytes,
        int sentImageWidth,
        int sentImageHeight)
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Set OPENAI_API_KEY (launchSettings.json).");

        var dataUrl = $"data:image/jpeg;base64,{Convert.ToBase64String(jpegBytes)}";

        // Prompt bem restritivo: 1 click apenas, coords na imagem enviada
        var system = """
Você é um assistente de automação de desktop.
Você NÃO executa nada. Você só propõe UMA ação de click (no máximo 1 click).
Responda SOMENTE com JSON válido (sem markdown).

Formato:
- Se tiver certeza razoável: {"action":"click","x":123,"y":456,"confidence":0.0,"reason":"..."}
- Se estiver incerto/ambíguo: {"action":"ask_confirmation","question":"...","confidence":0.0,"reason":"..."}
- Se for inseguro: {"action":"refuse","confidence":0.0,"reason":"..."}
Regras:
- x,y devem ser coordenadas na imagem enviada (0..width-1, 0..height-1).
- Confiança de 0 a 1.
""";

        var user = $"""
Comando do usuário: {userCommand}
Dimensões da imagem enviada: {sentImageWidth}x{sentImageHeight}

Diga onde clicar para cumprir o comando (1 click no máximo).
""";

        var payload = new
        {
            model = "gpt-4.1-mini",
            input = new object[]
            {
                new {
                    role="system",
                    content = new object[]{ new { type="input_text", text=system } }
                },
                new {
                    role="user",
                    content = new object[]
                    {
                        new { type="input_text", text=user },
                        new { type="input_image", image_url=dataUrl }
                    }
                }
            },
            // Dica: se quiser não armazenar requests no servidor, use store=false
            store = false
        };

        using var http = new HttpClient();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var resp = await http.PostAsync("https://api.openai.com/v1/responses", content);
        var body = await resp.Content.ReadAsStringAsync();

        if (!resp.IsSuccessStatusCode)
            throw new Exception($"OpenAI error {(int)resp.StatusCode}: {body}");

        // output_text é um "atalho" que agrega texto (quando presente) :contentReference[oaicite:2]{index=2}
        using var doc = JsonDocument.Parse(body);
        var outputText = TextToResponse.ExtractTextFromResponsesApi(doc);

        if (string.IsNullOrWhiteSpace(outputText))
            throw new Exception("Resposta sem texto interpretável. Veja body bruto.");

        // Parse do JSON que o modelo devolveu
        try
        {
            var decision = JsonSerializer.Deserialize<ClickDecision>(
                outputText,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (decision is null) throw new Exception("decision null");
            return decision;
        }
        catch (Exception ex)
        {
            throw new Exception($"Não consegui parsear JSON do modelo.\noutput_text={outputText}\n\nRaw={body}", ex);
        }
    }
}
