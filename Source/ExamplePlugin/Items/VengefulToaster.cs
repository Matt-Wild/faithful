using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class VengefulToaster
    {
        // Toolbox
        protected Toolbox toolbox;

        // Store item and buff
        Buff vengeanceBuff;
        Item vengefulToasterItem;

        // Store display settings
        ItemDisplaySettings displaySettings;

        // Constructor
        public VengefulToaster(Toolbox _toolbox)
        {
            toolbox = _toolbox;

            // Get Vengeance buff
            vengeanceBuff = toolbox.buffs.GetBuff("VENGEANCE");

            // Create display settings
            CreateDisplaySettings();

            // Create Vengeful Toaster item
            vengefulToasterItem = toolbox.items.AddItem("VENGEFUL_TOASTER", [ItemTag.Damage, ItemTag.AIBlacklist], "texvengefultoastericon", "vengefultoastermesh", ItemTier.Tier2, _displaySettings: displaySettings);

            // Link On Damage Dealt behaviour
            toolbox.behaviour.AddOnDamageDealtCallback(OnDamageDealt);
        }

        private void CreateDisplaySettings()
        {
            // Create display settings
            displaySettings = toolbox.utils.CreateItemDisplaySettings("vengefultoasterdisplaymesh");

            // Add character display settings
            displaySettings.AddCharacterDisplay("Commando", "ThighR", new Vector3(-0.1738F, 0.0778F, 0.0148F), new Vector3(0F, 6F, 90F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Huntress", "ThighR", new Vector3(-0.105F, -0.065F, 0.02785F), new Vector3(347.5F, 334F, 117.5F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Bandit", "ThighR", new Vector3(-0.09275F, 0.4F, 0.0715F), new Vector3(0F, 35F, 90F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("MUL-T", "Chest", new Vector3(-1.75F, 2.875F, -1.31F), new Vector3(0F, 0F, 0F), new Vector3(0.75F, 0.75F, 0.75F));
            displaySettings.AddCharacterDisplay("Engineer", "CannonHeadR", new Vector3(0.055F, 0.24F, 0.2575F), new Vector3(270F, 180F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
            //displaySettings.AddCharacterDisplay("Turret", "Neck", new Vector3(0F, -0.3F, 0F), new Vector3(0F, 270F, 0F), new Vector3(0.3F, 0.3F, 0.3F));
            displaySettings.AddCharacterDisplay("Artificer", "Chest", new Vector3(-0.239F, 0.05525F, -0.2225F), new Vector3(9.5F, 0F, 90F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Mercenary", "ThighL", new Vector3(0.11575F, 0.13425F, -0.115F), new Vector3(2F, 45F, 260F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("REX", "Chest", new Vector3(0.48F, 0.425F, 0F), new Vector3(0F, 0F, 270F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Loader", "MechLowerArmL", new Vector3(0.0535F, 0.1525F, -0.1415F), new Vector3(352F, 90F, 270F), new Vector3(0.115F, 0.115F, 0.115F));
            displaySettings.AddCharacterDisplay("Acrid", "Chest", new Vector3(-1.83F, 3.05F, 3.025F), new Vector3(317.2F, 186.25F, 321F), new Vector3(1F, 1F, 1F));
            displaySettings.AddCharacterDisplay("Captain", "UpperArmL", new Vector3(0.0575F, 0.1375F, -0.1255F), new Vector3(0F, 61F, 265.25F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Railgunner", "Backpack", new Vector3(0.1315F, 0.461F, -0.02325F), new Vector3(0F, 180F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Void Fiend", "ForeArmR", new Vector3(0.082F, 0.27F, -0.191F), new Vector3(8F, 280F, 90F), new Vector3(0.1F, 0.1F, 0.1F));
        }

        void OnDamageDealt(DamageReport report)
        {
            // Check for inventory of victum
            Inventory inventory = report.victimBody.inventory;
            if (inventory)
            {
                // Get Vengeful Toaster amount
                int vengefulToasterCount = inventory.GetItemCount(vengefulToasterItem.itemDef.itemIndex);

                // Has Vengeful Toasters?
                if (vengefulToasterCount > 0)
                {
                    // Calculate buff duration
                    float buffDuration = vengefulToasterCount > 1 ? 4.0f + (1.0f * vengefulToasterCount - 1) : 4.0f;

                    // Add Vengeance buff
                    report.victimBody.AddTimedBuff(vengeanceBuff.buffDef, buffDuration);
                }
            }
        }
    }
}
