using System;
using System.Collections.Generic;
using UnityEngine;

namespace Arkeum.Production.Gameplay.Run
{
    [CreateAssetMenu(fileName = "ShopCatalog", menuName = "Arkeum/Shop Catalog")]
    public sealed class ShopCatalogDefinition : ScriptableObject
    {
        [SerializeField] private List<ShopCatalogEntry> entries = new List<ShopCatalogEntry>();

        public IReadOnlyList<ShopCatalogEntry> Entries => entries;
    }

    [Serializable]
    public sealed class ShopCatalogEntry
    {
        public WeaponDefinition Weapon;
        [Min(0)] public int Price = 10;
        [TextArea] public string Description;
    }
}
