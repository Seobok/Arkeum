namespace Arkeum.Production.Gameplay.Run
{
    // 추후 행동 속도 차이, 버프/디퍼브 지속 턴 감소, 독/화상 같은 지속피해 등이 붙을 수 있음
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
