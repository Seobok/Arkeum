using Arkeum.Production.Gameplay.Run;

namespace Arkeum.Production.Gameplay.Interaction
{
    public readonly struct InteractionResolution
    {
        public readonly bool Handled;
        public readonly bool ConsumesTurn;
        public readonly string Message;
        public readonly RunEndReason EndReason;

        public InteractionResolution(bool handled, bool consumesTurn, string message, RunEndReason endReason)
        {
            Handled = handled;
            ConsumesTurn = consumesTurn;
            Message = message;
            EndReason = endReason;
        }

        public static InteractionResolution Unhandled => new InteractionResolution(false, false, null, RunEndReason.None);
        public static InteractionResolution HandledWithoutTurn(string message = null)
        {
            return new InteractionResolution(true, false, message, RunEndReason.None);
        }
    }
}
