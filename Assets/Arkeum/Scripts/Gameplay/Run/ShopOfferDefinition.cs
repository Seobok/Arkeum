using System;
using UnityEngine;

namespace Arkeum.Production.Gameplay.Run
{
    [Serializable]
    public sealed class ShopOfferDefinition
    {
        public Vector2Int Position;
        public WeaponDefinition Weapon;
        public int Price = 10;
        public string EffectSummary;

        public string DisplayName => Weapon != null ? Weapon.DisplayName : "Item";

        public string Summary
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(EffectSummary))
                {
                    return EffectSummary;
                }

                return Weapon != null ? $"+{Weapon.AttackBonus} attack" : "No effect";
            }
        }
    }
}
