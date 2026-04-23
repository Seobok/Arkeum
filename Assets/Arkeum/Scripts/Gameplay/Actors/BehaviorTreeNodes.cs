using System;
using System.Collections.Generic;

namespace Arkeum.Production.Gameplay.Actors
{
    public sealed class BehaviorTreeActionNode : IBehaviorTreeNode
    {
        private readonly Func<EnemyBehaviorContext, BehaviorTreeStatus> action;

        public BehaviorTreeActionNode(Func<EnemyBehaviorContext, BehaviorTreeStatus> action)
        {
            this.action = action;
        }

        public BehaviorTreeStatus Tick(EnemyBehaviorContext context)
        {
            return action(context);
        }
    }

    public sealed class BehaviorTreeConditionNode : IBehaviorTreeNode
    {
        private readonly Func<EnemyBehaviorContext, bool> condition;

        public BehaviorTreeConditionNode(Func<EnemyBehaviorContext, bool> condition)
        {
            this.condition = condition;
        }

        public BehaviorTreeStatus Tick(EnemyBehaviorContext context)
        {
            return condition(context) ? BehaviorTreeStatus.Success : BehaviorTreeStatus.Failure;
        }
    }

    public sealed class BehaviorTreeSequenceNode : IBehaviorTreeNode
    {
        private readonly IReadOnlyList<IBehaviorTreeNode> children;

        public BehaviorTreeSequenceNode(params IBehaviorTreeNode[] children)
        {
            this.children = children;
        }

        public BehaviorTreeStatus Tick(EnemyBehaviorContext context)
        {
            for (int i = 0; i < children.Count; i++)
            {
                BehaviorTreeStatus status = children[i].Tick(context);
                if (status != BehaviorTreeStatus.Success)
                {
                    return status;
                }
            }

            return BehaviorTreeStatus.Success;
        }
    }

    public sealed class BehaviorTreeSelectorNode : IBehaviorTreeNode
    {
        private readonly IReadOnlyList<IBehaviorTreeNode> children;

        public BehaviorTreeSelectorNode(params IBehaviorTreeNode[] children)
        {
            this.children = children;
        }

        public BehaviorTreeStatus Tick(EnemyBehaviorContext context)
        {
            for (int i = 0; i < children.Count; i++)
            {
                BehaviorTreeStatus status = children[i].Tick(context);
                if (status != BehaviorTreeStatus.Failure)
                {
                    return status;
                }
            }

            return BehaviorTreeStatus.Failure;
        }
    }
}
