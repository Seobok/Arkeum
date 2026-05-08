using System.Collections.Generic;
using Arkeum.Production.Gameplay.Timing;
using UnityEngine;

namespace Arkeum.Production.Gameplay.Run
{
    [CreateAssetMenu(fileName = "WeaponDefinition", menuName = "Arkeum/Weapon Definition")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        [SerializeField] private string id = "weapon";
        [SerializeField] private string displayName = "Weapon";
        [SerializeField] private Sprite sprite;
        [SerializeField] private Color tint = new Color(0.75f, 0.43f, 0.18f);
        [SerializeField] private int attackBonus = 1;
        [SerializeField] private Vector2Int attackEditorMin = new Vector2Int(-3, -3);
        [SerializeField] private Vector2Int attackEditorMax = new Vector2Int(3, 3);
        [SerializeField] private bool rotateAttackByFacing = true;
        [SerializeField] private List<Vector2Int> attackOffsets = new List<Vector2Int>
        {
            Vector2Int.right,
        };
        [SerializeField] private List<WeaponEffectDefinition> effects = new List<WeaponEffectDefinition>();
        [SerializeField] private TimingChallengeDefinition timingChallenge;

        public string Id => id;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public Sprite Sprite => sprite;
        public Color Tint => tint;
        public int AttackBonus => attackBonus;
        public Vector2Int AttackEditorMin
        {
            get => attackEditorMin;
            set => attackEditorMin = value;
        }

        public Vector2Int AttackEditorMax
        {
            get => attackEditorMax;
            set => attackEditorMax = value;
        }

        public List<Vector2Int> AttackOffsets => attackOffsets;
        public IReadOnlyList<WeaponEffectDefinition> Effects => effects;
        public TimingChallengeDefinition TimingChallenge => timingChallenge;
        public bool HasTimingChallenge => timingChallenge != null;

        public bool RotateAttackByFacing
        {
            get => rotateAttackByFacing;
            set => rotateAttackByFacing = value;
        }

        public void ToggleAttackOffset(Vector2Int offset)
        {
            if (offset == Vector2Int.zero)
            {
                return;
            }

            if (attackOffsets.Contains(offset))
            {
                attackOffsets.RemoveAll(existing => existing == offset);
                return;
            }

            attackOffsets.Add(offset);
        }
    }
}
