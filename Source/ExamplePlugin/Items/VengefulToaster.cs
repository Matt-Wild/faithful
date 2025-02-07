using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class VengefulToaster : ItemBase
    {
        // Store item and buff
        Buff vengeanceBuff;
        Item vengefulToasterItem;

        // Store reference to vengeance behaviour
        Vengeance vengeanceBehaviour;

        // Store display settings
        ItemDisplaySettings displaySettings;

        // Store additional item settings
        Setting<float> durationSetting;
        Setting<float> durationStackingSetting;
        Setting<float> damageSetting;

        // Store item stats
        float duration;
        float durationStacking;

        // Constructor
        public VengefulToaster(Toolbox _toolbox, Vengeance _vengeance) : base(_toolbox)
        {
            // Assign vengeance behaviour
            vengeanceBehaviour = _vengeance;

            // Get Vengeance buff
            vengeanceBuff = Buffs.GetBuff("VENGEANCE");

            // Create display settings
            CreateDisplaySettings("vengefultoasterdisplaymesh");

            // Create Vengeful Toaster item
            vengefulToasterItem = Items.AddItem("VENGEFUL_TOASTER", [ItemTag.Damage, ItemTag.AIBlacklist], "texvengefultoastericon", "vengefultoastermesh", ItemTier.Tier2, _displaySettings: displaySettings);

            // Create item settings
            CreateSettings();

            // Fetch item settings
            FetchSettings();

            // Link On Damage Dealt behaviour
            Behaviour.AddOnDamageDealtCallback(OnDamageDealt);
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
            displaySettings.AddCharacterDisplay("Commando", "ThighR", new Vector3(-0.1738F, 0.0778F, 0.0148F), new Vector3(0F, 6F, 90F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Huntress", "ThighR", new Vector3(-0.105F, -0.065F, 0.02785F), new Vector3(347.5F, 334F, 117.5F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Bandit", "ThighR", new Vector3(-0.09275F, 0.4F, 0.0715F), new Vector3(0F, 35F, 90F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("MUL-T", "Chest", new Vector3(-1.75F, 2.875F, -1.31F), new Vector3(0F, 0F, 0F), new Vector3(0.75F, 0.75F, 0.75F));
            displaySettings.AddCharacterDisplay("Engineer", "CannonHeadR", new Vector3(0.055F, 0.24F, 0.2575F), new Vector3(270F, 180F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Turret", "Neck", new Vector3(0F, -0.3F, 0F), new Vector3(0F, 270F, 0F), new Vector3(0.3F, 0.3F, 0.3F));
            displaySettings.AddCharacterDisplay("Artificer", "Chest", new Vector3(-0.239F, 0.05525F, -0.2225F), new Vector3(9.5F, 0F, 90F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Mercenary", "ThighL", new Vector3(0.11575F, 0.13425F, -0.115F), new Vector3(2F, 45F, 260F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("REX", "Chest", new Vector3(0.48F, 0.425F, 0F), new Vector3(0F, 0F, 270F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Loader", "MechLowerArmL", new Vector3(0.0535F, 0.1525F, -0.1415F), new Vector3(352F, 90F, 270F), new Vector3(0.115F, 0.115F, 0.115F));
            displaySettings.AddCharacterDisplay("Acrid", "Chest", new Vector3(-1.83F, 3.05F, 3.025F), new Vector3(317.2F, 186.25F, 321F), new Vector3(1F, 1F, 1F));
            displaySettings.AddCharacterDisplay("Captain", "UpperArmL", new Vector3(0.0575F, 0.1375F, -0.1255F), new Vector3(0F, 61F, 265.25F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Railgunner", "Backpack", new Vector3(0.1315F, 0.461F, -0.02325F), new Vector3(0F, 180F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Void Fiend", "ForeArmR", new Vector3(0.082F, 0.27F, -0.191F), new Vector3(8F, 280F, 90F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Seeker", "Pack", new Vector3(-0.3675F, 0.012F, -0.21F), new Vector3(5F, 180F, 250F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("False Son", "Pelvis", new Vector3(0.20668F, 0.2025F, 0.021F), new Vector3(4.5F, 40.5F, 15.25F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Chef", "Chest", new Vector3(0.125F, -0.4F, -0.15F), new Vector3(85F, 12.5F, 180F), new Vector3(0.1F, 0.1F, 0.1F));
        }

        protected override void CreateSettings()
        {
            // Create settings specific to this item
            durationSetting = vengefulToasterItem.CreateSetting("DURATION", "Duration", 4.0f, "How long should the vengeance buff last when granted? (4.0 = 4 seconds)");
            durationStackingSetting = vengefulToasterItem.CreateSetting("DURATION_STACKING", "Duration Stacking", 1.0f, "How much longer should the vengeance buff last when granted with additional item stacks? (1.0 = 1 second)");
            damageSetting = vengefulToasterItem.CreateSetting("DAMAGE", "Damage", 75.0f, "How much should each stack of vengeance increase damage? (75.0 = 75% increase)");
        }

        public override void FetchSettings()
        {
            // Get item settings
            duration = durationSetting.Value;
            durationStacking = durationStackingSetting.Value;

            // Apply damage to buff
            vengeanceBehaviour.damage = damageSetting.Value / 100.0f;

            // Update item texts with new settings
            vengefulToasterItem.UpdateItemTexts();
        }

        void OnDamageDealt(DamageReport _report)
        {
            // Check for inventory of victim
            Inventory inventory = _report.victimBody.inventory;
            if (inventory)
            {
                // Get Vengeful Toaster amount
                int vengefulToasterCount = inventory.GetItemCount(vengefulToasterItem.itemDef.itemIndex);

                // Has Vengeful Toasters?
                if (vengefulToasterCount > 0)
                {
                    // Calculate buff duration
                    float buffDuration = vengefulToasterCount > 1 ? duration + (durationStacking * (vengefulToasterCount - 1)) : duration;

                    // Add Vengeance buff
                    _report.victimBody.AddTimedBuff(vengeanceBuff.buffDef, buffDuration);
                }
            }
        }
    }
}
