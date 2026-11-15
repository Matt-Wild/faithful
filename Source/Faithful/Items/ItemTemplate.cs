using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class ItemTemplate : ItemBase
    {
        // Buff tied to item
        Buff itemBuff;

        // Item display settings
        ItemDisplaySettings displaySettings;

        // Item settings
        Setting<float> exampleStatSetting;
        Setting<float> exampleStatStackingSetting;

        // Item stats from settings
        float exampleStat;
        float exampleStatStacking;

        // Item Quality settings
        QualitySetting<float> exampleStatQualitySetting;
        QualitySetting<float> exampleStatStackingQualitySetting;

        // Item Quality stats from settings
        QualityValues<float> exampleStatQualityValues = new();
        QualityValues<float> exampleStatStackingQualityValues = new();

        // Constructor
        public ItemTemplate(Toolbox _toolbox) : base(_toolbox, "ITEM_TEMPLATE")
        {
            // Create display settings
            CreateDisplaySettings("TemporalCubeMesh");

            // Create main item
            MainItem = Items.AddItem(new ItemOptions(
                token: token,
                safeName: "Template Item",
                tags: [ItemTag.Damage, ItemTag.Technology], // Example tags
                iconName: "texTemporalCubeIcon",
                modelName: "TemporalCubeMesh")
            {
                SupportsQuality = true, // Change to false if Quality is not supported yet or not desired for this item
                DisplaySettings = displaySettings
            });

            // Create buff tied to item
            itemBuff = Buffs.AddBuff(token, "Template Item", "texBuffTemporalCube", Color.white, false);

            // Create item settings
            CreateSettings();

            // Fetch item settings
            FetchSettings();

            // REGISTER NORMAL ITEM BEHAVIOUR HERE (NOT QUALITY-SPECIFIC BEHAVIOUR)
        }

        public override void QualityConstructor()
        {
            // CREATE QUALITY CONTENT AND BEHAVIOUR HERE (E.G. BUFFS ONLY USED BY QUALITY AND NEW QUALITY BEHAVIOUR HOOKS)
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

            // Add character display settings (adjust these example placements for the new display mesh)
            displaySettings.AddCharacterDisplay("Commando", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Huntress", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Bandit", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("MUL-T", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Engineer", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Turret", "Head", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Walker Turret", "Head", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Artificer", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Mercenary", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("REX", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Loader", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Acrid", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Captain", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Railgunner", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Void Fiend", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Scavenger", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Seeker", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("False Son", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Chef", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Technician", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Operator", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Drifter", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Enforcer", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Acolyte", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
        }

        protected override void CreateSettings()
        {
            // Create settings specific to this item
            exampleStatSetting = MainItem.CreateSetting("EXAMPLE_STAT", "Example Stat", 25.0f, "How much should the first stack of this item increase the example stat? (25.0 = 25 X)", _valueFormatting: "{0:0.0}");
            exampleStatStackingSetting = MainItem.CreateSetting("EXAMPLE_STAT_STACKING", "Example Stat Stacking", 25.0f, "How much should each additional stack of this item increase the example stat? (25.0 = 25 X)", _valueFormatting: "{0:0.0}");

            // Create quality settings for this item if quality is enabled and this item supports quality
            if (MainItem.supportsQuality && Utils.qualityEnabled) CreateQualitySettings();
        }

        protected void CreateQualitySettings()
        {
            // Create quality settings specific to this item
            exampleStatQualitySetting = MainItem.CreateQualitySetting("EXAMPLE_STAT", "Example Stat", 30.0f, 40.0f, 50.0f, 75.0f, "How much should the first stack of this quality increase the example stat? (30.0 = 30 X)", _valueFormatting: "{0:0.0}");
            exampleStatStackingQualitySetting = MainItem.CreateQualitySetting("EXAMPLE_STAT_STACKING", "Example Stat Stacking", 30.0f, 40.0f, 50.0f, 75.0f, "How much should each additional stack of this quality increase the example stat? (30.0 = 30 X)", _valueFormatting: "{0:0.0}");
        }

        public override void FetchSettings()
        {
            // Get item settings
            exampleStat = exampleStatSetting.Value;
            exampleStatStacking = exampleStatStackingSetting.Value;

            // Fetch quality settings for this item if quality is enabled and this item supports quality
            if (MainItem.supportsQuality && Utils.qualityEnabled) FetchQualitySettings();

            // Update item texts with new settings
            MainItem.UpdateItemTexts();
        }

        protected void FetchQualitySettings()
        {
            // Update item quality values
            exampleStatQualityValues.UpdateValues(exampleStatQualitySetting);
            exampleStatStackingQualityValues.UpdateValues(exampleStatStackingQualitySetting);
        }
    }
}
