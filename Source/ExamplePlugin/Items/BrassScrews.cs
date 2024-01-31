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
            displaySettings = toolbox.utils.CreateItemDisplaySettings("brassscrewsdisplaymesh");

            // Add character display settings
            displaySettings.AddCharacterDisplay("Commando", "LowerArmL", new Vector3(0.02264F, 0.16791F, -0.03632F), new Vector3(335.4802F, 67.51034F, 272.0254F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Huntress", "LowerArmL", new Vector3(0.00565F, 0.10005F, -0.02699F), new Vector3(325.6556F, 67.49967F, 283.802F), new Vector3(0.06F, 0.06F, 0.06F));
            displaySettings.AddCharacterDisplay("Bandit", "LowerArmL", new Vector3(-0.0025F, 0.0835F, -0.0475F), new Vector3(340F, 90F, 270F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("MUL-T", "Head", new Vector3(1.75763F, 3.26321F, 0.01843F), new Vector3(315F, 220F, 60F), new Vector3(0.45F, 0.45F, 0.45F));
            displaySettings.AddCharacterDisplay("Engineer", "CannonHeadL", new Vector3(0.11935F, 0.38362F, 0.162F), new Vector3(326.5692F, 291.2021F, 269.7665F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Turret", "Neck", new Vector3(0F, -0.43863F, -0.20092F), new Vector3(290F, 0F, 0F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Artificer", "CalfR", new Vector3(0.02813F, 0.05546F, 0.01644F), new Vector3(11.75F, 168.75F, 88F), new Vector3(0.06F, 0.06F, 0.06F));
            displaySettings.AddCharacterDisplay("Mercenary", "LowerArmL", new Vector3(-0.02268F, 0.12083F, -0.031F), new Vector3(338.8F, 144.5F, 267.8F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("REX", "FootFrontL", new Vector3(0.04454F, 0.20025F, -0.03737F), new Vector3(340F, 40F, 270F), new Vector3(0.15F, 0.15F, 0.15F));
            displaySettings.AddCharacterDisplay("Loader", "MechLowerArmL", new Vector3(0.0042F, 0.53245F, -0.11887F), new Vector3(48.68909F, 51.01041F, 227.7835F), new Vector3(0.095F, 0.095F, 0.095F));
            displaySettings.AddCharacterDisplay("Acrid", "Chest", new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f));
            displaySettings.AddCharacterDisplay("Captain", "Chest", new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f));
            displaySettings.AddCharacterDisplay("Railgunner", "Backpack", new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f));
            displaySettings.AddCharacterDisplay("Void Fiend", "CalfR", new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f));
            displaySettings.AddCharacterDisplay("Scavenger", "Weapon", new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f));
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
