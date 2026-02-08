using Analizer.Domain;
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
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Set GEMINI_API_KEY no ambiente.");

        // No Gemini, o endpoint inclui o nome do modelo e a sua chave
        var modelName = "gemini-1.5-flash";
        var url = $"https://generativelanguage.googleapis.com/v1/models/{modelName}:generateContent?key={apiKey}";

        var systemInstruction = """
        Você é um assistente de automação de desktop.
        Você NÃO executa nada. Você só propõe UMA ação de click (no máximo 1 click).
        
        Regras:
        - x,y devem ser coordenadas na imagem enviada (0..width-1, 0..height-1).
        - Confiança de 0 a 1.
        """;

        var userPrompt = $"""
        Comando do usuário: {userCommand}
        Dimensões da imagem enviada: {sentImageWidth}x{sentImageHeight}
        Diga onde clicar para cumprir o comando (1 click no máximo).
        """;

        var payload = new
        {
            system_instruction = new { parts = new[] { new { text = systemInstruction } } },
            contents = new[]
            {
            new
            {
                role = "user",
                parts = new object[]
                {
                    new { text = userPrompt },
                    new { inline_data = new { mime_type = "image/jpeg", data = Convert.ToBase64String(jpegBytes) } }
                }
            }
        },
            generationConfig = new
            {
                response_mime_type = "application/json", // Força o output a ser JSON puro
                temperature = 0.1 // Baixa temperatura para maior precisão em coordenadas
            }
        };

        using var http = new HttpClient();
        var jsonPayload = JsonSerializer.Serialize(payload);
        using var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var resp = await http.PostAsync(url, content);
        var body = await resp.Content.ReadAsStringAsync();

        if (!resp.IsSuccessStatusCode)
            throw new Exception($"Gemini error {(int)resp.StatusCode}: {body}");

        try
        {
            using var doc = JsonDocument.Parse(body);
            // O Gemini retorna o texto dentro de candidates[0].content.parts[0].text
            var outputText = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrWhiteSpace(outputText))
                throw new Exception("Resposta sem texto interpretável.");

            var decision = JsonSerializer.Deserialize<ClickDecision>(
                outputText,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return decision ?? throw new Exception("Decision null");
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro no parse do Gemini.\nRaw={body}", ex);
        }
    }
}
