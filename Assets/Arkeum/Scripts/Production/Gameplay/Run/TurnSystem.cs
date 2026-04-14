namespace Arkeum.Production.Gameplay.Run
{
    public sealed class TurnSystem
    {
        public void ConsumePlayerAction(RunState runState)
        {
            if (runState == null)
            {
                return;
            }

            runState.TurnCount += 1;
        }
    }
}
