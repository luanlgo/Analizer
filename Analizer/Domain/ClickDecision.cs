namespace Analizer.Domain
{
    public sealed class ClickDecision
    {
        public string Action { get; set; } = "ask_confirmation"; // "click" | "ask_confirmation" | "refuse"
        public int X { get; set; } // coordenadas na IMAGEM enviada
        public int Y { get; set; }
        public double Confidence { get; set; } // 0..1
        public string Reason { get; set; } = "";
        public string Question { get; set; } = ""; // usado quando ask_confirmation
    }

}
