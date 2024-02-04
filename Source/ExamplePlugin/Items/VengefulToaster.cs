using R2API;
using RoR2;
using UnityEngine;
using static Facepunch.Steamworks.Inventory.Item;

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
            displaySettings.AddCharacterDisplay("Commando", "LowerArmL", new Vector3(0.01898F, 0.26776F, 0.00182F), new Vector3(7.69423F, 1.2381F, 2.28152F), new Vector3(0.11F, 0.11F, 0.11F));
            displaySettings.AddCharacterDisplay("Huntress", "Muzzle", new Vector3(0F, -0.02925F, -0.02537F), new Vector3(75F, 270F, 90F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("Bandit", "LowerArmL", new Vector3(0F, 0.1F, 0F), new Vector3(355F, 180F, 180F), new Vector3(0.11F, 0.11F, 0.11F));
            displaySettings.AddCharacterDisplay("MUL-T", "Head", new Vector3(0.39384F, 2.95909F, -1.01878F), new Vector3(0F, 270F, 35F), new Vector3(0.5F, 0.5F, 0.5F));
            displaySettings.AddCharacterDisplay("Engineer", "CannonHeadL", new Vector3(0.141F, 0.382F, 0.1435F), new Vector3(45F, 135F, 270F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Turret", "Neck", new Vector3(0F, -0.3F, 0F), new Vector3(0F, 270F, 0F), new Vector3(0.3F, 0.3F, 0.3F));
            displaySettings.AddCharacterDisplay("Artificer", "CalfR", new Vector3(0.00525F, 0.08556F, 0.03226F), new Vector3(10F, 0F, 355F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Mercenary", "LowerArmL", new Vector3(0.0085F, 0.152F, -0.0075F), new Vector3(0F, 18.25F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("REX", "FootFrontL", new Vector3(0F, -0.034F, 0F), new Vector3(0F, 0F, 270F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Loader", "MechLowerArmL", new Vector3(-0.0025F, 0.64F, -0.0055F), new Vector3(356F, 90F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Acrid", "Chest", new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f));
            displaySettings.AddCharacterDisplay("Captain", "Chest", new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f));
            displaySettings.AddCharacterDisplay("Railgunner", "Backpack", new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f));
            displaySettings.AddCharacterDisplay("Void Fiend", "CalfR", new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f));
            displaySettings.AddCharacterDisplay("Scavenger", "Weapon", new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f));
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
