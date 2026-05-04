using UnityEngine;

namespace Arkeum.Production.Gameplay.Run
{
    [CreateAssetMenu(fileName = "RunStartingLoadout", menuName = "Arkeum/Run Starting Loadout")]
    public sealed class RunStartingLoadoutDefinition : ScriptableObject
    {
        [SerializeField] private WeaponDefinition weapon;
        [SerializeField] private int bandageCount;

        public WeaponDefinition Weapon => weapon;
        public int BandageCount => Mathf.Max(0, bandageCount);
    }
}
