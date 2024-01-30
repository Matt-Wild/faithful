using R2API;
using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class BrassScrews
    {
        // Toolbox
        protected Toolbox toolbox;

        // Store item and buff
        Item brassScrewsItem;
        Buff brassScrewsBuff;

        // Store display settings
        ItemDisplaySettings displaySettings;

        // Constructor
        public BrassScrews(Toolbox _toolbox)
        {
            toolbox = _toolbox;

            // Create display settings
            CreateDisplaySettings();

            // Create Brass Screws item and buff
            brassScrewsItem = toolbox.items.AddItem("BRASS_SCREWS", [ItemTag.Damage, ItemTag.HoldoutZoneRelated], "texbrassscrewsicon", "brassscrewsmesh", ItemTier.VoidTier1, _simulacrumBanned: true, _corruptToken: "FAITHFUL_COPPER_GEAR_NAME", _displaySettings: displaySettings);
            brassScrewsBuff = toolbox.buffs.AddBuff("BRASS_SCREWS", "texbuffteleporterscrew", Color.white);

            // Add stats modification
            toolbox.behaviour.AddStatsMod(brassScrewsBuff, BrassScrewsStatsMod);

            // Link Holdout Zone behaviour
            toolbox.behaviour.AddInHoldoutZoneCallback(InHoldoutZone);
        }

        private void CreateDisplaySettings()
        {
            // Create display settings
            displaySettings = toolbox.utils.CreateItemDisplaySettings("coppergeardisplaymesh");

            // Add character display settings
            //displaySettings.AddCharacterDisplay("Commando", "LowerArmL", new Vector3(0.01898f, 0.26776f, 0.00182f), new Vector3(7.69423f, 1.2381f, 2.28152f), new Vector3(0.11f, 0.11f, 0.11f));
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

        void BrassScrewsStatsMod(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Modify damage
            _stats.damageMultAdd += 0.20f * _count;
        }

        void InHoldoutZone(CharacterBody _body, HoldoutZoneController _zone)
        {
            // Check for inventory
            Inventory inventory = _body.inventory;
            if (inventory)
            {
                // Get Brass Screws amount
                int copperGearCount = inventory.GetItemCount(brassScrewsItem.itemDef.itemIndex);

                // Has Brass Screws?
                if (copperGearCount > 0)
                {
                    // Refresh Brass Screws buffs
                    toolbox.utils.RefreshTimedBuffs(_body, brassScrewsBuff.buffDef, 1);

                    // Get needed amount of buffs
                    int needed = copperGearCount - _body.GetBuffCount(brassScrewsBuff.buffDef);

                    // Catch up buff count
                    for (int i = 0; i < needed; i++)
                    {
                        // Add Brass Screws buff
                        _body.AddTimedBuff(brassScrewsBuff.buffDef, 1);
                    }
                }
            }
        }
    }
}
