using System.Text;
using System.Text.Json;

namespace Analizer.Utils
{
    public static class TextToResponse
    {
        public static string? ExtractTextFromResponsesApi(JsonDocument doc)
        {
            // 1) tenta o atalho (quando existir)
            if (doc.RootElement.TryGetProperty("output_text", out var ot) &&
                ot.ValueKind == JsonValueKind.String)
            {
                return ot.GetString();
            }

            // 2) fallback padrão (output[].content[].text)
            if (!doc.RootElement.TryGetProperty("output", out var output) ||
                output.ValueKind != JsonValueKind.Array)
                return null;

            var sb = new StringBuilder();

            foreach (var item in output.EnumerateArray())
            {
                if (!item.TryGetProperty("content", out var content) ||
                    content.ValueKind != JsonValueKind.Array)
                    continue;

                foreach (var c in content.EnumerateArray())
                {
                    if (c.TryGetProperty("type", out var type) &&
                        type.GetString() == "output_text" &&
                        c.TryGetProperty("text", out var text))
                    {
                        sb.Append(text.GetString());
                    }
                }
            }

            return sb.Length > 0 ? sb.ToString() : null;
        }
    }
}
