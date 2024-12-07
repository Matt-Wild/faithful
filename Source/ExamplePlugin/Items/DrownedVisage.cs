using RoR2;
using UnityEngine;
using System.Collections.Generic;

namespace Faithful
{
    internal class DrownedVisage
    {
        // Toolbox
        protected Toolbox toolbox;

        // Store item
        Item drownedVisageItem;

        // Store display settings
        ItemDisplaySettings displaySettings;

        // Store additional item settings
        Setting<float> chanceSetting;
        Setting<float> chanceStackingSetting;
        Setting<float> chargeAmountSetting;

        // Store item stats
        float chance;
        float chanceStacking;
        float chargeAmount;

        // Constructor
        public DrownedVisage(Toolbox _toolbox)
        {
            toolbox = _toolbox;

            // Create display settings
            CreateDisplaySettings("drownedvisagedisplaymesh");

            // Create Drowned Visage item
            drownedVisageItem = Items.AddItem("DROWNED_VISAGE", [ItemTag.Utility, ItemTag.AIBlacklist, ItemTag.HoldoutZoneRelated], "texdrownedvisageicon", "drownedvisagemesh", ItemTier.VoidTier2, _simulacrumBanned: true, _corruptToken: "FAITHFUL_SPACIOUS_UMBRELLA_NAME", _displaySettings: displaySettings);

            // Create item settings
            CreateSettings();

            // Fetch item settings
            FetchSettings();

            // Link On Character Death behaviour
            Behaviour.AddOnCharacterDeathCallback(OnCharacterDeath);
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
            displaySettings.AddCharacterDisplay("Commando", "Stomach", new Vector3(-0.15F, -0.015F, -0.13F), new Vector3(353F, 212F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Huntress", "Chest", new Vector3(0.143F, 0.1825F, -0.1175F), new Vector3(0F, 117.5F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Bandit", "Stomach", new Vector3(0.038F, -0.04785F, -0.188F), new Vector3(340F, 174.25F, 2F), new Vector3(0.11F, 0.11F, 0.11F));
            displaySettings.AddCharacterDisplay("MUL-T", "Chest", new Vector3(1.97F, 1.3F, -1.9F), new Vector3(0F, 180F, 0F), new Vector3(0.75F, 0.75F, 0.75F));
            displaySettings.AddCharacterDisplay("Engineer", "Chest", new Vector3(0.2415F, 0.18F, -0.2875F), new Vector3(0F, 180F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Turret", "Neck", new Vector3(0F, 0.3935F, 0.265F), new Vector3(355F, 0F, 0F), new Vector3(0.3F, 0.3F, 0.3F));
            displaySettings.AddCharacterDisplay("Artificer", "Chest", new Vector3(-0.1125F, 0.072F, -0.345F), new Vector3(340F, 180F, 1.5F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Mercenary", "Chest", new Vector3(0.08F, -0.07125F, -0.2285F), new Vector3(5F, 180F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("REX", "ClavicleL", new Vector3(0.2625F, 0.662F, -0.026F), new Vector3(288.5F, 270F, 270.5F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Loader", "MechBase", new Vector3(-0.1355F, 0.25F, -0.13F), new Vector3(0F, 180F, 0F), new Vector3(0.115F, 0.115F, 0.115F));
            displaySettings.AddCharacterDisplay("Acrid", "SpineChest1", new Vector3(-1.25F, 0.0635F, 3.5F), new Vector3(348.25F, 272.5F, 51.25F), new Vector3(1.1F, 1.1F, 1.1F));
            displaySettings.AddCharacterDisplay("Captain", "ThighR", new Vector3(-0.135F, 0.35F, 0.0564F), new Vector3(2.5F, 293.7499F, 187.5F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Railgunner", "Backpack", new Vector3(-0.091F, -0.1825F, -0.165F), new Vector3(355F, 180F, 0F), new Vector3(0.15F, 0.15F, 0.15F));
            displaySettings.AddCharacterDisplay("Void Fiend", "ForeArmL", new Vector3(0.11F, 0.1325F, 0.0155F), new Vector3(320F, 90F, 180F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Seeker", "Pack", new Vector3(-0.2475F, 0.1875F, -0.24F), new Vector3(330F, 275F, 35F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("False Son", "LowerArmR", new Vector3(-0.056F, 0.21F, 0.1325F), new Vector3(3.5F, 348.25F, 180F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("Chef", "OvenDoor", new Vector3(-0.51325F, -0.025F, 0.145F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
        }

        private void CreateSettings()
        {
            // Create settings specific to this item
            chanceSetting = drownedVisageItem.CreateSetting("CHANCE", "Charge Chance", 2.5f, "What should the chance be for this item to charge the teleporter when killing an enemy within the teleporter zone? (2.5 = 2.5% chance)");
            chanceStackingSetting = drownedVisageItem.CreateSetting("CHANCE_STACKING", "Charge Chance Stacking", 2.5f, "What should the additional stacking chance be for charging the teleporter when killing an enemy within the teleporter zone? (2.5 = 2.5% chance)");
            chargeAmountSetting = drownedVisageItem.CreateSetting("CHARGE_AMOUNT", "Charge Amount", 5.0f, "How much should this item charge the teleporter? (5.0 = 5% charge)");

            // Update item texts with new settings
            drownedVisageItem.UpdateItemTexts();
        }

        private void FetchSettings()
        {
            // Get item settings
            chance = chanceSetting.Value;
            chanceStacking = chanceStackingSetting.Value;
            chargeAmount = chargeAmountSetting.Value / 100.0f;
        }

        void OnCharacterDeath(DamageReport _report)
        {
            // Check for attacking character
            if (_report.attackerMaster == null)
            {
                return;
            }

            // Get attack character master
            CharacterMaster character = _report.attackerMaster;

            // Check attacker team
            if (character.teamIndex != TeamIndex.Player)
            {
                return;
            }

            // Check for body
            if (!character.hasBody)
            {
                return;
            }

            // Check for attacker inventory
            Inventory inventory = character.inventory;
            if (!inventory)
            {
                return;
            }

            // Get count for item
            int count = inventory.GetItemCount(drownedVisageItem.itemDef);

            // Check for item
            if (count == 0)
            {
                return;
            }

            // Roll the dice
            if (Util.CheckRoll(chance + (chanceStacking * (count - 1)), character))
            {
                // Get Holdout Zones containing character
                List<HoldoutZoneController> zones = Utils.GetHoldoutZonesContainingCharacter(character);

                // Cycle through Holdout Zones
                foreach (HoldoutZoneController zone in zones)
                {
                    // Add charge to zone
                    Utils.ChargeHoldoutZone(zone, chargeAmount);
                }
            }
        }
    }
}
