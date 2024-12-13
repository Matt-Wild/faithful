using R2API;
using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class BrassScrews : ItemBase
    {
        // Store item and buff
        Item brassScrewsItem;
        Buff brassScrewsBuff;

        // Store display settings
        ItemDisplaySettings displaySettings;

        // Store additional item settings
        Setting<float> damageSetting;
        Setting<float> damageStackingSetting;
        Setting<float> buffDurationSetting;

        // Store item stats
        float damage;
        float damageStacking;
        float buffDuration;

        // Constructor
        public BrassScrews(Toolbox _toolbox) : base(_toolbox)
        {
            // Create display settings
            CreateDisplaySettings("brassscrewsdisplaymesh");

            // Create Brass Screws item and buff
            brassScrewsItem = Items.AddItem("BRASS_SCREWS", [ItemTag.Damage, ItemTag.HoldoutZoneRelated], "texbrassscrewsicon", "brassscrewsmesh", ItemTier.VoidTier1, _simulacrumBanned: true, _corruptToken: "FAITHFUL_COPPER_GEAR_NAME", _displaySettings: displaySettings);
            brassScrewsBuff = Buffs.AddBuff("BRASS_SCREWS", "texbuffteleporterscrew", Color.white);

            // Create item settings
            CreateSettings();

            // Fetch item settings
            FetchSettings();

            // Add stats modification
            Behaviour.AddStatsMod(brassScrewsBuff, BrassScrewsStatsMod);

            // Link Holdout Zone behaviour
            Behaviour.AddInHoldoutZoneCallback(InHoldoutZone);
        }

        private void CreateDisplaySettings(string _displayMeshName)
        {
            // Create display settings
            displaySettings = Utils.CreateItemDisplaySettings(_displayMeshName);

            // Check for required asset
            if (!Assets.HasAsset(_displayMeshName))
            {
                return;
            }

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
            displaySettings.AddCharacterDisplay("Acrid", "LowerArmL", new Vector3(-0.793F, 3.70431F, 0.14379F), new Vector3(51.71926F, 146.3149F, 227.2395F), new Vector3(0.95F, 0.95F, 0.95F));
            displaySettings.AddCharacterDisplay("Acrid", "LowerArmR", new Vector3(0.7955F, 3.535F, 0.26625F), new Vector3(63.0825F, 231.25F, 159.35F), new Vector3(0.95F, 0.95F, 0.95F));
            displaySettings.AddCharacterDisplay("Captain", "MuzzleGun", new Vector3(-0.01064F, 0.01558F, -0.08823F), new Vector3(18.29989F, 139.6017F, 345.7164F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("Railgunner", "GunScope", new Vector3(-0.096F, -0.17875F, 0.1775F), new Vector3(316F, 305F, 160F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("Void Fiend", "CalfL", new Vector3(0.0146F, 0.3535F, -0.03525F), new Vector3(10F, 275F, 95F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("Scavenger", "Head", new Vector3(5.19109F, 2.45005F, -2.17756F), new Vector3(342.2899F, 335.8934F, 339.8638F), new Vector3(1F, 1F, 1F));
            displaySettings.AddCharacterDisplay("Seeker", "Chest", new Vector3(-0.03825F, 0.3525F, -0.0625F), new Vector3(5.5F, 196.5F, 250F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("False Son", "LowerArmL", new Vector3(-0.1165F, 0.3125F, -0.052F), new Vector3(340F, 0.5F, 70F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Chef", "OvenDoor", new Vector3(-0.04625F, 0.10875F, 0.03775F), new Vector3(65F, 190F, 150F), new Vector3(0.075F, 0.075F, 0.075F));
        }

        protected override void CreateSettings()
        {
            // Create settings specific to this item
            damageSetting = brassScrewsItem.CreateSetting("DAMAGE", "Damage", 20.0f, "How much should this item increase damage while within the teleporter radius? (20.0 = 20% increase)");
            damageStackingSetting = brassScrewsItem.CreateSetting("DAMAGE_STACKING", "Damage Stacking", 20.0f, "How much should further stacks of this item increase damage while within the teleporter radius? (20.0 = 20% increase)");
            buffDurationSetting = brassScrewsItem.CreateSetting("BUFF_DURATION", "Buff Duration", 1.0f, "How long should the buff be retained after leaving the teleporter radius? (1.0 = 1 second)");
        }

        public override void FetchSettings()
        {
            // Get item settings
            damage = damageSetting.Value / 100.0f;
            damageStacking = damageStackingSetting.Value / 100.0f;
            buffDuration = Mathf.Max(0.1f, buffDurationSetting.Value);

            // Update item texts with new settings
            brassScrewsItem.UpdateItemTexts();
        }

        void BrassScrewsStatsMod(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Check for buff
            if (_count == 0) return;

            // Modify damage
            _stats.damageMultAdd += damage + damageStacking * (_count - 1);
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
                    Utils.RefreshTimedBuffs(_body, brassScrewsBuff.buffDef, buffDuration);

                    // Get needed amount of buffs
                    int needed = copperGearCount - _body.GetBuffCount(brassScrewsBuff.buffDef);

                    // Catch up buff count
                    for (int i = 0; i < needed; i++)
                    {
                        // Add Brass Screws buff
                        _body.AddTimedBuff(brassScrewsBuff.buffDef, buffDuration);
                    }
                }
            }
        }
    }
}
