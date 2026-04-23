namespace Arkeum.Production.Gameplay.Actors
{
    public sealed class EnemyBehaviorTreeFactory
    {
        public IBehaviorTreeNode CreateDefaultTree()
        {
            return new BehaviorTreeSequenceNode(
                new BehaviorTreeActionNode(context => context.Actions.UpdateTarget(context)),
                new BehaviorTreeSelectorNode(
                    new BehaviorTreeSequenceNode(
                        new BehaviorTreeConditionNode(context => context.Actions.HasPendingAttack(context)),
                        new BehaviorTreeActionNode(context => context.Actions.AttackTarget(context))),
                    new BehaviorTreeSequenceNode(
                        new BehaviorTreeConditionNode(context => context.Actions.HasPendingMove(context)),
                        new BehaviorTreeActionNode(context => context.Actions.MoveToPreparedTarget(context))),
                    new BehaviorTreeSequenceNode(
                        new BehaviorTreeConditionNode(context => context.Actions.HasNoTarget(context)),
                        new BehaviorTreeActionNode(context => context.Actions.WanderMove(context))),
                    new BehaviorTreeSequenceNode(
                        new BehaviorTreeConditionNode(context => context.Actions.HasTarget(context)),
                        new BehaviorTreeActionNode(context => context.Actions.FaceTarget(context)),
                        new BehaviorTreeSelectorNode(
                            new BehaviorTreeSequenceNode(
                                new BehaviorTreeConditionNode(context => context.Actions.CanAttackTarget(context)),
                                new BehaviorTreeActionNode(context => context.Actions.AttackTarget(context))),
                            new BehaviorTreeActionNode(context => context.Actions.ChaseMove(context))))));
        }
    }
}
