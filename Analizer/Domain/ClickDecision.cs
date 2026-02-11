using Analizer.Domain.Enum;

namespace Analizer.Domain
{
    public sealed class ClickDecision
    {
        public string Action { get; set; } = string.Empty; // "click", "ask_confirmation", "none"
        public int X { get; set; }
        public int Y { get; set; }
        public double Confidence { get; set; } // 0..1
        public string Reason { get; set; } = string.Empty; // explicação da decisão, para feedback e aprendizado futuro
        public string Question { get; set; } = string.Empty; // se Action = "ask_confirmation", a pergunta a ser feita ao usuário

        public override string ToString()
        {
            return $"Action: {Action} - x: {X} - y: {Y} - Confidence: {Confidence} - Reason: {Reason} - Question: {Question}";
        }
    }
}

/*
    {
        "action": "click",
        "x": 854,
        "y": 108,
        "confidence": 0.9,
        "reason": "Clicar no botão de fechar (X) no canto superior direito da janela do Paint para encerrar o programa.",
        "question": null
    }
*/