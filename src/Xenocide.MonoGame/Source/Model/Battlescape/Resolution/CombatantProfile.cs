using System;
using System.Globalization;

using ProjectXenocide.Model.Battlescape.Combatants;
using ProjectXenocide.Model.StaticData.Battlescape;
using ProjectXenocide.Model.StaticData.Items;

namespace ProjectXenocide.Model.Battlescape
{
    /// <summary>
    /// A read-only snapshot of the values the Strategic Engagement resolver needs
    /// from a <see cref="Combatant"/>.  Snapshots are used so the Monte-Carlo
    /// prediction can simulate without mutating the real game state.
    /// </summary>
    public sealed class CombatantProfile
    {
        /// <summary>The combatant this profile was built from (may be null for synthetic profiles).</summary>
        public Combatant Combatant { get; init; }

        public string Name { get; init; }
        public bool IsXCorp { get; init; }

        /// <summary>Base hit chance rating (front-loaded accuracy, injury adjusted).</summary>
        public int Accuracy { get; init; }

        public int Health { get; init; }
        public int Reactions { get; init; }
        public int Bravery { get; init; }

        /// <summary>Points of damage this unit's weapon does (0 if unarmed).</summary>
        public int Damage { get; init; }

        /// <summary>Armor protecting the front facing.</summary>
        public int ArmorFront { get; init; }

        /// <summary>Build a profile snapshot from a live combatant.</summary>
        public static CombatantProfile Build(Combatant combatant, bool isXCorp, int defaultDamage)
        {
            Item weapon = FindWeapon(combatant);
            int damage = (weapon?.DamageInfo != null) ? weapon.DamageInfo.Points : defaultDamage;

            return new CombatantProfile
            {
                Combatant = combatant,
                Name = string.Format(CultureInfo.InvariantCulture, "{0} {1}",
                    combatant.CombatantInfo.Race, combatant.CombatantInfo.Rank),
                IsXCorp = isXCorp,
                Accuracy = (int)combatant.Accuracy(Combatant.ActiveArm.Both),
                Health = combatant.Stats[Statistic.Health],
                Reactions = combatant.Stats[Statistic.Reactions],
                Bravery = combatant.Stats[Statistic.Bravery],
                Damage = Math.Max(0, damage),
                ArmorFront = combatant.Armor?.Plate(Armor.Side.Front) ?? 0,
            };
        }

        /// <summary>Snapshot with no backing combatant (used by tests).</summary>
        public static CombatantProfile Synthetic(string name, bool isXCorp, int accuracy, int health,
            int reactions, int damage, int armorFront, int bravery = 50)
        {
            return new CombatantProfile
            {
                Combatant = null,
                Name = name,
                IsXCorp = isXCorp,
                Accuracy = accuracy,
                Health = health,
                Reactions = reactions,
                Bravery = bravery,
                Damage = damage,
                ArmorFront = armorFront,
            };
        }

        private static Item FindWeapon(Combatant combatant)
        {
            Item right = combatant.Inventory.ItemAt(1, 0);
            Item left = combatant.Inventory.ItemAt(0, 0);
            if (right?.DamageInfo != null)
            {
                return right;
            }
            if (left?.DamageInfo != null)
            {
                return left;
            }
            return null;
        }
    }
}
