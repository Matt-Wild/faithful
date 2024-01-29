using R2API;
using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class CopperGear
    {
        // Toolbox
        protected Toolbox toolbox;

        // Store item and buff
        Item copperGearItem;
        Buff copperGearBuff;

        // Constructor
        public CopperGear(Toolbox _toolbox)
        {
            toolbox = _toolbox;

            // Create Copper Gear item and buff
            copperGearItem = toolbox.items.AddItem("COPPER_GEAR", [ItemTag.Damage, ItemTag.HoldoutZoneRelated], "texcoppergearicon", "coppergearmesh", _simulacrumBanned: true);
            copperGearBuff = toolbox.buffs.AddBuff("COPPER_GEAR", "texbuffteleportergear", Color.yellow);

            // Add stats modification
            toolbox.behaviour.AddStatsMod(copperGearBuff, CopperGearStatsMod);

            // Link Holdout Zone behaviour
            toolbox.behaviour.AddInHoldoutZoneCallback(InHoldoutZone);
        }

        void CopperGearStatsMod(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Modify attack speed
            _stats.attackSpeedMultAdd += 0.25f * _count;
        }

        void InHoldoutZone(CharacterBody _body, HoldoutZoneController _zone)
        {
            // Check for inventory
            Inventory inventory = _body.inventory;
            if (inventory)
            {
                // Get Copper Gear amount
                int copperGearCount = inventory.GetItemCount(copperGearItem.itemDef.itemIndex);

                // Has Copper Gears?
                if (copperGearCount > 0)
                {
                    // Refresh Copper Gear buffs
                    toolbox.utils.RefreshTimedBuffs(_body, copperGearBuff.buffDef, 1);

                    // Get needed amount of buffs
                    int needed = copperGearCount - _body.GetBuffCount(copperGearBuff.buffDef);

                    // Catch up buff count
                    for (int i = 0; i < needed; i++)
                    {
                        // Add Copper Gear buff
                        _body.AddTimedBuff(copperGearBuff.buffDef, 1);
                    }
                }
            }
        }
    }
}
