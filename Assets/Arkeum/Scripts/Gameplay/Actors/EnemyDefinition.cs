using UnityEngine;

namespace Arkeum.Production.Gameplay.Actors
{
    [CreateAssetMenu(fileName = "EnemyDefinition", menuName = "Arkeum/Enemy Definition")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        [SerializeField] private string enemyId = "enemy";
        [SerializeField] private string displayName = "Enemy";
        [SerializeField] private BrainType brainType = BrainType.Chaser;
        [SerializeField] private ActorStats stats = new ActorStats();
        [SerializeField] private int bloodReward = 1;
        [SerializeField] private EnemyAttackPatternDefinition attackPattern;

        public string EnemyId => enemyId;
        public string DisplayName => displayName;
        public BrainType BrainType => brainType;
        public ActorStats Stats => stats;
        public int BloodReward => bloodReward;
        public EnemyAttackPatternDefinition AttackPattern => attackPattern;
    }
}
