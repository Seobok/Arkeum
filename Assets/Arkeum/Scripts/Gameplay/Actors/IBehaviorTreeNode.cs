namespace Arkeum.Production.Gameplay.Actors
{
    public interface IBehaviorTreeNode
    {
        BehaviorTreeStatus Tick(EnemyBehaviorContext context);
    }
}
