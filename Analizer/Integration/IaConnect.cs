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
            http.Timeout = TimeSpan.FromSeconds(30);

            // O Google exige camelCase. Sem isso, ele não acha o campo e dá erro 400.
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var jsonPayload = JsonSerializer.Serialize(payload, options);

            using var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            Console.WriteLine("Se demorar muito, baixe a qualidade da imagem enviada nas configuracoes do ambiente, variavel 'IMAGE_QUALITY'.");
            var retry = int.TryParse(Environment.GetEnvironmentVariable("RETRY_IA_CONNECTION"), out var v) ? v : 3;
            var text = string.Empty;
            for (int i = 0; i < retry; i++)
            {
                // Use v1beta para garantir que o 'response_mime_type' funcione
                var resp = await http.PostAsync(ia.Url, content);
                var body = await resp.Content.ReadAsStringAsync();
                
                if (resp.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(body);
                    text = doc.RootElement.GetProperty("candidates")[0]
                                              .GetProperty("content")
                                              .GetProperty("parts")[0]
                                              .GetProperty("text").GetString();
                }

                if ((int)resp.StatusCode == 503 || (int)resp.StatusCode == 429)
                {
                    Console.WriteLine($"Servidor ocupado (Tentativa {i + 1}). Aguardando 5s...");
                    await Task.Delay(5000); // Espera 5 segundos antes de tentar de novo
                    continue;
                }
            }
            var clickDecision = new ClickDecision();

            if (string.IsNullOrEmpty(text))
            {
                Console.WriteLine("Não foi possível obter uma resposta válida da IA após várias tentativas.");
                return clickDecision; // Retorna uma decisão vazia ou com valores padrão
            }

            try
            {
                return JsonSerializer.Deserialize<ClickDecision>(text, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? clickDecision;
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
