using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class LuckyFigurine : ItemBase
    {
        // Store display settings
        ItemDisplaySettings displaySettings;

        // Store additional item settings
        Setting<bool> simulacrumBannedSetting;
        Setting<float> itemChanceSetting;
        Setting<float> itemChanceStackingSetting;

        // Store item stats
        float itemChance;
        float itemChanceStacking;

        // Constructor
        public LuckyFigurine(Toolbox _toolbox) : base(_toolbox, "LUCKY_FIGURINE")
        {
            // Create display settings
            CreateDisplaySettings("TemporalCubeMesh");

            // Create startup setting before item registration
            simulacrumBannedSetting = Config.CreateSetting($"ITEM_{token}_SIMULACRUM_BANNED", "Item: Lucky Figurine", "Ban From Simulacrum?", true, "Should this item be banned from Simulacrum?", false, _canRandomise: false, _restartRequired: true);

            // Create item
            MainItem = Items.AddItem(new ItemOptions(
                token: token,
                safeName: "Lucky Figurine",
                tags: [ItemTag.Utility, ItemTag.AIBlacklist, ItemTag.CannotDuplicate, ItemTag.SacrificeBlacklist],
                iconName: "texTemporalCubeIcon",
                modelName: "TemporalCubeMesh")
            {
                Tier = ItemTier.VoidTier2,
                CorruptToken = "ITEM_EXTRASHRINEITEM_NAME",
                SimulacrumBanned = simulacrumBannedSetting.Value,
                SupportsQuality = false,    // Should eventually support quality
                DisplaySettings = displaySettings
            });

            // Create and fetch item settings
            CreateSettings();
            FetchSettings();
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
            displaySettings.AddCharacterDisplay("Commando", "LowerArmL", new Vector3(0.01898F, 0.26776F, 0.00182F), new Vector3(7.69423F, 1.2381F, 2.28152F), new Vector3(0.11F, 0.11F, 0.11F));
            displaySettings.AddCharacterDisplay("Huntress", "Muzzle", new Vector3(0F, -0.02925F, -0.02537F), new Vector3(75F, 270F, 90F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("Bandit", "LowerArmL", new Vector3(0F, 0.1F, 0F), new Vector3(355F, 180F, 180F), new Vector3(0.11F, 0.11F, 0.11F));
            displaySettings.AddCharacterDisplay("MUL-T", "Head", new Vector3(0.39384F, 2.95909F, -1.01878F), new Vector3(0F, 270F, 35F), new Vector3(0.5F, 0.5F, 0.5F));
            displaySettings.AddCharacterDisplay("Engineer", "CannonHeadL", new Vector3(0.141F, 0.382F, 0.1435F), new Vector3(45F, 135F, 270F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Turret", "Neck", new Vector3(0F, -0.3F, 0F), new Vector3(0F, 270F, 0F), new Vector3(0.3F, 0.3F, 0.3F));
            displaySettings.AddCharacterDisplay("Walker Turret", "Neck", new Vector3(0F, -0.3F, 0F), new Vector3(0F, 270F, 0F), new Vector3(0.3F, 0.3F, 0.3F));
            displaySettings.AddCharacterDisplay("Artificer", "CalfR", new Vector3(0.00525F, 0.08556F, 0.03226F), new Vector3(10F, 0F, 355F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Mercenary", "LowerArmL", new Vector3(0.0085F, 0.152F, -0.0075F), new Vector3(0F, 18.25F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("REX", "FootFrontL", new Vector3(0F, -0.034F, 0F), new Vector3(0F, 0F, 270F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Loader", "MechLowerArmL", new Vector3(-0.0025F, 0.64F, -0.0055F), new Vector3(356F, 90F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Acrid", "LowerArmL", new Vector3(0F, 3.725F, 0F), new Vector3(355F, 180F, 0F), new Vector3(1.5F, 1.5F, 1.5F));
            displaySettings.AddCharacterDisplay("Acrid", "LowerArmR", new Vector3(0F, 3.725F, 0F), new Vector3(359.765F, 267.319F, 355.005F), new Vector3(1.5F, 1.5F, 1.5F));
            displaySettings.AddCharacterDisplay("Captain", "MuzzleGun", new Vector3(-0.00353F, -0.00525F, -0.06F), new Vector3(0F, 90F, 90F), new Vector3(0.07F, 0.07F, 0.07F));
            displaySettings.AddCharacterDisplay("Railgunner", "GunScope", new Vector3(-0.075F, -0.1475F, 0.2855F), new Vector3(0F, 90F, 270F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("Void Fiend", "CalfL", new Vector3(-0.0025F, 0.385F, 0.0025F), new Vector3(4.5F, 0F, 350F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Scavenger", "Head", new Vector3(5.13896F, 3.45994F, 0.08489F), new Vector3(338.495F, 358.5428F, 334.8337F), new Vector3(1.5F, 1.5F, 1.5F));
            displaySettings.AddCharacterDisplay("Seeker", "Chest", new Vector3(0F, 0.3275F, -0.075F), new Vector3(0F, 315F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("False Son", "LowerArmL", new Vector3(0.01725F, 0.2145F, 0.0045F), new Vector3(358.75F, 0.0375F, 358.95F), new Vector3(0.2F, 0.2F, 0.2F));
            displaySettings.AddCharacterDisplay("False Son", "LowerArmL", new Vector3(0.02F, 0.36575F, 0.0011F), new Vector3(1.61F, 149.31F, 0.265F), new Vector3(0.2F, 0.2F, 0.2F));
            displaySettings.AddCharacterDisplay("Chef", "OvenDoor", new Vector3(0F, 0.12975F, 0F), new Vector3(0F, 2.5F, 0F), new Vector3(0.05F, 0.1F, 0.05F));
            displaySettings.AddCharacterDisplay("Chef", "OvenDoor", new Vector3(0F, -0.12975F, 0F), new Vector3(0F, 122.5F, 0F), new Vector3(0.05F, 0.1F, 0.05F));
            displaySettings.AddCharacterDisplay("Technician", "Shin.L", new Vector3(0F, 0.505F, 0F), new Vector3(0F, 90F, 0F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("Operator", "BackpackKnobUp", new Vector3(0F, 0F, 0.005F), new Vector3(90F, 0F, 0F), new Vector3(0.03F, 0.03F, 0.03F));
            displaySettings.AddCharacterDisplay("Operator", "BackpackKnobDown", new Vector3(0F, 0F, 0.005F), new Vector3(15F, 90F, 270F), new Vector3(0.03F, 0.03F, 0.03F));
            displaySettings.AddCharacterDisplay("Operator", "LowerArmR", new Vector3(0F, 0F, 0F), new Vector3(0F, 230F, 0F), new Vector3(0.15F, 0.075F, 0.15F));
            displaySettings.AddCharacterDisplay("Drifter", "BagFrontPocket", new Vector3(-0.1F, 0.5125F, 0F), new Vector3(0F, 0F, 352.5F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("Enforcer", "Weapon", new Vector3(-0.36F, 0.1075F, 0F), new Vector3(30F, 270F, 270F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Acolyte", "UpperArmR", new Vector3(0.175F, -0.006F, -0.0075F), new Vector3(36.75F, 185.5F, 279.375F), new Vector3(0.13F, 0.13F, 0.13F));
        }

        protected override void CreateSettings()
        {
            // Create settings specific to this item
            itemChanceSetting = MainItem.CreateSetting("ITEM_CHANCE", "Item Chance", 0.0f, "What should the chance be for this item to drop an additional item when using a Shrine of Chance? (10.0 = 10% chance)", _valueFormatting: "{0:0.0}%");
            itemChanceStackingSetting = MainItem.CreateSetting("ITEM_CHANCE_STACKING", "Item Chance Stacking", 10.0f, "How much additional chance should further stacks of this item provide to drop an additional item when using a Shrine of Chance? (10.0 = 10% chance)", _valueFormatting: "{0:0.0}%");
        }

        public override void FetchSettings()
        {
            // Get item settings
            itemChance = itemChanceSetting.Value;
            itemChanceStacking = itemChanceStackingSetting.Value;

            // Update item texts with new settings
            MainItem.UpdateItemTexts();
        }
    }
}
