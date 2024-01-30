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

        // Store display settings
        ItemDisplaySettings displaySettings;

        // Constructor
        public CopperGear(Toolbox _toolbox)
        {
            toolbox = _toolbox;

            // Create display settings
            CreateDisplaySettings();

            // Create Copper Gear item and buff
            copperGearItem = toolbox.items.AddItem("COPPER_GEAR", [ItemTag.Damage, ItemTag.HoldoutZoneRelated], "texcoppergearicon", "coppergearmesh", _simulacrumBanned: true, _displaySettings: displaySettings);
            copperGearBuff = toolbox.buffs.AddBuff("COPPER_GEAR", "texbuffteleportergear", Color.white);

            // Add stats modification
            toolbox.behaviour.AddStatsMod(copperGearBuff, CopperGearStatsMod);

            // Link Holdout Zone behaviour
            toolbox.behaviour.AddInHoldoutZoneCallback(InHoldoutZone);
        }

        private void CreateDisplaySettings()
        {
            // Create display settings
            displaySettings = toolbox.utils.CreateItemDisplaySettings("coppergeardisplaymesh");

            // Add character display settings
            displaySettings.AddCharacterDisplay("Commando", "LowerArmL", new Vector3(0.01898f, 0.26776f, 0.00182f), new Vector3(7.69423f, 1.2381f, 2.28152f), new Vector3(0.11f, 0.11f, 0.11f));
            //displaySettings.AddCharacterDisplay("Huntress", "Chest", new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f));
            //displaySettings.AddCharacterDisplay("Bandit", "Chest", new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f));
            //displaySettings.AddCharacterDisplay("MUL-T", "Chest", new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f));
            //displaySettings.AddCharacterDisplay("Engineer", "Chest", new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f));
            //displaySettings.AddCharacterDisplay("Turret", "Head", new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f));
            //displaySettings.AddCharacterDisplay("Artificer", "Chest", new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f));
            //displaySettings.AddCharacterDisplay("Mercenary", "Chest", new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f));
            //displaySettings.AddCharacterDisplay("REX", "CalfBackL", new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f));
            //displaySettings.AddCharacterDisplay("Loader", "Chest", new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f));
            //displaySettings.AddCharacterDisplay("Acrid", "Chest", new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f));
            //displaySettings.AddCharacterDisplay("Captain", "Chest", new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f));
            //displaySettings.AddCharacterDisplay("Railgunner", "Backpack", new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f));
            //displaySettings.AddCharacterDisplay("Void Fiend", "CalfR", new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f));
            //displaySettings.AddCharacterDisplay("Scavenger", "Weapon", new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f));
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
