using R2API;
using RoR2;
using System;
using UnityEngine;

namespace Faithful
{
    internal class SpaciousUmbrella : ItemBase
    {
        // Store item
        Item spaciousUmbrellaItem;

        // Store display settings
        ItemDisplaySettings displaySettings;

        // Store additional item settings
        Setting<float> sizeSetting;
        Setting<float> sizeStackingSetting;

        // Store item stats
        float size;
        float sizeStacking;

        // Constructor
        public SpaciousUmbrella(Toolbox _toolbox) : base(_toolbox)
        {
            // Create display settings
            CreateDisplaySettings("spaciousumbrelladisplaymesh");

            // Create Copper Gear item and buff
            spaciousUmbrellaItem = Items.AddItem("SPACIOUS_UMBRELLA", [ItemTag.Utility, ItemTag.AIBlacklist, ItemTag.CannotCopy, ItemTag.HoldoutZoneRelated], "texspaciousumbrellaicon", "spaciousumbrellamesh", ItemTier.Tier2, _simulacrumBanned: true, _displaySettings: displaySettings);

            // Create item settings
            CreateSettings();

            // Fetch item settings
            FetchSettings();

            // Link Holdout Zone behaviour
            Behaviour.AddOnHoldoutZoneCalcRadiusCallback(OnHoldoutZoneCalcRadius);
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
            displaySettings.AddCharacterDisplay("Commando", "ThighL", new Vector3(0.1225F, 0.14F, 0.0375F), new Vector3(6.735F, 231.25F, 212F), new Vector3(0.35F, 0.35F, 0.35F));
            displaySettings.AddCharacterDisplay("Huntress", "Chest", new Vector3(0.06675F, 0.03375F, -0.1355F), new Vector3(329.5F, 206.5F, 36.6F), new Vector3(0.4F, 0.4F, 0.4F));
            displaySettings.AddCharacterDisplay("Bandit", "Stomach", new Vector3(0.30375F, -0.152F, -0.0085F), new Vector3(3.315F, 24.115F, 40.9545F), new Vector3(0.3F, 0.3F, 0.3F));
            displaySettings.AddCharacterDisplay("MUL-T", "Chest", new Vector3(1.925F, 0.2125F, -1.5F), new Vector3(0F, 90F, 0F), new Vector3(2.5F, 2.5F, 2.5F));
            displaySettings.AddCharacterDisplay("Engineer", "Chest", new Vector3(0.1825F, 0.2235F, -0.245F), new Vector3(357.5F, 180F, 1.5F), new Vector3(0.3F, 0.3F, 0.3F));
            displaySettings.AddCharacterDisplay("Artificer", "Chest", new Vector3(-0.125F, 0.3025F, -0.195F), new Vector3(2.5F, 90F, 0F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Mercenary", "Chest", new Vector3(0.1275F, 0.025F, -0.23F), new Vector3(6.5F, 270F, 10F), new Vector3(0.35F, 0.35F, 0.35F));
            displaySettings.AddCharacterDisplay("REX", "LowerArmL", new Vector3(-0.08F, 0.32F, 0.03F), new Vector3(2F, 270F, 10F), new Vector3(0.5F, 0.5F, 0.5F));
            displaySettings.AddCharacterDisplay("Loader", "MechBase", new Vector3(-0.1355F, 0.0775F, -0.175F), new Vector3(0F, 90F, 12.5F), new Vector3(0.4F, 0.4F, 0.4F));
            displaySettings.AddCharacterDisplay("Acrid", "Head", new Vector3(-0.00725F, 2.525F, -0.625F), new Vector3(0F, 180F, 275F), new Vector3(2.5F, 2.5F, 2.5F));
            displaySettings.AddCharacterDisplay("Captain", "ThighR", new Vector3(-0.12F, 0.3225F, 0.0375F), new Vector3(7.5F, 110F, 175F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Railgunner", "Backpack", new Vector3(0.0375F, 0.00325F, -0.14F), new Vector3(359.5F, 180F, 11.35F), new Vector3(0.35F, 0.35F, 0.35F));
            displaySettings.AddCharacterDisplay("Void Fiend", "ForeArmL", new Vector3(0.0825F, 0.05F, 0.0175F), new Vector3(0F, 0F, 200F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Seeker", "Pack", new Vector3(-0.248F, 0.162F, -0.2245F), new Vector3(342.5F, 16F, 60F), new Vector3(0.3F, 0.3F, 0.3F));
            displaySettings.AddCharacterDisplay("False Son", "LowerArmR", new Vector3(-0.05125F, 0.3195F, 0.1375F), new Vector3(3.5F, 180.5F, 188.5F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Chef", "Chest", new Vector3(0.0355F, 0.22875F, 0.11475F), new Vector3(0F, 270F, 315F), new Vector3(0.35F, 0.35F, 0.35F));
        }

        protected override void CreateSettings()
        {
            // Create settings specific to this item
            sizeSetting = spaciousUmbrellaItem.CreateSetting("SIZE", "Radius Increase", 25.0f, "How much should this item increase the size of the teleporter radius? (25.0 = 25% increase)", _valueFormatting: "{0:0.0}%");
            sizeStackingSetting = spaciousUmbrellaItem.CreateSetting("SIZE_STACKING", "Radius Increase Stacking", 25.0f, "How much should additional stacks of this item increase the size of the teleporter radius? (25.0 = 25% increase)", _valueFormatting: "{0:0.0}%");
        }

        public override void FetchSettings()
        {
            // Get item settings
            size = sizeSetting.Value / 100.0f;
            sizeStacking = sizeStackingSetting.Value / 100.0f;

            // Update item texts with new settings
            spaciousUmbrellaItem.UpdateItemTexts();
        }

        void OnHoldoutZoneCalcRadius(ref float _radius, HoldoutZoneController _zone)
        {
            // Number of Spacious Umbrella items
            int count = Utils.GetItemCountForTeam(TeamIndex.Player, spaciousUmbrellaItem.itemDef);

            // Check if players have one of the item
            if (count == 1)
            {
                // Add onto radius
                _radius += size * _zone.baseRadius;
            }

            // Check if players have multiple of the item
            else if (count > 1)
            {
                // Add onto radius
                _radius += (size + Mathf.Log(count + 1, Mathf.Pow(2.0f, 1.0f / sizeStacking)) - sizeStacking) * _zone.baseRadius;
            }
        }
    }
}
