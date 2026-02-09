using System.Windows.Forms;

namespace Analizer.Domain
{
    public class IA
    {
        private string _url = string.Empty;
        public string Url 
        { 
            get => _url;
            set
            {
                if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(Key))
                    throw new Exception("API_KEY environment variable is not set or URL is invalid.");

                _url = value;
            }
        }
        public string Model { get; set; }

        private string _key = string.Empty;
        public string Key
        {
            get => _key;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new Exception("API_KEY environment variable is not set.");

                _key = value;
            }
        }

        public string UserCommand { get; set; } = string.Empty;

        public string SystemInstruction { get; set; } = string.Empty;

        public IA(string url, string model, string key)
        {
            Key = key;
            Url = url;
            Model = model;
        }

        public void SetUserCommand(string cmd)
        {
            UserCommand = cmd;
        }

        public void SetSystemInstruction(string systemInstruction)
        {
            SystemInstruction = systemInstruction;
        }
    }
}
