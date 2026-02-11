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

            // O Google exige camelCase. Sem isso, ele não acha o campo e dá erro 400.
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var jsonPayload = JsonSerializer.Serialize(payload, options);

            using var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // Use v1beta para garantir que o 'response_mime_type' funcione
            var resp = await http.PostAsync(ia.Url, content);
            var body = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                // Isso aqui vai cuspir o erro real se falhar
                Console.WriteLine($"DEBUG GOOGLE: {body}");
                throw new Exception($"Erro {resp.StatusCode}");
            }

            using var doc = JsonDocument.Parse(body);
            var text = doc.RootElement.GetProperty("candidates")[0]
                                      .GetProperty("content")
                                      .GetProperty("parts")[0]
                                      .GetProperty("text").GetString();

            var clickDecision = new ClickDecision();
            try
            {
                return JsonSerializer.Deserialize<ClickDecision>(text, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception e)
            {
                Console.WriteLine(text); // Loga o texto bruto para ajudar no debug
                Console.WriteLine($"Erro ao desserializar resposta da IA: {e.Message}");
            }
            return clickDecision;
        }
    }
}
