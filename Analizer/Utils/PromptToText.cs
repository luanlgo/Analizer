namespace Analizer.Utils
{
    public static class PromptToText
    {
        private static string ReplaceProperties(string template, Dictionary<string, string> vars)
        {
            foreach (var (key, value) in vars)
            {
                template = template.Replace($"{{{{{key}}}}}", value);
            }
            return template;
        }

        public static string RenderPrompt(string userCommand, int sentW, int sentH)
        {
            var template = File.ReadAllText("../../../Prompt/click_system.txt");

            return ReplaceProperties(template, new()
            {
                ["userCommand"] = userCommand,
                ["sentImageWidth"] = sentW.ToString(),
                ["sentImageHeight"] = sentH.ToString()
            });
        }
    }
}
