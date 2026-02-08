using Analizer.Domain.Enum;

namespace Analizer.Domain
{
    public sealed class ClickDecision
    {
        private static readonly ActionType ACTION_DEFAULT = ActionType.AskConfirmation;

        public ActionType Action { get; set; } = ACTION_DEFAULT;
        public int X { get; set; }
        public int Y { get; set; }
        public double Confidence { get; set; } // 0..1
        public string Reason { get; set; } = "";
        public string Question { get; set; } = ""; // usado quando ask_confirmation
    }
}
