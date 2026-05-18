using UnityEngine;

namespace Arkeum.Production.Presentation.World
{
    [CreateAssetMenu(fileName = "WorldVisualSet", menuName = "Arkeum/World Visual Set")]
    public sealed class WorldVisualSet : ScriptableObject
    {
        [Header("Tiles")]
        [SerializeField] private Sprite floorSprite;
        [SerializeField] private Color floorTint = new Color(0.16f, 0.13f, 0.14f);
        [SerializeField] private Sprite wallSprite;
        [SerializeField] private Color wallTint = new Color(0.08f, 0.08f, 0.09f);

        [Header("Actors")]
        [SerializeField] private Sprite playerSprite;
        [SerializeField] private Color playerTint = new Color(0.91f, 0.86f, 0.78f);
        [SerializeField] private Sprite defaultEnemySprite;
        [SerializeField] private Color defaultEnemyTint = new Color(0.63f, 0.25f, 0.21f);

        [Header("Interactables")]
        [SerializeField] private Sprite defaultWeaponSprite;
        [SerializeField] private Color defaultWeaponTint = new Color(0.75f, 0.43f, 0.18f);
        [SerializeField] private Sprite floorExitSprite;
        [SerializeField] private Color floorExitTint = new Color(0.76f, 0.65f, 0.17f);
        [SerializeField] private Sprite dungeonEntranceSprite;
        [SerializeField] private Color dungeonEntranceTint = new Color(0.62f, 0.29f, 0.22f);

        [Header("Markers")]
        [SerializeField] private Sprite enemyAttackMarkerSprite;
        [SerializeField] private Color enemyAttackMarkerTint = new Color(0.82f, 0.16f, 0.13f);
        [SerializeField] private Sprite enemyMoveMarkerSprite;
        [SerializeField] private Color enemyMoveMarkerTint = new Color(0.18f, 0.68f, 0.26f);

        public Sprite FloorSprite => floorSprite;
        public Color FloorTint => floorTint;
        public Sprite WallSprite => wallSprite;
        public Color WallTint => wallTint;
        public Sprite PlayerSprite => playerSprite;
        public Color PlayerTint => playerTint;
        public Sprite DefaultEnemySprite => defaultEnemySprite;
        public Color DefaultEnemyTint => defaultEnemyTint;
        public Sprite DefaultWeaponSprite => defaultWeaponSprite;
        public Color DefaultWeaponTint => defaultWeaponTint;
        public Sprite FloorExitSprite => floorExitSprite;
        public Color FloorExitTint => floorExitTint;
        public Sprite DungeonEntranceSprite => dungeonEntranceSprite;
        public Color DungeonEntranceTint => dungeonEntranceTint;
        public Sprite EnemyAttackMarkerSprite => enemyAttackMarkerSprite;
        public Color EnemyAttackMarkerTint => enemyAttackMarkerTint;
        public Sprite EnemyMoveMarkerSprite => enemyMoveMarkerSprite;
        public Color EnemyMoveMarkerTint => enemyMoveMarkerTint;
    }
}
