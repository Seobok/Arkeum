using UnityEngine;

namespace Arkeum.Production.Gameplay.Actors
{
    [CreateAssetMenu(fileName = "EnemyDefinition", menuName = "Arkeum/Enemy Definition")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        [SerializeField] private string enemyId = "enemy";
        [SerializeField] private string displayName = "Enemy";
        [SerializeField] private ActorStats stats = new ActorStats();
        [SerializeField] private int bloodReward = 1;
        [SerializeField] private EnemyAttackPatternDefinition attackPattern;
        [Header("Boss")]
        [SerializeField] private bool isBoss;
        [SerializeField, Min(1)] private int spaceCutInterval = 7;
        [SerializeField, Min(1)] private int spaceCutWallDuration = 2;
        [SerializeField, Min(0)] private int chargeStunDuration = 3;
        [Header("Visuals")]
        [SerializeField] private Sprite sprite;
        [SerializeField] private Color tint = Color.white;
        [SerializeField] private Sprite[] idleFrames;
        [SerializeField, Min(1f)] private float idleFrameRate = 8f;

        public string EnemyId => enemyId;
        public string DisplayName => displayName;
        public ActorStats Stats => stats;
        public int BloodReward => bloodReward;
        public EnemyAttackPatternDefinition AttackPattern => attackPattern;
        public bool IsBoss => isBoss;
        public int SpaceCutInterval => Mathf.Max(1, spaceCutInterval);
        public int SpaceCutWallDuration => Mathf.Max(1, spaceCutWallDuration);
        public int ChargeStunDuration => Mathf.Max(0, chargeStunDuration);
        public Sprite Sprite => sprite;
        public Color Tint => tint;
        public Sprite[] IdleFrames => idleFrames;
        public float IdleFrameRate => Mathf.Max(1f, idleFrameRate);
    }
}
