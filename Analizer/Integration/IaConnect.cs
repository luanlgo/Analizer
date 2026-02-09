using Analizer.Domain;
using System.Text;
using System.Text.Json;

namespace Analizer.Integration
{
    public static class IaConnect
    {
        public static async Task<ClickDecision> GetIaDecision(IA ia, object payload)
        {
            using var http = new HttpClient();
            var jsonPayload = JsonSerializer.Serialize(payload);
            using var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var resp = await http.PostAsync(ia.Url, content);
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
}
